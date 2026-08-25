using GeelyEasyToolkit.Services;
using System.Windows;
using System.Windows.Controls;

namespace GeelyEasyToolkit.Views
{
    public partial class LogsView : System.Windows.Controls.UserControl
    {
        public LogsView()
        {
            InitializeComponent();

            LoadLogs();
        }


        private void LoadLogs()
        {
            string logs =
                AppServices.Logger.ReadAll();

            if (string.IsNullOrWhiteSpace(logs))
            {
                LogTextBox.Text =
                    "Журнал пока пуст.";
            }
            else
            {
                LogTextBox.Text =
                    logs;
            }


            LogFileText.Text =
                $"Файл: {AppServices.Logger.LogFilePath}";


            LogTextBox.ScrollToEnd();
        }


        private void RefreshButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            LoadLogs();
        }


        private void CopyButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(
                LogTextBox.Text))
            {
                return;
            }

            System.Windows.Clipboard.SetText(
                LogTextBox.Text);

            AppServices.Logger.Log(
                "Содержимое журнала скопировано в буфер обмена.");
        }


        private void ClearButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            System.Windows.MessageBoxResult result =
                System.Windows.MessageBox.Show(
                    "Очистить журнал?\n\n" +
                    "Все текущие записи будут удалены.",
                    "Очистка журнала",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Warning);

            if (result != System.Windows.MessageBoxResult.Yes)
                return;


            AppServices.Logger.Clear();

            LogTextBox.Text =
                "Журнал очищен.";

            AppServices.Logger.Log(
                "Журнал очищен пользователем.");

            LoadLogs();
        }
    }
}