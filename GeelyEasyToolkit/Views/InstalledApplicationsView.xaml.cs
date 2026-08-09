using GeelyEasyToolkit.Models;
using GeelyEasyToolkit.Services;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
using UserControl = System.Windows.Controls.UserControl;

namespace GeelyEasyToolkit.Views
{
    public partial class InstalledApplicationsView : UserControl
    {
        public InstalledApplicationsView()
        {
            InitializeComponent();

            Loaded += InstalledApplicationsView_Loaded;
        }

        private async void InstalledApplicationsView_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            Loaded -= InstalledApplicationsView_Loaded;

            await LoadApplications();
        }

        private async Task LoadApplications()
        {
            RefreshButton.IsEnabled = false;
            StatusText.Text = "Обновление...";

            try
            {
                List<InstalledApplication> apps =
                    await Task.Run(() =>
                        AppServices.Adb.GetInstalledApplications());

                InstalledAppsList.ItemsSource = apps;

                StatusText.Text =
                    $"Найдено приложений: {apps.Count}";
            }
            catch (Exception ex)
            {
                StatusText.Text = "Ошибка обновления";

                System.Windows.MessageBox.Show(
                    $"Не удалось получить список приложений.\n\n{ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                RefreshButton.IsEnabled = true;
            }
        }

        private async void RefreshButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            await LoadApplications();
        }

        private void Launch_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.Button button)
                return;

            if (button.DataContext is not InstalledApplication app)
                return;

            StatusText.Text =
                $"Запуск: {app.Name}...";

            string result =
                AppServices.Adb.LaunchApplication(
                    app.PackageName);

            if (result.Contains(
                    "Monkey finished",
                    StringComparison.OrdinalIgnoreCase))
            {
                StatusText.Text =
                    $"Запущено: {app.Name}";
            }
            else
            {
                StatusText.Text =
                    $"Не удалось запустить: {app.Name}";

                System.Windows.MessageBox.Show(
                    $"Не удалось запустить приложение.\n\n" +
                    $"Название: {app.Name}\n" +
                    $"Package: {app.PackageName}\n\n" +
                    $"Ответ ADB:\n{result}",
                    "Ошибка запуска",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void UninstallItem_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.Button button)
                return;

            if (button.DataContext is not InstalledApplication app)
                return;

            MessageBoxResult confirm =
                System.Windows.MessageBox.Show(
                    $"Удалить приложение?\n\n" +
                    $"{app.Name}\n" +
                    $"{app.PackageName}\n\n" +
                    "Внимание: системные приложения могут " +
                    "быть недоступны для удаления.",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes)
                return;

            string result =
                AppServices.Adb.UninstallApplication(
                    app.PackageName);

            if (result.Contains(
                    "Success",
                    StringComparison.OrdinalIgnoreCase))
            {
                StatusText.Text =
                    $"Удалено: {app.Name}";

                _ = LoadApplications();
            }
            else
            {
                System.Windows.MessageBox.Show(
                    $"Не удалось удалить приложение.\n\n" +
                    $"Package: {app.PackageName}\n\n" +
                    $"Ответ ADB:\n{result}",
                    "Ошибка удаления",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void Uninstall_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (InstalledAppsList.SelectedItem
                is not InstalledApplication app)
            {
                System.Windows.MessageBox.Show(
                    "Выберите приложение для удаления.",
                    "Geely Easy Toolkit",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            MessageBoxResult confirm =
                System.Windows.MessageBox.Show(
                    $"Удалить приложение?\n\n" +
                    $"Название: {app.Name}\n" +
                    $"Package: {app.PackageName}\n\n" +
                    "Внимание: системные приложения могут " +
                    "быть недоступны для удаления.",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes)
                return;

            string result =
                AppServices.Adb.UninstallApplication(
                    app.PackageName);

            if (result.Contains(
                    "Success",
                    StringComparison.OrdinalIgnoreCase))
            {
                StatusText.Text =
                    $"Удалено: {app.Name}";

                InstalledAppsList.SelectedItem = null;

                _ = LoadApplications();
            }
            else
            {
                System.Windows.MessageBox.Show(
                    $"Не удалось удалить приложение.\n\n" +
                    $"Package: {app.PackageName}\n\n" +
                    $"Ответ ADB:\n{result}",
                    "Ошибка удаления",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}