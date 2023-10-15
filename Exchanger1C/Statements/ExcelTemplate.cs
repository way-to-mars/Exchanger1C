using ClosedXML.Report;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;


namespace Exchanger
{
    internal class ExcelTemplate
    {
        private ExcelTemplate() { }

        public static XLTemplate fromStatementReader(StatementReader reader)
        {
            string tmp = Path.GetTempFileName();

            var template = new XLTemplate("template.xlsx");

            template.AddVariable("OwnerName", reader.name);
            template.AddVariable("DateStart", reader.dateStart);
            template.AddVariable("DateEnd", reader.dateEnd);
            template.AddVariable("OwnerInn", reader.inn);
            template.AddVariable("OwnerAccount", reader.account);
            template.AddVariable("OwnerBankName", reader.bankName);
            template.AddVariable("OwnerBankBik", reader.bik);
            template.AddVariable("OwnerBankKS", reader.bankKS);
            template.AddVariable("BalanceStart", reader.balanceStart.ParseAmount());
            template.AddVariable("BalanceEnd", reader.balanceEnd.ParseAmount());

            var wrapper = new Wrapper();
            wrapper.Transactions = reader.Transactions;

            template.AddVariable(wrapper);
            template.Generate();

            var wb = template.Workbook;
            var sheet = wb.Worksheet("statement");
            sheet.Row(10 + wrapper.Size).Height = 15;
            sheet.Cell("B2").SetActive(true);

            // set document.xlsx properties
            foreach (var prop in wb.CustomProperties) { prop.Value = null; }
            wb.Author = reader.bankName;
            wb.Properties.Author = reader.bankName;
            wb.Properties.Title = reader.name;
            DateTime? dateEnd = reader.dateEnd.ParseDate();
            if (dateEnd != null)
            {
                wb.Properties.Created = (DateTime)dateEnd;
                wb.Properties.Modified = (DateTime)dateEnd;
            }
            wb.Properties.Company = null;
            wb.Properties.Manager = null;
            wb.Properties.LastModifiedBy = null;
            wb.CustomProperties.Delete("PrintDate");  // seems to be useless
            sheet.Author = null;

            return template;
        }

        public static void WriteFile(XLTemplate template, string fileName) {
            template.SaveAs(fileName);
            Process.Start(new ProcessStartInfo(fileName) { UseShellExecute = true });
        }

        public static string GenerateFileName(StatementReader reader) {
            string firmName = reader.name.ShortenFirmName();
            string dateStart = reader.dateStart;
            string dateEnd = reader.dateEnd;
            return $"{firmName} с {dateStart} по {dateEnd}.xlsx";
        }

        public class Wrapper
        {
            public int Size { get { return Transactions.Count; } }
            public List<Transaction> Transactions { get; set; }
        }

    }

}
