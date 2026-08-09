using GeelyEasyToolkit.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GeelyEasyToolkit.Views
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : System.Windows.Controls.UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
        }


        private void BrowseAdb_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();

            dialog.Title = "Выберите adb.exe";
            dialog.Filter = "ADB executable|adb.exe";

            if (dialog.ShowDialog() == true)
            {
                AdbPathTextBox.Text = dialog.FileName;
                AppServices.Settings.AdbPath = dialog.FileName;
            }
        }


        private void BrowseApk_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();

            dialog.Description = "Выберите папку с APK";

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ApkFolderTextBox.Text = dialog.SelectedPath;

                AppServices.Settings.ApkFolder = dialog.SelectedPath;
            }
        }
    }
}
