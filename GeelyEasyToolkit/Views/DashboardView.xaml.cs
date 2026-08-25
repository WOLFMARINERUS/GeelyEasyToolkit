using GeelyEasyToolkit.Services;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace GeelyEasyToolkit.Views
{
    public partial class DashboardView : System.Windows.Controls.UserControl
    {
        public DashboardView()
        {
            InitializeComponent();

            AppServices.DeviceMonitor.ConnectionChanged += DeviceMonitor_ConnectionChanged;
            AppServices.Repository.RepositoryChanged += Repository_RepositoryChanged;

            UpdateDashboard();
        }

        private void Repository_RepositoryChanged()
        {
            Dispatcher.Invoke(() =>
            {
                UpdateDashboard();
            });
        }

        private void DeviceMonitor_ConnectionChanged(bool connected)
        {
            Dispatcher.Invoke(() =>
            {
                UpdateDashboard();
            });
        }

        private void ApplicationsCard_MouseLeftButtonUp(
    object sender,
    MouseButtonEventArgs e)
        {
            AppServices.Navigation.Navigate("Applications");
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
            // Приложения / репозиторий
            //------------------------------------------

            if (AppServices.Repository.Repository != null)
            {
                RepositoryStatusText.Text =
                    "🟢 Репозиторий готов";

                RepositoryStatusText.Foreground =
                    System.Windows.Media.Brushes.LightGreen;
            }
            else
            {
                RepositoryStatusText.Text =
                    "🔴 Репозиторий не загружен";

                RepositoryStatusText.Foreground =
                    System.Windows.Media.Brushes.OrangeRed;
            }
        }
    }
}