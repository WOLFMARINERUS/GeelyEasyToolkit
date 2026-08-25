using GeelyEasyToolkit.Services;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace GeelyEasyToolkit.Views
{
    public partial class LogsView : System.Windows.Controls.UserControl
    {
        private FileSystemWatcher? _logWatcher;
        private DispatcherTimer? _updateDebounceTimer;
        private bool _updatePending = false;

        public LogsView()
        {
            InitializeComponent();

            LoadLogs();
            SetupLogWatcher();
        }


        private void SetupLogWatcher()
        {
            try
            {
                string logFolder = Path.GetDirectoryName(AppServices.Logger.LogFilePath);
                string logFileName = Path.GetFileName(AppServices.Logger.LogFilePath);

                if (string.IsNullOrEmpty(logFolder))
                    return;

                _logWatcher = new FileSystemWatcher(logFolder)
                {
                    Filter = logFileName,
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
                };

                _logWatcher.Changed += LogWatcher_Changed;
                _logWatcher.EnableRaisingEvents = true;

                // Таймер для дебаунсирования обновлений
                _updateDebounceTimer = new DispatcherTimer()
                {
                    Interval = TimeSpan.FromMilliseconds(500)
                };
                _updateDebounceTimer.Tick += (s, e) =>
                {
                    _updateDebounceTimer.Stop();
                    if (_updatePending)
                    {
                        _updatePending = false;
                        LoadLogs();
                    }
                };
            }
            catch
            {
                // Если не удалось установить watcher, это не критично
            }
        }


        private void LogWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            // Отмечаем, что нужно обновление, но не обновляем сразу
            _updatePending = true;

            // Перезапускаем таймер дебаунсирования
            if (_updateDebounceTimer != null)
            {
                _updateDebounceTimer.Stop();
                _updateDebounceTimer.Start();
            }
            else
            {
                // Если таймер не инициализирован, обновляем сразу
                Dispatcher.Invoke(LoadLogs);
            }
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


        ~LogsView()
        {
            if (_logWatcher != null)
            {
                _logWatcher.EnableRaisingEvents = false;
                _logWatcher.Dispose();
            }

            if (_updateDebounceTimer != null)
            {
                _updateDebounceTimer.Stop();
            }
        }
    }
}
