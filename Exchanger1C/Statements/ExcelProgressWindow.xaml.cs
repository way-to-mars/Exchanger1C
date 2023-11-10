using Exchanger;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using static Exchanger1C.CommonUtils.FileUsageCheck;


namespace Exchanger1C.Statements
{
    public partial class ExcelProgressWindow : Window
    {

        private readonly string SourceFileName;
        private readonly bool ForceNotepad;
        private static readonly int UpdDelay = 3;
        public bool Result {  get; private set; }

        public ExcelProgressWindow(string SourceFileName, bool ForceNotepad = false)
        {
            this.SourceFileName = SourceFileName;
            this.ForceNotepad = ForceNotepad;
            this.Result = false;
            InitializeComponent();
        }

        public async Task UpdateProgress(int value, string txt) {
            TxtBlock.Text = txt;
            UndefinedProgressBar.Value = value;
            await Task.Delay(UpdDelay);
            return;
        }

        private void OpenNotePad() {
            Process.Start("notepad.exe", SourceFileName);
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {

            await UpdateProgress(5, "Открываем исходный файл в блокноте");
            if (ForceNotepad) OpenNotePad();

            await UpdateProgress(10, "Чтение исходного файла");
            StatementReader reader = StatementReader.FromFile(SourceFileName);
            if (reader == null)
            {
                await ClosingScenario($"Ошибка чтения файла\n{SourceFileName}");
                return;
            }

            try
            {
                await UpdateProgress(25, "Создание таблицы");
                var template = ExcelTemplate.FromStatementReader(reader);
                if (template == null)
                {
                    await ClosingScenario($"Ошибка создания таблицы из шаблона");
                    return;
                }

                Int32 unixTimestamp = (Int32)(DateTime.Now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                string defaultName = unixTimestamp.ToString("X") + " " + ExcelTemplate.GenerateFileName(reader);

                // var outputFilename = SaveExcelDialog(defaultName);
                var outputFilename = Path.Combine(Path.GetTempPath(), defaultName);
                if (outputFilename.Length == 0) return;

                await UpdateProgress(50, "Сохранение таблицы в файл");

                ExcelTemplate.WriteFile(template, outputFilename);
                await UpdateProgress(99, "Открываем файл эксель");
                Process.Start(new ProcessStartInfo(outputFilename) { UseShellExecute = true });
                await UpdateProgress(100, "Открываем файл эксель");
                await UntilBusyAsync(outputFilename);
   
                Close();

                DeleteWhenFree(outputFilename);
            }
            catch (Exception ex)
            {
                await ClosingScenario($"{ex.Message}");
                Close();
                return;
            }
            
            Result = true;
        }

        private async Task<bool> ClosingScenario(string message) {
            if (ForceNotepad) OpenNotePad();
            await UpdateProgress(99, message);
            ClsBtn.Visibility = Visibility.Visible;
            for (int i = 10; i > 0; i--) {
                ClsBtn.Content = $"Закрыть ({i} сек)";
                await Task.Delay(1000);
            }
            Close();
            return true;
        }

        private void ClsBtn_Click(object sender, RoutedEventArgs e) => this.Close();

    }
}
