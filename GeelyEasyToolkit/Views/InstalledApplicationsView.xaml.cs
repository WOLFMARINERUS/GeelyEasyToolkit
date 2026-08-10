using GeelyEasyToolkit.Models;
using GeelyEasyToolkit.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace GeelyEasyToolkit.Views
{
    public partial class InstalledApplicationsView : System.Windows.Controls.UserControl
    {
        private List<InstalledApplication> _allApplications = new();

        private ICollectionView? _applicationsView;

        public InstalledApplicationsView()
        {
            InitializeComponent();

            Loaded += InstalledApplicationsView_Loaded;

            SortComboBox.SelectedIndex = 0;
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
            DeleteSelectedButton.IsEnabled = false;

            StatusText.Text = "Обновление...";

            try
            {
                List<InstalledApplication> apps =
                    await Task.Run(() =>
                        AppServices.Adb.GetInstalledApplications());

                _allApplications = apps;

                _applicationsView =
                    CollectionViewSource.GetDefaultView(_allApplications);

                _applicationsView.Filter = FilterApplication;

                ApplySorting();

                InstalledAppsList.ItemsSource =
                    _applicationsView;

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

                UpdateDeleteButton();
            }
        }

        private bool FilterApplication(object obj)
        {
            if (obj is not InstalledApplication app)
                return false;

            string search =
                SearchTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(search))
                return true;

            return
                app.Name.Contains(
                    search,
                    StringComparison.OrdinalIgnoreCase)
                ||
                app.PackageName.Contains(
                    search,
                    StringComparison.OrdinalIgnoreCase);
        }

        private void SearchTextBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            _applicationsView?.Refresh();
        }

        private void SortComboBox_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (!IsLoaded)
                return;

            ApplySorting();
        }

        private void ApplySorting()
        {
            if (_applicationsView == null)
                return;

            _applicationsView.SortDescriptions.Clear();

            if (SortComboBox.SelectedItem
                is not ComboBoxItem item)
                return;

            string sort =
                item.Tag?.ToString() ?? "NameAsc";

            switch (sort)
            {
                case "NameAsc":

                    _applicationsView.SortDescriptions.Add(
                        new SortDescription(
                            nameof(InstalledApplication.Name),
                            ListSortDirection.Ascending));

                    break;

                case "NameDesc":

                    _applicationsView.SortDescriptions.Add(
                        new SortDescription(
                            nameof(InstalledApplication.Name),
                            ListSortDirection.Descending));

                    break;

                case "PackageAsc":

                    _applicationsView.SortDescriptions.Add(
                        new SortDescription(
                            nameof(InstalledApplication.PackageName),
                            ListSortDirection.Ascending));

                    break;

                case "PackageDesc":

                    _applicationsView.SortDescriptions.Add(
                        new SortDescription(
                            nameof(InstalledApplication.PackageName),
                            ListSortDirection.Descending));

                    break;
            }

            _applicationsView.Refresh();
        }

        private void UpdateDeleteButton()
        {
            int count =
                _allApplications.Count(a => a.IsSelected);

            DeleteSelectedButton.IsEnabled =
                count > 0;

            DeleteSelectedButton.Content =
                count > 0
                    ? $"Удалить выбранные ({count})"
                    : "Удалить выбранные";
        }

        private void Launch_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.Button button)
                return;

            if (button.DataContext
                is not InstalledApplication app)
                return;

            StatusText.Text =
                $"Запуск: {app.Name}...";

            string result =
                AppServices.Adb.LaunchApplication(
                    app.PackageName);

            if (AppServices.Adb.IsSuccessfulLaunchResult(result))
            {
                StatusText.Text =
                    $"Запущено: {app.Name}";
            }
            else
            {
                StatusText.Text =
                    $"Ошибка запуска: {app.Name}";

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

            if (button.DataContext
                is not InstalledApplication app)
                return;

            UninstallApplications(
                new List<InstalledApplication> { app });
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

            UninstallApplications(
                new List<InstalledApplication> { app });
        }

        private void UninstallSelected_Click(
            object sender,
            RoutedEventArgs e)
        {
            List<InstalledApplication> selected =
                _allApplications
                    .Where(a => a.IsSelected)
                    .ToList();

            if (selected.Count == 0)
            {
                System.Windows.MessageBox.Show(
                    "Выберите хотя бы одно приложение.",
                    "Geely Easy Toolkit",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            UninstallApplications(selected);
        }

        private void ApplicationCheckBox_Click(
    object sender,
    RoutedEventArgs e)
        {
            UpdateDeleteButton();
        }

        private void UninstallApplications(
            List<InstalledApplication> applications)
        {
            string names =
                string.Join(
                    "\n",
                    applications.Select(
                        a => $"• {a.Name}"));

            MessageBoxResult confirm =
                System.Windows.MessageBox.Show(
                    $"Будет удалено приложений: {applications.Count}\n\n" +
                    names +
                    "\n\nПродолжить?",
                    "Подтверждение удаления",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Warning);

            if (confirm != System.Windows.MessageBoxResult.Yes)
                return;

            int success = 0;
            int failed = 0;

            List<string> errors = new();

            foreach (InstalledApplication app
                     in applications)
            {
                string result =
                    AppServices.Adb.UninstallApplication(
                        app.PackageName);

                if (result.Contains(
                    "Success",
                    StringComparison.OrdinalIgnoreCase))
                {
                    success++;

                    app.IsSelected = false;
                }
                else
                {
                    failed++;

                    errors.Add(
                        $"{app.Name}\n{result}");
                }
            }

            string message =
                $"Удаление завершено.\n\n" +
                $"Успешно: {success}\n" +
                $"Ошибок: {failed}";

            if (errors.Count > 0)
            {
                message +=
                    "\n\nОшибки:\n\n" +
                    string.Join(
                        "\n\n----------------\n\n",
                        errors);
            }

            System.Windows.MessageBox.Show(
                message,
                "Geely Easy Toolkit",
                System.Windows.MessageBoxButton.OK,
                failed == 0
                    ? System.Windows.MessageBoxImage.Information
                    : System.Windows.MessageBoxImage.Warning);

            _ = LoadApplications();
        }

        private async void RefreshButton_Click(
    object sender,
    RoutedEventArgs e)
        {
            await LoadApplications();
        }

        private void SelectAllButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            bool allSelected =
                _allApplications.Count > 0 &&
                _allApplications.All(a => a.IsSelected);

            foreach (InstalledApplication app
                     in _allApplications)
            {
                app.IsSelected = !allSelected;
            }

            _applicationsView?.Refresh();

            UpdateDeleteButton();

            SelectAllButton.Content =
                allSelected
                    ? "Выбрать все"
                    : "Снять выбор";
        }

    }
}