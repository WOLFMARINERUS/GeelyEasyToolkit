using GeelyEasyToolkit.Services;
using System;
using System.Windows;
using System.Windows.Controls;

namespace GeelyEasyToolkit.Views
{
    public partial class SettingsView : System.Windows.Controls.UserControl
    {
        public SettingsView()
        {
            InitializeComponent();

            LoadSettings();
        }


        private void LoadSettings()
        {
            AdbPathTextBox.Text =
                AppServices.Settings.AdbPath;

            ApkFolderTextBox.Text =
                AppServices.Settings.ApkFolder;

            ShowDeveloperCheckBox.IsChecked =
                AppServices.Settings.ShowDeveloperTab;

            WindowInfoText.Text =
                $"Размер окна: " +
                $"{AppServices.Settings.WindowWidth:0} × " +
                $"{AppServices.Settings.WindowHeight:0}";
        }


        private void BrowseAdb_Click(
            object sender,
            RoutedEventArgs e)
        {
            var dialog =
                new Microsoft.Win32.OpenFileDialog();

            dialog.Title =
                "Выберите adb.exe";

            dialog.Filter =
                "ADB executable|adb.exe";

            if (dialog.ShowDialog() == true)
            {
                AdbPathTextBox.Text =
                    dialog.FileName;
            }
        }


        private void BrowseApk_Click(
            object sender,
            RoutedEventArgs e)
        {
            using var dialog =
                new System.Windows.Forms.FolderBrowserDialog();

            dialog.Description =
                "Выберите папку с APK";

            if (dialog.ShowDialog() ==
                System.Windows.Forms.DialogResult.OK)
            {
                ApkFolderTextBox.Text =
                    dialog.SelectedPath;
            }
        }


        private void ShowDeveloperCheckBox_Checked(
            object sender,
            RoutedEventArgs e)
        {
            AppServices.Settings.ShowDeveloperTab =
                true;
        }


        private void ShowDeveloperCheckBox_Unchecked(
            object sender,
            RoutedEventArgs e)
        {
            AppServices.Settings.ShowDeveloperTab =
                false;
        }


        private void SaveSettingsButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            AppServices.Settings.AdbPath =
                AdbPathTextBox.Text.Trim();

            AppServices.Settings.ApkFolder =
                ApkFolderTextBox.Text.Trim();

            AppServices.Settings.ShowDeveloperTab =
                ShowDeveloperCheckBox.IsChecked == true;

            AppServices.Settings.Save();

            SaveStatusText.Text =
                "✓ Настройки сохранены";
        }
    }
}