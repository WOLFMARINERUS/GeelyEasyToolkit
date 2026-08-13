using GeelyEasyToolkit.Models;
using GeelyEasyToolkit.Services;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GeelyEasyToolkit.Views
{
    public partial class ConnectionView : System.Windows.Controls.UserControl
    {
        private bool _isCleaningUp = false;

        public ConnectionView()
        {
            try
            {
                InitializeComponent();

                // Подписываемся на событие Unloaded для очистки
                this.Unloaded += ConnectionView_Unloaded;

                // Загружаем профили при старте
                AppServices.Profiles.LoadProfiles();

                // Подписываемся на события
                AppServices.DeviceMonitor.ConnectionChanged += DeviceMonitor_ConnectionChanged;

                // Обновляем состояние
                UpdateConnectionState(AppServices.DeviceMonitor.IsConnected);

                // Логируем успешную инициализацию
                DiagnosticsOutput?.AppendText("✅ ConnectionView инициализирован\n");
            }
            catch (Exception ex)
            {
                StatusText.Text = $"⚠️ Ошибка инициализации: {ex.Message}";
                StatusText.Foreground = System.Windows.Media.Brushes.OrangeRed;

                // Логирование
                System.Diagnostics.Debug.WriteLine($"[ConnectionView] Ошибка: {ex.Message}");
            }
        }

        // ==================== ОБРАБОТЧИК ВЫГРУЗКИ ====================

        private void ConnectionView_Unloaded(object sender, RoutedEventArgs e)
        {
            Cleanup();
        }

        // ==================== ОБРАБОТЧИКИ СОБЫТИЙ ====================

        private void CheckConnection_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!AppServices.Adb.IsAdbAvailable())
                {
                    StatusText.Text = "❌ ADB не найден";
                    StatusText.Foreground = System.Windows.Media.Brushes.OrangeRed;

                    ModelText.Text = "Модель: -";
                    ManufacturerText.Text = "Производитель: -";
                    AndroidText.Text = "Android: -";
                    BuildText.Text = "Build: -";
                    FirmwareText.Text = "Прошивка: -";
                    ProfileText.Text = "Профиль: -";
                    return;
                }

                UpdateConnectionState(AppServices.Adb.IsDeviceConnected());
            }
            catch (Exception ex)
            {
                StatusText.Text = $"⚠️ Ошибка: {ex.Message}";
                StatusText.Foreground = System.Windows.Media.Brushes.OrangeRed;
                System.Diagnostics.Debug.WriteLine($"[ConnectionView] CheckConnection error: {ex.Message}");
            }
        }

        private void Diagnostics_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DiagnosticsOutput.Clear();
                DiagnosticsOutput.AppendText("=== Диагностика ADB ===\n\n");

                if (!AppServices.Adb.IsAdbAvailable())
                {
                    DiagnosticsOutput.AppendText("❌ adb.exe не найден.\n");
                    DiagnosticsOutput.AppendText("\nУбедитесь, что adb.exe находится в папке:\n");
                    DiagnosticsOutput.AppendText(AppDomain.CurrentDomain.BaseDirectory);
                    return;
                }

                DiagnosticsOutput.AppendText("✅ ADB найден.\n\n");

                // Получаем список устройств
                string devices = AppServices.Adb.GetDevices();
                DiagnosticsOutput.AppendText("Список устройств:\n");
                DiagnosticsOutput.AppendText(devices);
                DiagnosticsOutput.AppendText("\n");

                // Проверяем подключение
                if (!AppServices.Adb.IsDeviceConnected())
                {
                    DiagnosticsOutput.AppendText("\n❌ Автомобиль не подключен.\n");
                    DiagnosticsOutput.AppendText("\nРекомендации:\n");
                    DiagnosticsOutput.AppendText("1. Подключите телефон к автомобилю через USB\n");
                    DiagnosticsOutput.AppendText("2. Включите отладку по USB в настройках разработчика\n");
                    DiagnosticsOutput.AppendText("3. Разрешите отладку на телефоне при запросе\n");
                    return;
                }

                DiagnosticsOutput.AppendText("\n✅ Автомобиль подключен.\n");

                // Получаем информацию об устройстве
                DeviceInfo? info = AppServices.Adb.GetDeviceInfo();
                if (info == null)
                {
                    DiagnosticsOutput.AppendText("\n⚠️ Не удалось получить информацию об устройстве.\n");
                    return;
                }

                DiagnosticsOutput.AppendText("\n=== Информация об устройстве ===\n");
                DiagnosticsOutput.AppendText($"Модель: {info.Model ?? "Неизвестно"}\n");
                DiagnosticsOutput.AppendText($"Производитель: {info.Manufacturer ?? "Неизвестно"}\n");
                DiagnosticsOutput.AppendText($"Android: {info.AndroidVersion ?? "Неизвестно"}\n");
                DiagnosticsOutput.AppendText($"Build: {info.BuildId ?? "Неизвестно"}\n");
                DiagnosticsOutput.AppendText($"Прошивка: {info.FirmwareVersion ?? "Неизвестно"}\n");

                // Проверяем профиль
                VehicleProfile? profile = AppServices.Profiles.GetCurrentProfile();
                DiagnosticsOutput.AppendText($"\nПрофиль: {(profile?.Name ?? "не выбран")}\n");

                DiagnosticsOutput.ScrollToEnd();
            }
            catch (Exception ex)
            {
                DiagnosticsOutput.AppendText($"\n❌ Ошибка диагностики: {ex.Message}\n");
                System.Diagnostics.Debug.WriteLine($"[ConnectionView] Diagnostics error: {ex.Message}");
            }
        }

        private void DeviceMonitor_ConnectionChanged(bool connected)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    if (!_isCleaningUp)
                    {
                        UpdateConnectionState(connected);
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ConnectionView] DeviceMonitor error: {ex.Message}");
            }
        }

        private void UpdateConnectionState(bool connected)
        {
            try
            {
                if (!connected)
                {
                    StatusText.Text = "🔴 Автомобиль не подключен";
                    StatusText.Foreground = System.Windows.Media.Brushes.OrangeRed;

                    ModelText.Text = "Модель: -";
                    ManufacturerText.Text = "Производитель: -";
                    AndroidText.Text = "Android: -";
                    BuildText.Text = "Build: -";
                    FirmwareText.Text = "Прошивка: -";
                    ProfileText.Text = "Профиль: -";
                    return;
                }

                // Проверяем ADB
                if (!AppServices.Adb.IsAdbAvailable())
                {
                    StatusText.Text = "❌ ADB не найден";
                    StatusText.Foreground = System.Windows.Media.Brushes.OrangeRed;
                    return;
                }

                // Получаем информацию об устройстве
                DeviceInfo? info = AppServices.Adb.GetDeviceInfo();
                if (info == null)
                {
                    StatusText.Text = "⚠️ Не удалось получить информацию";
                    StatusText.Foreground = System.Windows.Media.Brushes.OrangeRed;
                    return;
                }

                StatusText.Text = "🟢 Автомобиль подключен";
                StatusText.Foreground = System.Windows.Media.Brushes.LightGreen;

                ModelText.Text = $"Модель: {info.Model ?? "Неизвестно"}";
                ManufacturerText.Text = $"Производитель: {info.Manufacturer ?? "Неизвестно"}";
                AndroidText.Text = $"Android: {info.AndroidVersion ?? "Неизвестно"}";
                BuildText.Text = $"Build: {info.BuildId ?? "Неизвестно"}";
                FirmwareText.Text = $"Прошивка: {info.FirmwareVersion ?? "Неизвестно"}";

                // Отображаем профиль
                VehicleProfile? profile = AppServices.Profiles.GetCurrentProfile();
                ProfileText.Text = profile != null
                    ? $"Профиль: {profile.Name}"
                    : "Профиль: не выбран";

                // Если профиль не выбран, пытаемся определить автоматически
                if (profile == null)
                {
                    var detectedProfile = AppServices.Profiles.DetectProfile(info);
                    if (detectedProfile != null)
                    {
                        ProfileText.Text = $"🔍 Обнаружен: {detectedProfile.Name} (автоматически)";
                        ProfileText.Foreground = System.Windows.Media.Brushes.LightYellow;
                    }
                }
                else
                {
                    ProfileText.Foreground = System.Windows.Media.Brushes.LightGreen;
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = $"⚠️ Ошибка обновления: {ex.Message}";
                StatusText.Foreground = System.Windows.Media.Brushes.OrangeRed;
                System.Diagnostics.Debug.WriteLine($"[ConnectionView] UpdateState error: {ex.Message}");
            }
        }

        private void ExecuteAdbCommand_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string command = AdbCommandTextBox.Text.Trim();

                if (string.IsNullOrWhiteSpace(command))
                {
                    System.Windows.MessageBox.Show(
                        "Введите команду ADB.",
                        "ADB",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return;
                }

                if (!AppServices.Adb.IsAdbAvailable())
                {
                    System.Windows.MessageBox.Show(
                        "ADB не найден.",
                        "ADB",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                if (!AppServices.Adb.IsDeviceConnected())
                {
                    System.Windows.MessageBox.Show(
                        "Устройство не подключено.",
                        "ADB",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                DiagnosticsOutput.AppendText($"\n\n> adb shell {command}\n");

                string result = AppServices.Adb.ExecuteShellCommand(command);
                DiagnosticsOutput.AppendText(result + "\n");

                DiagnosticsOutput.ScrollToEnd();

                // Очищаем поле ввода после выполнения
                AdbCommandTextBox.Clear();
            }
            catch (Exception ex)
            {
                DiagnosticsOutput.AppendText($"\n❌ Ошибка выполнения команды: {ex.Message}\n");
                System.Diagnostics.Debug.WriteLine($"[ConnectionView] ExecuteCommand error: {ex.Message}");
            }
        }

        private async void ActivateAdb_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Блокируем кнопку, чтобы предотвратить повторный запуск
                var button = sender as System.Windows.Controls.Button;
                if (button != null)
                {
                    button.IsEnabled = false;
                }

                DiagnosticsOutput.Clear();
                DiagnosticsOutput.AppendText("=== Мастер активации ADB ===\n\n");
                DiagnosticsOutput.AppendText("Запуск процесса подготовки...\n\n");

                // Подписываемся на логи
                AppServices.AdbActivation.Log += OnActivationLog;

                // Запускаем активацию
                bool result = await AppServices.AdbActivation.Activate();

                // Отписываемся от логов
                AppServices.AdbActivation.Log -= OnActivationLog;

                DiagnosticsOutput.AppendText(Environment.NewLine);

                if (result)
                {
                    DiagnosticsOutput.AppendText("✅ Процесс подготовки завершён успешно.\n");
                    DiagnosticsOutput.AppendText("\nТеперь вы можете:\n");
                    DiagnosticsOutput.AppendText("- Подключиться к автомобилю\n");
                    DiagnosticsOutput.AppendText("- Выполнять ADB команды\n");
                    DiagnosticsOutput.AppendText("- Управлять функциями автомобиля\n");

                    // Обновляем состояние после активации
                    UpdateConnectionState(AppServices.Adb.IsDeviceConnected());
                }
                else
                {
                    DiagnosticsOutput.AppendText("❌ Процесс подготовки завершился с ошибкой.\n");
                    DiagnosticsOutput.AppendText("\nПроверьте:\n");
                    DiagnosticsOutput.AppendText("1. Подключение к интернету\n");
                    DiagnosticsOutput.AppendText("2. Доступ к папке приложения\n");
                    DiagnosticsOutput.AppendText("3. Наличие adb.exe в папке\n");
                }

                DiagnosticsOutput.ScrollToEnd();
            }
            catch (Exception ex)
            {
                DiagnosticsOutput.AppendText($"\n❌ Критическая ошибка: {ex.Message}\n");
                System.Diagnostics.Debug.WriteLine($"[ConnectionView] ActivateAdb error: {ex.Message}");
            }
            finally
            {
                // Разблокируем кнопку
                var button = sender as System.Windows.Controls.Button;
                if (button != null)
                {
                    button.IsEnabled = true;
                }
            }
        }

        // ==================== ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ====================

        private void OnActivationLog(string text)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    DiagnosticsOutput.AppendText(text + Environment.NewLine);
                    DiagnosticsOutput.ScrollToEnd();
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ConnectionView] Log error: {ex.Message}");
            }
        }

        // ==================== ОТПИСКА ОТ СОБЫТИЙ ====================

        /// <summary>
        /// Очищает ресурсы и отписывается от событий
        /// </summary>
        private void Cleanup()
        {
            try
            {
                _isCleaningUp = true;

                // Отписываемся от события Unloaded
                this.Unloaded -= ConnectionView_Unloaded;

                // Отписываемся от DeviceMonitor
                if (AppServices.DeviceMonitor != null)
                {
                    AppServices.DeviceMonitor.ConnectionChanged -= DeviceMonitor_ConnectionChanged;
                }

                // Отписываемся от логов
                if (AppServices.AdbActivation != null)
                {
                    AppServices.AdbActivation.Log -= OnActivationLog;
                }

                System.Diagnostics.Debug.WriteLine("[ConnectionView] Очистка выполнена успешно");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ConnectionView] Cleanup error: {ex.Message}");
            }
        }
    }
}