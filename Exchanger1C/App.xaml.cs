using Exchanger1C.Statements;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Exchanger
{
    public partial class App : Application
    {
        public static string FileName = null;

        public static string AppPath { get { return ExecutablePath(); } }

        public enum FileType
        {
            txt1C_to_kl,
            kl_to_txt1C,
            other_type,
            none,
        }

        public static FileType checkedType = FileType.none;

        public App() { Startup += new StartupEventHandler(App_Startup); }

        void App_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                Debug.WriteLine($"APP: Checking the type of input file. Args = {String.Join(" ", Environment.GetCommandLineArgs())}");
                FileName = GetFirstArgAsFilename();
                checkedType = CheckInputFileType(FileName);
                Debug.WriteLine($"APP: type = {checkedType}");
                switch (checkedType)
                {
                    case FileType.txt1C_to_kl:
                        break;
                    case FileType.kl_to_txt1C:
                        if (CreateExcelStatement(FileName, ForceNotepad: true))
                        {
                            Debug.WriteLine($"APP: Shutdown");
                            this.Shutdown();
                        }
                        break;
                    case FileType.other_type:
                        Process.Start("notepad.exe", Environment.GetCommandLineArgs()[1]);
                        this.Shutdown();
                        break;
                    case FileType.none:
                        this.Shutdown();
                        break;
                    default: throw new Exception("Illegal FileType!");
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                Process.Start("notepad.exe", Environment.GetCommandLineArgs()[1]);
                this.Shutdown();
            }
        }

        public static FileType CheckInputFileType(string file)
        {
            if (file == null) return FileType.none;

            var fastLines = File.ReadLines(file, Encoding.GetEncoding("windows-1251"));

            if (fastLines.Count() == 0 || !fastLines.First().StartsWith("1CClientBankExchange")) return FileType.other_type;

            // if the File starts with "1CClientBankExchange" check the exactly type of it
            string[] lines = File.ReadAllLines(file, Encoding.GetEncoding("windows-1251"));
            foreach (string line in lines)
            {
                if (line.StartsWith("Отправитель="))
                {
                    if (line.StartsWith("Отправитель=Бухгалтерия")) return FileType.txt1C_to_kl;
                    else return FileType.kl_to_txt1C;
                }
            }

            return FileType.other_type;
        }

        private string GetFirstArgAsFilename()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args != null && args.Length > 1)
            {
                Debug.WriteLine($"Found an argument {args[1]}");
                System.IO.FileInfo fi = null;
                try
                {
                    fi = new System.IO.FileInfo(args[1]);
                }
                catch (ArgumentException)
                {
                    Debug.WriteLine("Argument exception");
                    return null;
                }
                catch (System.IO.PathTooLongException)
                {
                    Debug.WriteLine("PathTooLongException exception");
                    return null;
                }
                catch (NotSupportedException)
                {
                    Debug.WriteLine("NotSupportedException exception");
                    return null;
                }
                if (ReferenceEquals(fi, null))
                {
                    Debug.WriteLine("File name is not valid");
                    return null;
                }
                else
                {
                    string fileName = fi.FullName;
                    Debug.WriteLine($"Determined a correct file name: {fileName}");
                    return fileName;
                }
            }
            else
            {
                return null;
            }
        }

        public static bool CreateExcelStatement(string SourceFileName, bool ForceNotepad = false)
        {
            Debug.WriteLine($"APP.CreateExcelStatement: reading a statement from {SourceFileName}");

            var progressWindow = new ExcelProgressWindow();
            progressWindow.UpdateProgress(5, "Чтение исходного файла");
            progressWindow.Show();

            StatementReader reader = StatementReader.FromFile(SourceFileName);

            if (ForceNotepad) Process.Start("notepad.exe", SourceFileName);

            if (reader == null)
            {
                if (!ForceNotepad)
                {
                    var res = MessageBox.Show($"Ошибка чтения файла\n{SourceFileName}.\nОткрыть в блокноте?",
                        "Экспорт выписки в формат Excel",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Error);
                    if (res == MessageBoxResult.Yes)
                        Process.Start("notepad.exe", SourceFileName);
                }
                progressWindow.Close();
                return false;
            }
            progressWindow.UpdateProgress(25, "Применение шаблона данных");

            try
            {
                Debug.WriteLine($"APP.CreateExcelStatement: creating xlsx object from reader");
                var template = ExcelTemplate.FromStatementReader(reader);
                if (template == null) return false;

                progressWindow.UpdateProgress(50, "Создание электронной таблицы");

                Int32 unixTimestamp = (Int32)(DateTime.Now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

                string defaultName = unixTimestamp.ToString("X") + " " + ExcelTemplate.GenerateFileName(reader);

                // var outputFilename = SaveExcelDialog(defaultName);
                var outputFilename = Path.Combine(Path.GetTempPath(), defaultName);
                if (outputFilename.Length == 0) return false;

                Debug.WriteLine($"APP.CreateExcelStatement: saving xlsx object to {outputFilename}");
                ExcelTemplate.WriteFile(template, outputFilename);
                progressWindow.UpdateProgress(99, "Запуск Excel");
                Process.Start(new ProcessStartInfo(outputFilename) { UseShellExecute = true });
                progressWindow.Close();

                DeleteWhenClosed(outputFilename);
                return true;
            }
            catch (Exception)
            {
                progressWindow.Close();
                return false;
            }            
        }

        private static void DeleteWhenClosed(string fileName) {
            Thread.Sleep(10000);
            while (true) {                
                if (!File.Exists(fileName)) {
                    Debug.WriteLine($"File {fileName} doesn't exist anymore");
                    return;
                }
                try
                {
                    File.Delete(fileName);
                }
                catch (IOException ex) {
                    Debug.WriteLine($"File {fileName} is busy");
                    Thread.Sleep(1000);
                }
                catch(Exception ex)
                {
                    Debug.WriteLine($"Other exception: {ex}");
                    return;
                }
            }
        
        }

        private static string SaveExcelDialog(string defaultName = "")
        {
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = defaultName,
                DefaultExt = ".xlsx",
                Filter = "Файл эксель|*.xlsx",
                AddExtension = true,
                Title = "Экспорт выписки в формат Excel",
                OverwritePrompt = true,
                ValidateNames = true
            };

            Nullable<bool> result = saveFileDialog.ShowDialog();
            if (result == null || result == false) return string.Empty;

            return saveFileDialog.FileName;
        }

        private static string ExecutablePath()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            try
            {
                foreach (Assembly assem in assemblies)
                    if (assem.GetName().Name == "Exchanger1C")
                        return Path.GetDirectoryName(assem.Location);
            }
            catch (Exception ex) { Debug.WriteLine($"App.ExecutablePath: {ex}"); }
            return Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
        }

    }
}
