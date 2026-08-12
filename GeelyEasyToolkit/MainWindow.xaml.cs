using GeelyEasyToolkit.Services;
using GeelyEasyToolkit.Views;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Interop;

namespace GeelyEasyToolkit
{
    public partial class MainWindow : Window
    {
        private readonly DashboardView _dashboardView = new();
        private readonly ConnectionView _connectionView = new();
        private readonly ApplicationsView _applicationsView = new();
        private readonly InstalledApplicationsView _installedApplicationsView = new();
        private readonly RepositoryView _repositoryView = new();
        public MainWindow()
        {
            InitializeComponent();
            var windowHandle = new WindowInteropHelper(this).Handle;

            LoadWindowSettings();

            AppServices.Navigation.Initialize(MainContent);
            AppServices.Navigation.Register("Dashboard", _dashboardView);
            AppServices.Navigation.Register("Connection", _connectionView);
            AppServices.Navigation.Register("Applications", _applicationsView);
            AppServices.Navigation.Register("InstalledApplications", _installedApplicationsView);
            AppServices.DeviceMonitor.Start();
            AppServices.DeviceMonitor.ConnectionChanged += DeviceMonitor_ConnectionChanged;
            UpdateConnectionStatus(AppServices.DeviceMonitor.IsConnected);
            AppServices.Navigation.Navigate("Dashboard");
            AppServices.Navigation.Register("Repository", _repositoryView);
            HighlightButton(DashboardButton);
        }

        private void LoadWindowSettings()
        {
            var settings = AppServices.Settings;


            Width = settings.WindowWidth;

            Height = settings.WindowHeight;


            if (settings.WindowLeft >= 0)
                Left = settings.WindowLeft;


            if (settings.WindowTop >= 0)
                Top = settings.WindowTop;


            if (settings.WindowMaximized)
                WindowState = WindowState.Maximized;
        }

        protected override void OnClosed(EventArgs e)
        {
            var settings = AppServices.Settings;


            if (WindowState == WindowState.Normal)
            {
                settings.WindowWidth = Width;
                settings.WindowHeight = Height;

                settings.WindowLeft = Left;
                settings.WindowTop = Top;
            }


            settings.WindowMaximized =
                WindowState == WindowState.Maximized;


            base.OnClosed(e);
        }

        private void DashboardButton_Click(object sender, RoutedEventArgs e)
        {
            AppServices.Navigation.Navigate("Dashboard");
            HighlightButton(DashboardButton);
        }

        private void InstalledApplicationsButton_Click(
    object sender,
    RoutedEventArgs e)
        {
            AppServices.Navigation.Navigate("InstalledApplications");
            HighlightButton(InstalledApplicationsButton);
        }

        private void ConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            AppServices.Navigation.Navigate("Connection");
            HighlightButton(ConnectionButton);
        }

        private void ApplicationsButton_Click(object sender, RoutedEventArgs e)
        {
            AppServices.Navigation.Navigate("Applications");
            HighlightButton(ApplicationsButton);
        }

        private void RepositoryButton_Click(object sender, RoutedEventArgs e)
        {
            AppServices.Navigation.Navigate("Repository");
            HighlightButton(RepositoryButton);
        }

        private void HighlightButton(System.Windows.Controls.Button activeButton)
        {
            System.Windows.Media.Brush normal = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#3A4050"));
            System.Windows.Media.Brush selected = System.Windows.Media.Brushes.White;
            System.Windows.Media.Brush selectedText = System.Windows.Media.Brushes.Black;

            System.Windows.Controls.Button[] buttons =
{
    DashboardButton,
    ConnectionButton,
    ApplicationsButton,
    ProfilesButton,
    RepositoryButton,
    InstalledApplicationsButton,
    LogsButton,
    SettingsButton,
    DeveloperButton
};

            foreach (var button in buttons)
            {
                button.Background = normal;
                button.Foreground = System.Windows.Media.Brushes.White;
            }

            activeButton.Background = selected;
            activeButton.Foreground = selectedText;
        }
        private void DeviceMonitor_ConnectionChanged(bool connected)
        {
            Dispatcher.Invoke(() =>
            {
                UpdateConnectionStatus(connected);
            });
        }

        private void UpdateConnectionStatus(bool connected)
        {
            if (connected)
            {
                ConnectionIndicator.Fill = System.Windows.Media.Brushes.LimeGreen;
                ConnectionStatusText.Text = "Подключено";
            }
            else
            {
                ConnectionIndicator.Fill = System.Windows.Media.Brushes.Red;
                ConnectionStatusText.Text = "Не подключено";
            }
        }
    }
}