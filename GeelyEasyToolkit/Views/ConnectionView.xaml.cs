using GeelyEasyToolkit.Models;
using GeelyEasyToolkit.Services;
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

            AppServices.DeviceMonitor.ConnectionChanged += DeviceMonitor_ConnectionChanged;

            UpdateConnectionState(AppServices.DeviceMonitor.IsConnected);
        }

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
        }
    }
}