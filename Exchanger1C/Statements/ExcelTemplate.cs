using ClosedXML.Report;
using Exchanger1C;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;


namespace Exchanger
{
    internal class ExcelTemplate
    {
        private ExcelTemplate() { }

        public static XLTemplate FromStatementReader(StatementReader reader)
        {
            XLTemplate template;

            try
            {
                string templateFullname = Path.Combine(App.AppPath, "template.xlsx");
                template = new XLTemplate(templateFullname);
            }
            catch
            {
                MessageBox.Show($"Ошибка чтения файла-шаблона template.xlsx\nФайл не найден или поврежден",
                        "Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                return null;
            }


            var wrapper = new Wrapper { Transactions = reader.Transactions };

            if (wrapper.IsNotEmpty) template.AddVariable(wrapper);
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

            try
            {
                template.Generate();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"exception = {ex}",
                        "ExcelTemplate.FromStatementReader",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
            }

            var wb = template.Workbook;
            var sheet = wb.Worksheet("statement");
            sheet.Row(10 + wrapper.Size).Height = 15;
            if (wrapper.IsEmpty) sheet.Row(10).Delete();    

            // set document.xlsx properties, trying to delete private information
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
            sheet.Author = null;

            return template;
        }

        public static void WriteFile(XLTemplate template, string fileName)
        {
            if (template == null || fileName == null) return;
            template.SaveAs(fileName);
            Process.Start(new ProcessStartInfo(fileName) { UseShellExecute = true });
        }

        public static string GenerateFileName(StatementReader reader)
        {
            string firmName = reader.name.ShortenFirmName();
            string bankShortName = BankShorts.findShortName(reader.bankName);
            string dateStart = " c " + reader.dateStart;
            string dateEnd = " по " + reader.dateEnd;
            if (bankShortName.Length > 0) return $"{firmName} {bankShortName} {dateStart}{dateEnd}.xlsx";
            return $"{firmName}{dateStart}{dateEnd}.xlsx";
        }

        public class Wrapper
        {
            public int Size { get { return Transactions.Count; } }

            public bool IsEmpty { get { return Size == 0; } }
            public bool IsNotEmpty { get { return Size > 0; } }
            public List<Transaction> Transactions { get; set; }
        }

    }

}
