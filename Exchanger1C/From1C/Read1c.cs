using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;


namespace Exchanger
{
    internal class Read1c
    {
        public enum State_enum
        {
            OK,
            IO_EXCEPTION,
            WRONG_FORMAT,
            EMPTY_FILE,
        }
        public static State_enum State;

        private RubleKop _sum;
        private int _count_payments = 0;
        private readonly List<PaymentsToListView> _payments = new List<PaymentsToListView>();
        private string _payerName;
        private string _payerAccount;
        private string _payerBankName;
        private string _payerBankCity;
        private string _payerBankKS;
        private string _payerBankBik;
        private string[] _inputFileLines;
        private static readonly char[] trimChars = { ' ', '\n' };

        public string TotalSum => _sum != null ? _sum.ToString() : "n/a";
        public string TotalCount => _count_payments.ToString();
        public string PayerName => _payerName ?? "n/a";
        public string PayerAccount => _payerAccount ?? "n/a";
        public string PayerBankName => _payerBankName ?? "n/a";
        public string PayerBankCity => _payerBankCity ?? "n/a";
        public string PayerBankKS => _payerBankKS ?? "n/a";
        public string PayerBankBik => _payerBankBik ?? "n/a";
        public ReadOnlyCollection<PaymentsToListView> PaymentsList => _payments.AsReadOnly();

        private Read1c() { }

        public static Read1c FromFile(string filename)
        {
            string[] file_lines;
            try
            {
                file_lines = File.ReadAllLines(filename, Encoding.GetEncoding("windows-1251"));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                State = State_enum.IO_EXCEPTION;
                return null;
            }
            if (file_lines.Length == 0)
            {
                State = State_enum.EMPTY_FILE;
                return null;
            }
            if (!CheckFormat(file_lines))
            {
                State = State_enum.WRONG_FORMAT;
                return null;
            }

            Read1c read1c = new Read1c
            {
                _inputFileLines = file_lines,
                _sum = new RubleKop(0, 0)
            };
            bool isName = false, isAccount = false, isBankName = false, isBankCity = false, isBankKS = false, isBankBik = false;
            // read Payer info and count sum of all payments
            foreach (string line in file_lines)
            {
                if (line.StartsWith("Сумма="))
                {
                    read1c._sum += RubleKop.FromString(line.Split('=')[1].Trim(trimChars));
                    read1c._count_payments++;
                }

                if (!isName && line.StartsWith("Плательщик1="))
                {
                    read1c._payerName = line.Split('=')[1].Trim(trimChars);
                    isName = true;
                    continue;
                }

                if (!isAccount && line.StartsWith("ПлательщикРасчСчет="))
                {
                    read1c._payerAccount = line.Split('=')[1].Trim(trimChars);
                    isAccount = true;
                    continue;
                }

                if (!isBankName && line.StartsWith("ПлательщикБанк1="))
                {
                    read1c._payerBankName = line.Split('=')[1].Trim(trimChars);
                    isBankName = true;
                    continue;
                }

                if (!isBankCity && line.StartsWith("ПлательщикБанк2="))
                {
                    read1c._payerBankCity = line.Split('=')[1].Trim(trimChars);
                    isBankCity = true;
                    continue;
                }

                if (!isBankKS && line.StartsWith("ПлательщикКорсчет="))
                {
                    read1c._payerBankKS = line.Split('=')[1].Trim(trimChars);
                    isBankKS = true;
                    continue;
                }

                if (!isBankBik && line.StartsWith("ПлательщикБИК="))
                {
                    read1c._payerBankBik = line.Split('=')[1].Trim(trimChars);
                    isBankBik = true;
                    continue;
                }
            }

            PaymentsToListView payment = new PaymentsToListView();
            bool isInPayment = false;
            // read payments
            foreach (string line in file_lines)
            {
                if (line.StartsWith("СекцияДокумент=Платежное поручение"))
                {
                    payment = new PaymentsToListView();
                    isInPayment = true;
                    continue;
                }
                if (line.StartsWith("КонецДокумента"))
                {
                    read1c._payments.Add(payment);
                    isInPayment = false;
                    continue;
                }
                if (isInPayment)
                {
                    payment.AddString(line);
                }

            }
            return read1c;
        }

        public static bool CheckFormat(string[] lines)
        {
            String internalTag = "1CClientBankExchange";
            bool isInternalFlag = false;

            String encodingTag = "Кодировка=Windows";
            bool isEncodingTag = false;

            String senderTag = "Отправитель=Бухгалтерия";
            bool isSenderTag = false;

            foreach (string str in lines)
            {
                if (isInternalFlag && isEncodingTag && isSenderTag) return true;

                if (!isInternalFlag)
                    if (str.Equals(internalTag)) isInternalFlag = true;

                if (!isEncodingTag)
                    if (str.Equals(encodingTag)) isEncodingTag = true;

                if (!isSenderTag)
                    if (str.StartsWith(senderTag)) isSenderTag = true;
            }
            return isInternalFlag && isEncodingTag && isSenderTag;
        }


        public string[] ApplyChanges(bool isNewDate,
                                bool isNewNumbers,
                                bool isNewRequisites,
                                string newDate,
                                int newNumerationNumber,
                                string newAccount,
                                string newBankName,
                                string newBankCity,
                                string newBankKS,
                                string newBankBik)
        {
            string[] result = new string[_inputFileLines.Length];

            for (int i = 0, pNumber = newNumerationNumber; i < result.Length; ++i)
            {
                string line = _inputFileLines[i];

                if (isNewDate)
                {
                    if (line.StartsWith("ДатаНачала="))
                    {
                        result[i] = "ДатаНачала=" + newDate;
                        continue;
                    }
                    if (line.StartsWith("ДатаКонца="))
                    {
                        result[i] = "ДатаКонца=" + newDate;
                        continue;
                    }
                    if (line.StartsWith("Дата="))
                    {
                        result[i] = "Дата=" + newDate;
                        continue;
                    }
                }

                if (isNewNumbers)
                {
                    if (line.StartsWith("Номер="))
                    {
                        result[i] = "Номер=" + pNumber.ToString();
                        pNumber++;
                        continue;
                    }
                }

                if (isNewRequisites)
                {

                    // Рассчетный счет
                    if (line.StartsWith("РасчСчет="))
                    {
                        result[i] = "РасчСчет=" + newAccount;
                        continue;
                    }
                    if (line.StartsWith("ПлательщикСчет="))
                    {
                        result[i] = "ПлательщикСчет=" + newAccount;
                        continue;
                    }
                    if (line.StartsWith("ПлательщикРасчСчет="))
                    {
                        result[i] = "ПлательщикРасчСчет=" + newAccount;
                        continue;
                    }

                    // Название банка
                    if (line.StartsWith("ПлательщикБанк1="))
                    {
                        result[i] = "ПлательщикБанк1=" + newBankName;
                        continue;
                    }

                    // Город Банка
                    if (line.StartsWith("ПлательщикБанк2="))
                    {
                        result[i] = "ПлательщикБанк2=" + newBankCity;
                        continue;
                    }

                    // БИК Банка
                    if (line.StartsWith("ПлательщикБИК="))
                    {
                        result[i] = "ПлательщикБИК=" + newBankBik;
                        continue;
                    }

                    // к/с Банка
                    if (line.StartsWith("ПлательщикКорсчет="))
                    {
                        result[i] = "ПлательщикКорсчет=" + newBankKS;
                        continue;
                    }
                }

                result[i] = String.Copy(line);
            }

            return result;
        }
    }
}
