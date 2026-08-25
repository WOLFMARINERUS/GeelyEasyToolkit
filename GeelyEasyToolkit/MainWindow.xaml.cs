using GeelyEasyToolkit.Services;
using GeelyEasyToolkit.Views;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Interop;
// 👇 ДОБАВЛЕНО
using System.Windows.Input;
// 👇 ДОБАВЛЕНО: для Win32 API
using System.Runtime.InteropServices;

namespace GeelyEasyToolkit
{
    public partial class MainWindow : Window
    {
        // 👇 ДОБАВЛЕНО: Win32 API для изменения цвета заголовка
        [DllImport("user32.dll")]
        private static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        [StructLayout(LayoutKind.Sequential)]
        private struct WindowCompositionAttributeData
        {
            public WindowCompositionAttribute Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        private enum WindowCompositionAttribute
        {
            WCA_ACCENT_POLICY = 19
        }

        private enum AccentState
        {
            ACCENT_ENABLE_BLURBEHIND = 3,
            ACCENT_ENABLE_ACRYLICBLURBEHIND = 4
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct AccentPolicy
        {
            public AccentState AccentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }

        private readonly DashboardView _dashboardView = new();
        private readonly ConnectionView _connectionView = new();
        private readonly ApplicationsView _applicationsView = new();
        private readonly InstalledApplicationsView _installedApplicationsView = new();
        private readonly RepositoryView _repositoryView = new();
        private readonly ProfilesView _profilesView = new();
        private readonly LogsView _logsView = new();
        private readonly SettingsView _settingsView = new();
        private readonly DeveloperView _developerView = new();

        // 👇 ДОБАВЛЕНО: переменные для тройного клика
        private DateTime _lastClickTime = DateTime.MinValue;
        private int _clickCount = 0;

        public MainWindow()
        {
            InitializeComponent();
            var windowHandle = new WindowInteropHelper(this).Handle;

            // 👇 ДОБАВЛЕНО: установка тёмного цвета заголовка
            this.SourceInitialized += MainWindow_SourceInitialized;

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
            AppServices.Navigation.Register("Profiles", _profilesView);
            AppServices.Navigation.Register("Logs",    _logsView);
            AppServices.Navigation.Register("Settings", _settingsView);
            AppServices.Navigation.Register("Developer", _developerView);
            HighlightButton(DashboardButton);

            // 👇 ДОБАВЛЕНО: подписка на тройной клик по заголовку
            if (TitleTextBlock != null)
            {
                TitleTextBlock.MouseDown += TitleTextBlock_MouseDown;
            }
        }

        // 👇 ДОБАВЛЕНО: обработчик события SourceInitialized для изменения цвета заголовка
        private void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            try
            {
                var handle = new WindowInteropHelper(this).Handle;
                if (handle == IntPtr.Zero) return;

                // Настройка цвета заголовка (тёмный)
                var accent = new AccentPolicy
                {
                    AccentState = AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND,
                    AccentFlags = 0x20 | 0x40 | 0x80,
                    GradientColor = 0x00252A32  // тёмно-серый (#252932)
                };

                var accentStructSize = Marshal.SizeOf(accent);
                var accentPtr = Marshal.AllocHGlobal(accentStructSize);
                Marshal.StructureToPtr(accent, accentPtr, false);

                var data = new WindowCompositionAttributeData
                {
                    Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
                    Data = accentPtr,
                    SizeOfData = accentStructSize
                };

                SetWindowCompositionAttribute(handle, ref data);
                Marshal.FreeHGlobal(accentPtr);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MainWindow] Ошибка изменения заголовка: {ex.Message}");
            }
        }

        // 👇 ДОБАВЛЕНО: перетаскивание окна
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                this.DragMove();
            }
        }

        // 👇 ДОБАВЛЕНО: кнопка "Свернуть"
        private void MinimizeWindow_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        // 👇 ДОБАВЛЕНО: кнопка "Развернуть"
        private void MaximizeWindow_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                MaximizeButton.Content = "☐";
            }
            else
            {
                WindowState = WindowState.Maximized;
                MaximizeButton.Content = "⧉";
            }
        }

        // 👇 ДОБАВЛЕНО: кнопка "Закрыть"
        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            Close();
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

            settings.WindowMaximized = WindowState == WindowState.Maximized;
            settings.Save();
            base.OnClosed(e);
        }

        private void DashboardButton_Click(object sender, RoutedEventArgs e)
        {
            AppServices.Navigation.Navigate("Dashboard");
            HighlightButton(DashboardButton);
        }

        private void InstalledApplicationsButton_Click(object sender, RoutedEventArgs e)
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

        private void ProfilesButton_Click(object sender, RoutedEventArgs e)
        {
            AppServices.Navigation.Navigate("Profiles");
            HighlightButton(ProfilesButton);
        }

        private void RepositoryButton_Click(object sender, RoutedEventArgs e)
        {
            AppServices.Navigation.Navigate("Repository");
            HighlightButton(RepositoryButton);
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            AppServices.Navigation.Navigate("Settings");
            HighlightButton(SettingsButton);
        }

        private void DeveloperButton_Click(object sender, RoutedEventArgs e)
        {
            AppServices.Navigation.Navigate("Developer");
            HighlightButton(DeveloperButton);
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

        private void LogsButton_Click(object sender, RoutedEventArgs e)
        {
            AppServices.Navigation.Navigate("Logs");

            HighlightButton(LogsButton);
        }

        // 👇 ДОБАВЛЕНО: обработчик тройного клика
        private void TitleTextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                // Сбрасываем счётчик, если прошло больше 1 секунды
                if ((DateTime.Now - _lastClickTime).TotalSeconds > 1)
                {
                    _clickCount = 0;
                }

                _lastClickTime = DateTime.Now;
                _clickCount++;

                if (_clickCount >= 3)
                {
                    _clickCount = 0;  // Сбрасываем счётчик

                    // Переключаем видимость кнопки "Разработчик"
                    bool isVisible = DeveloperButton.Visibility != Visibility.Visible;
                    DeveloperButton.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;

                    // Если кнопка скрыта и мы на вкладке разработчика — переключаемся на Dashboard
                    if (!isVisible)
                    {
                        var currentView = AppServices.Navigation.GetCurrentView();
                        if (currentView is DeveloperView)
                        {
                            AppServices.Navigation.Navigate("Dashboard");
                            HighlightButton(DashboardButton);
                        }
                    }

                    // Сохраняем состояние в настройках (если есть)
                    AppServices.Settings.ShowDeveloperTab = isVisible;
                    AppServices.Settings.Save();

                    // Показываем уведомление в строке состояния
                    StatusBarText.Text = isVisible
                        ? "🔧 Режим разработчика включён"
                        : "🔒 Режим разработчика выключен";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MainWindow] TitleClick error: {ex.Message}");
            }
        }
    }
}