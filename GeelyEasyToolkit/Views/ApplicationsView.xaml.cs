using GeelyEasyToolkit.Models;
using GeelyEasyToolkit.Services;
using GeelyEasyToolkit.ViewModels;
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
    public partial class ApplicationsView : System.Windows.Controls.UserControl
    {
        private readonly ApplicationsViewModel _vm = new ApplicationsViewModel();
        public ApplicationsView()
        {
            InitializeComponent();

            ApplicationsList.ItemsSource = _vm.Applications;
        }

        private void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            AppServices.Logger.Log("Нажата кнопка установки.");

            var selectedApps = _vm.Applications
                .Where(a => a.IsSelected)
                .ToList();

            if (selectedApps.Count == 0)
            {
                System.Windows.MessageBox.Show(
                    "Отметьте хотя бы одно приложение.",
                    "Geely Easy Toolkit",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            MessageBoxResult confirm = System.Windows.MessageBox.Show(
                $"Будет установлено приложений: {selectedApps.Count}\n\nПродолжить?",
                "Подтверждение установки",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes)
                return;

            int installed = 0;
            int failed = 0;

            StringBuilder report = new StringBuilder();

            foreach (var app in selectedApps)
            {
                string apkPath = AppServices.Repository.GetApplicationPath(app);

                report.AppendLine($"==============================");
                report.AppendLine(app.Name);
                report.AppendLine(apkPath);

                if (!System.IO.File.Exists(apkPath))
                {
                    failed++;

                    report.AppendLine("❌ APK не найден.");
                    report.AppendLine();

                    continue;
                }

                string adbResult = AppServices.Adb.InstallApk(apkPath);

                report.AppendLine(adbResult);
                report.AppendLine();

                if (adbResult.Contains("Success"))
                {
                    installed++;
                }
                else
                {
                    failed++;
                }
            }

            report.AppendLine("==============================");
            report.AppendLine($"Успешно: {installed}");
            report.AppendLine($"Ошибок: {failed}");

            System.Windows.MessageBox.Show(
                report.ToString(),
                "Результат установки",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}