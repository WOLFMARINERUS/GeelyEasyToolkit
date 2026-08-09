using GeelyEasyToolkit.Services;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;

namespace GeelyEasyToolkit.Views
{
    public partial class DashboardView : System.Windows.Controls.UserControl
    {
        public DashboardView()
        {
            InitializeComponent();

            AppServices.DeviceMonitor.ConnectionChanged += DeviceMonitor_ConnectionChanged;

            UpdateDashboard();
        }

        private void DeviceMonitor_ConnectionChanged(bool connected)
        {
            Dispatcher.Invoke(() =>
            {
                UpdateDashboard();
            });
        }

        private void UpdateDashboard()
        {
            //------------------------------------------
            // Статус автомобиля
            //------------------------------------------

            if (AppServices.DeviceMonitor.IsConnected)
            {
                CarStatusText.Text = "🟢 Подключен";
                CarStatusText.Foreground = System.Windows.Media.Brushes.LightGreen;

                if (AppServices.DeviceMonitor.DeviceInfo != null)
                {
                    CarModelText.Text =
                        AppServices.DeviceMonitor.DeviceInfo.Model;

                    AndroidVersionText.Text =
                        AppServices.DeviceMonitor.DeviceInfo.AndroidVersion;
                }
            }
            else
            {
                CarStatusText.Text = "🔴 Не подключен";
                CarStatusText.Foreground = System.Windows.Media.Brushes.OrangeRed;

                CarModelText.Text = "-";
                AndroidVersionText.Text = "-";
            }

            //------------------------------------------
            // Репозиторий
            //------------------------------------------

            string folder = AppServices.Settings.ApkFolder;

            if (!string.IsNullOrWhiteSpace(folder) &&
                Directory.Exists(folder))
            {
                RepositoryStatusText.Text = "🟢 Готов";
                RepositoryStatusText.Foreground = System.Windows.Media.Brushes.LightGreen;
            }
            else
            {
                RepositoryStatusText.Text = "🔴 Не выбран";
                RepositoryStatusText.Foreground = System.Windows.Media.Brushes.OrangeRed;
            }
        }
    }
}