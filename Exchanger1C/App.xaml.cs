using Exchanger1C.Statements;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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
                        CreateExcelStatement(FileName, true);                        
                        Debug.WriteLine($"APP: Shutdown");
                        this.Shutdown();
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

            // if the File starts with "1CClientBankExchange" check the exact type of it
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

        public static bool CreateExcelStatement(string SourceFileName, bool ForceNotepad = false) {
            var progressWindow = new ExcelProgressWindow(SourceFileName, ForceNotepad);
            progressWindow.ShowDialog();

            return progressWindow.Result;
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
