using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exchanger
{
    internal class StatementReader
    {
        private readonly List<TransactionRaw> _transactions = new List<TransactionRaw>();
        private readonly Dictionary<string, string> _mainInfo = new Dictionary<string, string>();

        public string name { get { return FindFieldByAccount(_mainInfo.GetOrEmpty("РасчСчет"), "Плательщик1", "Плательщик", "Получатель1", "Получатель"); } }
        public string inn { get { return FindFieldByAccount(_mainInfo.GetOrEmpty("РасчСчет"), "ПлательщикИНН", "ПолучательИНН"); } }
        public string kpp { get { return FindFieldByAccount(_mainInfo.GetOrEmpty("РасчСчет"), "ПлательщикКПП", "ПолучательКПП"); } }
        public string bik { get { return FindFieldByAccount(_mainInfo.GetOrEmpty("РасчСчет"), "ПлательщикБИК", "ПолучательБИК"); } }
        public string bankKS { get { return FindFieldByAccount(_mainInfo.GetOrEmpty("РасчСчет"), "ПлательщикКорсчет", "ПолучательКорсчет"); } }
        public string bankName { get { return FindFieldByAccount(_mainInfo.GetOrEmpty("РасчСчет"), "ПлательщикБанк", "ПлательщикБанк1", "ПолучательБанк1", "ПолучательБанк1"); } }
        public string account { get { return _mainInfo.GetOrEmpty("РасчСчет"); } }
        public string dateStart { get { return _mainInfo.GetOrEmpty("ДатаНачала"); } }
        public string dateEnd { get { return _mainInfo.GetOrEmpty("ДатаКонца"); } }
        public string balanceStart { get { return _mainInfo.GetOrEmpty("НачальныйОстаток"); } }
        public string balanceEnd { get { return _mainInfo.GetOrEmpty("КонечныйОстаток"); } }
        public List<Transaction> Transactions { get { return _transactions.Select(raw => raw.ToTransaction(account)).ToList(); } }

        private StatementReader() { }  // A private void constructor

        public static StatementReader FromFile(string filename)
        {
            string[] file_lines;
            try
            {
                file_lines = File.ReadAllLines(filename, Encoding.GetEncoding("windows-1251"));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return null;
            }

            StatementReader reader = new StatementReader();

            foreach (string line in file_lines)
            {
                if (line.StartsWith("СекцияДокумент")) break;
                string key = line.ParseKey();
                string value = line.ParseValue();
                switch (key)
                {
                    case "ДатаНачала":
                    case "РасчСчет":
                    case "НачальныйОстаток":
                        reader._mainInfo.AddOnce(key, value);
                        break;
                    case "ДатаКонца":
                    case "КонечныйОстаток":
                        reader._mainInfo.AddOrUpdate(key, value);
                        break;
                }
            }

            // read _transactions
            TransactionRaw transaction = new TransactionRaw();  // for null-safety purposes
            bool isInTransaction = false;
            foreach (string line in file_lines)
            {
                if (line.StartsWith("СекцияДокумент="))
                {
                    transaction = new TransactionRaw();
                    isInTransaction = true;
                    transaction.AddLine(line);
                    continue;
                }
                if (line.StartsWith("КонецДокумента"))
                {
                    transaction.AddLine(line);
                    reader._transactions.Add(transaction);
                    isInTransaction = false;
                    continue;
                }
                if (isInTransaction)
                {
                    transaction.AddLine(line);
                }
            }

            return reader;
        }

        private string FindFieldByAccount(string account, string payerKey, string recieverKey)
        {
            if (account == null) return "";
            foreach (TransactionRaw ts in _transactions)
            {
                if (ts.GetOrEmpty("ПлательщикРасчСчет", "ПлательщикСчет") == account)
                {
                    string fieldValue = ts.GetOrEmpty(payerKey);
                    if (fieldValue.Length > 0) return fieldValue;
                }
                if (ts.GetOrEmpty("ПолучательРасчСчет", "ПолучательСчет") == account)
                {
                    string fieldValue = ts.GetOrEmpty(recieverKey);
                    if (fieldValue.Length > 0) return fieldValue;
                }
            }
            return "";
        }

        private string FindFieldByAccount(string account, string payerKey1, string payerKey2, string recieverKey1, string recieverKey2)
        {
            if (account == null) return "";
            foreach (TransactionRaw ts in _transactions)
            {
                if (ts.GetOrEmpty("ПлательщикРасчСчет", "ПлательщикСчет") == account)
                {
                    string fieldValue = ts.GetOrEmpty(payerKey1, payerKey2);
                    if (fieldValue.Length > 0) return fieldValue;
                }
                if (ts.GetOrEmpty("ПолучательРасчСчет", "ПолучательСчет") == account)
                {
                    string fieldValue = ts.GetOrEmpty(recieverKey1, recieverKey2);
                    if (fieldValue.Length > 0) return fieldValue;
                }
            }
            return "";
        }

        public RubleKop SumIncoming()
        {
            RubleKop sum = RubleKop.ZERO;
            string ownerAccount = account;

            foreach (TransactionRaw ts in _transactions)
            {
                if (ts.GetOrEmpty("ПолучательРасчСчет", "ПолучательСчет") == ownerAccount)
                    sum += RubleKop.FromString(ts.GetOrEmpty("Сумма"));
            }

            return sum;
        }

        public RubleKop SumOutcoming()
        {
            RubleKop sum = RubleKop.ZERO;
            string ownerAccount = account;

            foreach (TransactionRaw ts in _transactions)
            {
                if (ts.GetOrEmpty("ПлательщикРасчСчет", "ПлательщикСчет") == ownerAccount)
                    sum += RubleKop.FromString(ts.GetOrEmpty("Сумма"));
            }

            return sum;
        }
    }

}
