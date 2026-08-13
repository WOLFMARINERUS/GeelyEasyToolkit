using GeelyEasyToolkit.Models;
using System.Windows;

namespace GeelyEasyToolkit.Views
{
    public partial class ProfileEditorWindow : Window
    {
        public VehicleProfile Profile { get; private set; }

        public ProfileEditorWindow(VehicleProfile? profile = null)
        {
            InitializeComponent();

            if (profile == null)
            {
                Profile = new VehicleProfile
                {
                    Manufacturer = "Geely",
                    ConnectionMethod = "USB ADB"
                };

                TitleText.Text = "Добавление профиля";
            }
            else
            {
                Profile = new VehicleProfile
                {
                    Name = profile.Name,
                    Manufacturer = profile.Manufacturer,
                    AndroidVersion = profile.AndroidVersion,
                    Firmware = profile.Firmware,
                    ConnectionMethod = profile.ConnectionMethod,
                    SupportsWirelessAdb = profile.SupportsWirelessAdb,
                    RequiresDeveloperMode = profile.RequiresDeveloperMode
                };

                TitleText.Text = "Редактирование профиля";
            }

            LoadProfileToFields();
        }

        private void LoadProfileToFields()
        {
            NameTextBox.Text = Profile.Name;
            ManufacturerTextBox.Text = Profile.Manufacturer;
            AndroidTextBox.Text = Profile.AndroidVersion;
            FirmwareTextBox.Text = Profile.Firmware;
            ConnectionTextBox.Text = Profile.ConnectionMethod;

            WirelessAdbCheckBox.IsChecked =
                Profile.SupportsWirelessAdb;

            DeveloperModeCheckBox.IsChecked =
                Profile.RequiresDeveloperMode;
        }

        private void SaveButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                System.Windows.MessageBox.Show(
                    "Введите название автомобиля.",
                    "Профиль",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);

                return;
            }

            Profile.Name =
                NameTextBox.Text.Trim();

            Profile.Manufacturer =
                ManufacturerTextBox.Text.Trim();

            Profile.AndroidVersion =
                AndroidTextBox.Text.Trim();

            Profile.Firmware =
                FirmwareTextBox.Text.Trim();

            Profile.ConnectionMethod =
                ConnectionTextBox.Text.Trim();

            Profile.SupportsWirelessAdb =
                WirelessAdbCheckBox.IsChecked == true;

            Profile.RequiresDeveloperMode =
                DeveloperModeCheckBox.IsChecked == true;

            DialogResult = true;
        }

        private void CancelButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}