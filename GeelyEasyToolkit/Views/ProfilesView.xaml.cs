using GeelyEasyToolkit.Models;
using GeelyEasyToolkit.Services;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GeelyEasyToolkit.Views
{
    public partial class ProfilesView : System.Windows.Controls.UserControl
    {
        public ProfilesView()
        {
            InitializeComponent();

            LoadProfiles();
        }

        private void LoadProfiles()
        {
            try
            {
                AppServices.Profiles.LoadProfiles();

                ProfilesList.ItemsSource =
                    AppServices.Profiles.Profiles;

                StatusText.Text =
                    $"Найдено профилей: {AppServices.Profiles.Profiles.Count}";

                if (AppServices.Profiles.CurrentProfile != null)
                {
                    ProfilesList.SelectedItem =
                        AppServices.Profiles.CurrentProfile;
                }
                else if (ProfilesList.Items.Count > 0)
                {
                    ProfilesList.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                StatusText.Text =
                    "Ошибка загрузки профилей";

                System.Windows.MessageBox.Show(
                    $"Не удалось загрузить профили.\n\n{ex.Message}",
                    "Geely Easy Toolkit",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }


        private void ProfilesList_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (ProfilesList.SelectedItem
                is not VehicleProfile profile)
            {
                ClearProfileInfo();
                return;
            }

            ShowProfile(profile);
        }


        private void ShowProfile(
            VehicleProfile profile)
        {
            ProfileNameText.Text =
                $"Автомобиль: {profile.Name}";

            ManufacturerText.Text =
                $"Производитель: {profile.Manufacturer}";

            AndroidText.Text =
                $"Android: {profile.AndroidVersion}";

            FirmwareText.Text =
                $"Прошивка: {profile.Firmware}";

            ConnectionMethodText.Text =
                $"Подключение: {profile.ConnectionMethod}";

            WirelessAdbText.Text =
                $"Wireless ADB: {(profile.SupportsWirelessAdb ? "Да" : "Нет")}";

            DeveloperModeText.Text =
                $"Режим разработчика: {(profile.RequiresDeveloperMode ? "Требуется" : "Не требуется")}";


            if (AppServices.Profiles.CurrentProfile != null &&
                AppServices.Profiles.CurrentProfile.Name ==
                profile.Name)
            {
                CurrentProfileText.Text =
                    "✓ Этот профиль используется сейчас";

                CurrentProfileText.Foreground =
                    System.Windows.Media.Brushes.LightGreen;
            }
            else
            {
                CurrentProfileText.Text =
                    "Профиль не выбран как текущий";

                CurrentProfileText.Foreground =
                    System.Windows.Media.Brushes.LightGray;
            }
        }


        private void ClearProfileInfo()
        {
            ProfileNameText.Text =
                "Автомобиль:";

            ManufacturerText.Text =
                "Производитель:";

            AndroidText.Text =
                "Android:";

            FirmwareText.Text =
                "Прошивка:";

            ConnectionMethodText.Text =
                "Подключение:";

            WirelessAdbText.Text =
                "Wireless ADB:";

            DeveloperModeText.Text =
                "Режим разработчика:";

            CurrentProfileText.Text =
                "Профиль не выбран";
        }


        private void UseProfileButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (ProfilesList.SelectedItem
                is not VehicleProfile profile)
            {
                System.Windows.MessageBox.Show(
                    "Сначала выберите профиль автомобиля.",
                    "Geely Easy Toolkit",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);

                return;
            }

            AppServices.Profiles.LoadProfile(profile);

            ShowProfile(profile);

            StatusText.Text =
                $"Текущий профиль: {profile.Name}";
        }


        private void RefreshButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            LoadProfiles();
        }
    }
}