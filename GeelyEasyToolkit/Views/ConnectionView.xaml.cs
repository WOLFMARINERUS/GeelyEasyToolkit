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
        public ConnectionView()
        {
            InitializeComponent();

            // 👇 ДОБАВЛЕНО: подписка на событие изменения профиля
            AppServices.Profiles.CurrentProfileChanged += OnCurrentProfileChanged;

            // 👇 ДОБАВЛЕНО: обновляем отображение профиля при загрузке
            UpdateProfileDisplay();

            AppServices.DeviceMonitor.ConnectionChanged += DeviceMonitor_ConnectionChanged;

            UpdateConnectionState(AppServices.DeviceMonitor.IsConnected);
        }

        // 👇 ДОБАВЛЕНО: обработчик события изменения профиля
        private void OnCurrentProfileChanged(VehicleProfile? profile)
        {
            Dispatcher.Invoke(() =>
            {
                UpdateProfileDisplay();
            });
        }

        // 👇 ДОБАВЛЕНО: метод обновления текста профиля
        private void UpdateProfileDisplay()
        {
            try
            {
                var profile = AppServices.Profiles.GetCurrentProfile();
                if (profile != null)
                {
                    ProfileText.Text = $"Профиль: {profile.Name}";
                    ProfileText.Foreground = System.Windows.Media.Brushes.LightGreen;
                }
                else
                {
                    ProfileText.Text = "Профиль: не выбран";
                    ProfileText.Foreground = System.Windows.Media.Brushes.Gray;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ConnectionView] UpdateProfileDisplay error: {ex.Message}");
                ProfileText.Text = "Профиль: ошибка";
            }
        }

        // ============================================================
        // ВАШ СУЩЕСТВУЮЩИЙ КОД НИЖЕ (НИЧЕГО НЕ МЕНЯЛОСЬ)
        // ============================================================

        private void CheckConnection_Click(object sender, RoutedEventArgs e)
        {
            UpdateConnectionState(AppServices.Adb.IsDeviceConnected());
        }

        private void Diagnostics_Click(object sender, RoutedEventArgs e)
        {
            DiagnosticsOutput.Clear();

            DiagnosticsOutput.AppendText("=== Диагностика ADB ===\n\n");

            if (!AppServices.Adb.IsAdbAvailable())
            {
                DiagnosticsOutput.AppendText("❌ adb.exe не найден.\n");
                return;
            }

            DiagnosticsOutput.AppendText("✅ ADB найден.\n\n");

            string devices = AppServices.Adb.GetDevices();

            DiagnosticsOutput.AppendText("Список устройств:\n");
            DiagnosticsOutput.AppendText(devices);
            DiagnosticsOutput.AppendText("\n");

            DeviceInfo info = AppServices.Adb.GetDeviceInfo();

            if (!info.Connected)
            {
                DiagnosticsOutput.AppendText("\nАвтомобиль не подключен.");
                return;
            }

            DiagnosticsOutput.AppendText("\nИнформация об устройстве\n");
            DiagnosticsOutput.AppendText($"Модель: {info.Model}\n");
            DiagnosticsOutput.AppendText($"Производитель: {info.Manufacturer}\n");
            DiagnosticsOutput.AppendText($"Android: {info.AndroidVersion}\n");
            DiagnosticsOutput.AppendText($"Build: {info.BuildId}\n");
            DiagnosticsOutput.AppendText($"Прошивка: {info.FirmwareVersion}\n");
        }

        private void DeviceMonitor_ConnectionChanged(bool connected)
        {
            Dispatcher.Invoke(() =>
            {
                UpdateConnectionState(connected);
            });
        }

        private void UpdateConnectionState(bool connected)
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

                return;
            }

            DeviceInfo info = AppServices.Adb.GetDeviceInfo();

            StatusText.Text = "🟢 Автомобиль подключен";
            StatusText.Foreground = System.Windows.Media.Brushes.LightGreen;

            ModelText.Text = $"Модель: {info.Model}";
            ManufacturerText.Text = $"Производитель: {info.Manufacturer}";
            AndroidText.Text = $"Android: {info.AndroidVersion}";
            BuildText.Text = $"Build: {info.BuildId}";
            FirmwareText.Text = $"Прошивка: {info.FirmwareVersion}";

            // 👇 ЭТА ЧАСТЬ УЖЕ ЕСТЬ В ВАШЕМ КОДЕ, НО МЫ ЕЁ НЕ ТРОГАЕМ
            // Она остаётся как есть, а добавленный выше метод UpdateProfileDisplay()
            // обновляет ProfileText отдельно.
        }

        private void ExecuteAdbCommand_Click(object sender, RoutedEventArgs e)
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

            if (!AppServices.Adb.IsDeviceConnected())
            {
                System.Windows.MessageBox.Show(
                    "Устройство не подключено.",
                    "ADB",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            DiagnosticsOutput.AppendText(
                $"\n\n> adb shell {command}\n");

            string result =
                AppServices.Adb.ExecuteShellCommand(command);

            DiagnosticsOutput.AppendText(
                result + "\n");

            DiagnosticsOutput.ScrollToEnd();
        }

        private async void ActivateAdb_Click(object sender, RoutedEventArgs e)
        {
            DiagnosticsOutput.Clear();

            DiagnosticsOutput.AppendText(
                "Запуск мастера активации ADB...\n\n");

            AppServices.AdbActivation.Log +=
                text =>
                Dispatcher.Invoke(() =>
                {
                    DiagnosticsOutput.AppendText(
                        text + Environment.NewLine);

                    DiagnosticsOutput.ScrollToEnd();
                });

            bool result =
                await AppServices.AdbActivation.Activate();

            DiagnosticsOutput.AppendText(
                Environment.NewLine);

            if (result)
            {
                DiagnosticsOutput.AppendText(
                    "Процесс подготовки завершён.\n");
            }
            else
            {
                DiagnosticsOutput.AppendText(
                    "Процесс завершился с ошибкой.\n");
            }
        }
    }
}