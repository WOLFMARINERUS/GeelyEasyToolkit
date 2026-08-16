using GeelyEasyToolkit.Services;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;  // ← ВАЖНО: для KeyEventArgs

namespace GeelyEasyToolkit.Views
{
    public partial class DeveloperView : System.Windows.Controls.UserControl
    {
        public DeveloperView()
        {
            try
            {
                InitializeComponent();

                // Подписываемся на события
                AppServices.DeviceMonitor.ConnectionChanged += DeviceMonitor_ConnectionChanged;

                // Загружаем начальные данные
                Loaded += DeveloperView_Loaded;

                // Подписываемся на событие Unloaded
                this.Unloaded += DeveloperView_Unloaded;
            }
            catch (Exception ex)
            {
                ConsoleOutput?.AppendText($"❌ Ошибка инициализации: {ex.Message}\n");
                Debug.WriteLine($"[DeveloperView] Ошибка: {ex.Message}");
            }
        }

        // ==================== ЗАГРУЗКА ====================

        private void DeveloperView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                UpdateInfo();
            }
            catch (Exception ex)
            {
                ConsoleOutput?.AppendText($"❌ Ошибка загрузки: {ex.Message}\n");
                Debug.WriteLine($"[DeveloperView] Load error: {ex.Message}");
            }
        }

        // ==================== ОБНОВЛЕНИЕ ИНФОРМАЦИИ ====================

        private void UpdateInfo()
        {
            try
            {
                // Версия приложения
                VersionText.Text = GetAppVersion();

                // Профиль
                var profile = AppServices.Profiles.GetCurrentProfile();
                ProfileText.Text = profile?.Name ?? "Не выбран";

                // ADB статус
                bool adbAvailable = AppServices.Adb.IsAdbAvailable();
                AdbStatusText.Text = adbAvailable ? "✅ Доступен" : "❌ Не найден";
                AdbStatusText.Foreground = adbAvailable ?
                    System.Windows.Media.Brushes.LightGreen :
                    System.Windows.Media.Brushes.OrangeRed;

                // Устройство
                var deviceInfo = AppServices.Adb.GetDeviceInfo();
                DeviceText.Text = deviceInfo?.Connected == true
                    ? $"{deviceInfo.Model ?? "Неизвестно"} ({deviceInfo.Manufacturer ?? "Неизвестно"})"
                    : "❌ Не подключено";

                // Репозиторий
                RepositoryStatusText.Text = "✅ GitHub";
            }
            catch (Exception ex)
            {
                ConsoleOutput?.AppendText($"❌ Ошибка обновления: {ex.Message}\n");
                Debug.WriteLine($"[DeveloperView] Update error: {ex.Message}");
            }
        }

        private string GetAppVersion()
        {
            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var version = assembly.GetName().Version;
                return version != null ? $"{version.Major}.{version.Minor}.{version.Build}" : "1.0.0";
            }
            catch
            {
                return "1.0.0";
            }
        }

        // ==================== ОБРАБОТЧИКИ СОБЫТИЙ ====================

        private void DeviceMonitor_ConnectionChanged(bool connected)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    UpdateInfo();
                    ConsoleOutput?.AppendText($"🔌 Состояние подключения: {(connected ? "Подключено" : "Отключено")}\n");
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DeveloperView] DeviceMonitor error: {ex.Message}");
            }
        }

        private void CheckAdbButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ConsoleOutput.Clear();
                ConsoleOutput.AppendText("=== Проверка ADB ===\n\n");

                bool adbAvailable = AppServices.Adb.IsAdbAvailable();
                ConsoleOutput.AppendText($"ADB доступен: {(adbAvailable ? "✅ Да" : "❌ Нет")}\n");

                if (adbAvailable)
                {
                    string version = AppServices.Adb.Version;
                    ConsoleOutput.AppendText($"Версия ADB: {version}\n");

                    string devices = AppServices.Adb.GetDevices();
                    ConsoleOutput.AppendText($"\nУстройства:\n{devices}\n");

                    bool connected = AppServices.Adb.IsDeviceConnected();
                    ConsoleOutput.AppendText($"\nПодключено: {(connected ? "✅ Да" : "❌ Нет")}\n");

                    if (connected)
                    {
                        var info = AppServices.Adb.GetDeviceInfo();
                        if (info != null)
                        {
                            ConsoleOutput.AppendText($"\nИнформация об устройстве:\n");
                            ConsoleOutput.AppendText($"Модель: {info.Model}\n");
                            ConsoleOutput.AppendText($"Производитель: {info.Manufacturer}\n");
                            ConsoleOutput.AppendText($"Android: {info.AndroidVersion}\n");
                        }
                    }
                }

                UpdateInfo();
                ConsoleOutput.ScrollToEnd();
            }
            catch (Exception ex)
            {
                ConsoleOutput.AppendText($"\n❌ Ошибка: {ex.Message}\n");
                Debug.WriteLine($"[DeveloperView] CheckAdb error: {ex.Message}");
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ConsoleOutput.AppendText("🔄 Обновление информации...\n");
                UpdateInfo();
                ConsoleOutput.AppendText("✅ Информация обновлена\n");
                ConsoleOutput.ScrollToEnd();
            }
            catch (Exception ex)
            {
                ConsoleOutput.AppendText($"❌ Ошибка обновления: {ex.Message}\n");
                Debug.WriteLine($"[DeveloperView] Refresh error: {ex.Message}");
            }
        }

        private void ClearLogButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ConsoleOutput.Clear();
                ConsoleOutput.AppendText("📋 Журнал очищен\n");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DeveloperView] ClearLog error: {ex.Message}");
            }
        }

        private void FocusCommandButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CommandTextBox.Focus();
                ConsoleOutput.AppendText("⌨️ Введите ADB команду в поле ввода\n");
                ConsoleOutput.ScrollToEnd();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DeveloperView] FocusCommand error: {ex.Message}");
            }
        }

        // ==================== ОБРАБОТЧИК КЛАВИШ (ИСПРАВЛЕННЫЙ) ====================

        /// <summary>
        /// Обработчик нажатия клавиш в поле ввода команды
        /// Используем PreviewKeyDown вместо KeyDown для более надёжного перехвата
        /// </summary>
        private void CommandTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                // Проверяем, что нажата клавиша Enter
                if (e.Key == Key.Enter)
                {
                    e.Handled = true;  // Предотвращаем стандартную обработку
                    ExecuteCommand();
                }
            }
            catch (Exception ex)
            {
                ConsoleOutput.AppendText($"\n❌ Ошибка: {ex.Message}\n");
                Debug.WriteLine($"[DeveloperView] KeyDown error: {ex.Message}");
            }
        }

        private void ExecuteCommandButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ExecuteCommand();
            }
            catch (Exception ex)
            {
                ConsoleOutput.AppendText($"\n❌ Ошибка: {ex.Message}\n");
                Debug.WriteLine($"[DeveloperView] ExecuteCommand error: {ex.Message}");
            }
        }

        // ==================== ВЫПОЛНЕНИЕ КОМАНДЫ ====================

        private void ExecuteCommand()
        {
            try
            {
                string command = CommandTextBox.Text.Trim();

                if (string.IsNullOrWhiteSpace(command))
                {
                    ConsoleOutput.AppendText("⚠️ Введите команду ADB\n");
                    return;
                }

                if (!AppServices.Adb.IsAdbAvailable())
                {
                    ConsoleOutput.AppendText("❌ ADB не найден\n");
                    return;
                }

                if (!AppServices.Adb.IsDeviceConnected())
                {
                    ConsoleOutput.AppendText("❌ Устройство не подключено\n");
                    return;
                }

                ConsoleOutput.AppendText($"\n> adb shell {command}\n");

                string result = AppServices.Adb.ExecuteShellCommand(command);
                ConsoleOutput.AppendText(result + "\n");

                ConsoleOutput.ScrollToEnd();

                // Очищаем поле ввода после выполнения
                CommandTextBox.Clear();
            }
            catch (Exception ex)
            {
                ConsoleOutput.AppendText($"\n❌ Ошибка выполнения: {ex.Message}\n");
                Debug.WriteLine($"[DeveloperView] Execute error: {ex.Message}");
            }
        }

        // ==================== ОТПИСКА ОТ СОБЫТИЙ ====================

        private void Cleanup()
        {
            try
            {
                if (AppServices.DeviceMonitor != null)
                {
                    AppServices.DeviceMonitor.ConnectionChanged -= DeviceMonitor_ConnectionChanged;
                }

                this.Loaded -= DeveloperView_Loaded;
                this.Unloaded -= DeveloperView_Unloaded;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DeveloperView] Cleanup error: {ex.Message}");
            }
        }

        private void DeveloperView_Unloaded(object sender, RoutedEventArgs e)
        {
            Cleanup();
        }
    }
}