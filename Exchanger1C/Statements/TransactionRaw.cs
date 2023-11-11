using System.Collections.Generic;


namespace Exchanger
{
    internal class TransactionRaw
    {
        private Dictionary<string, string> values = new Dictionary<string, string>();

        private List<string> source = new List<string>();

        public TransactionRaw() { }

        public void AddLine(string line)
        {
            source.Add(line);
            string key = line.ParseKey();
            string value = line.ParseValue();
            if (key.Length > 0 && value.Length > 0 && !values.ContainsKey(key)) values.Add(key, value);
        }


        public string GetOrEmpty(string key)
        {
            return values.GetOrEmpty(key);
        }

        public string GetOrEmpty(string key1, string key2)
        {
            return values.GetOrEmpty(key1, key2);
        }

        public string GetSource()
        {
            return string.Join("\n", source);
        }

        public Transaction ToTransaction(string ownerAccount)
        {
            var item = new Transaction();

            var docType = GetOrEmpty("СекцияДокумент");
            var docNumber = GetOrEmpty("Номер").ParseInt();
            var docDate = GetOrEmpty("Дата").ParseDate();
            item.DocumentInfo = string.Join(" ", docType.toShortDocTypeString(), docNumber.toNumberedString(), docDate.toFromDateString());

            if (GetOrEmpty("ПлательщикСчет", "ПлательщикРасчСчет") == ownerAccount)
            {
                item.DateOut = GetOrEmpty("ДатаСписано").ParseDate();
                item.Name = GetOrEmpty("Получатель1", "Получатель");
                item.Account = GetOrEmpty("ПолучательРасчСчет", "ПолучательСчет");
                var kpp = GetOrEmpty("ПолучательКПП");
                if (kpp.Length == 9) item.InnKpp = $"{GetOrEmpty("ПолучательИНН")}/\n{kpp}";
                else item.InnKpp = GetOrEmpty("ПолучательИНН");
                item.Bank = string.Join(" ",
                    new List<string>() { GetOrEmpty("ПолучательБИК"), GetOrEmpty("ПолучательБанк1", "ПолучательБанк") }
                    );
                item.Debet = GetOrEmpty("Сумма").ParseAmount();
                item.Credit = null;
            }
            if (GetOrEmpty("ПолучательСчет", "ПолучательРасчСчет") == ownerAccount)
            {
                item.DateOut = GetOrEmpty("ДатаПоступило").ParseDate();
                item.Name = GetOrEmpty("Плательщик1", "Плательщик");
                item.Account = GetOrEmpty("ПлательщикРасчСчет", "ПлательщикСчет");
                var kpp = GetOrEmpty("ПлательщикКПП");
                if (kpp.Length == 9) item.InnKpp = $"{GetOrEmpty("ПлательщикИНН")}/\n{kpp}";
                else item.InnKpp = GetOrEmpty("ПлательщикИНН");
                item.Bank = string.Join(" ",
                    new List<string>() { GetOrEmpty("ПлательщикБИК"), GetOrEmpty("ПлательщикБанк1", "ПлательщикБанк") }
                    );
                item.Debet = null;
                item.Credit = GetOrEmpty("Сумма").ParseAmount();
            }
            item.Description = GetOrEmpty("НазначениеПлатежа");
            item.SourceRaw = GetSource();

            return item;
        }
    }
}
