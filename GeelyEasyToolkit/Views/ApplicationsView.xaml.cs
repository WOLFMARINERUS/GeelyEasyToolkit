using GeelyEasyToolkit.Models;
using GeelyEasyToolkit.Services;
using GeelyEasyToolkit.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace GeelyEasyToolkit.Views
{
    public partial class ApplicationsView : System.Windows.Controls.UserControl
    {
        private readonly ApplicationsViewModel _vm =
            new ApplicationsViewModel();

        private ICollectionView? _applicationsView;

        public ApplicationsView()
        {
            InitializeComponent();

            // Оставляем существующую загрузку репозитория
            ApplicationsList.ItemsSource = _vm.Applications;

            // Создаём представление поверх существующей коллекции
            _applicationsView =
                CollectionViewSource.GetDefaultView(
                    _vm.Applications);

            // Подключаем поиск
            _applicationsView.Filter =
                ApplicationFilter;

            // Начальная сортировка
            SortApplications();

            AppServices.Profiles.CurrentProfileChanged +=
                OnCurrentProfileChanged;

            AppServices.Repository.RepositoryChanged +=
                OnRepositoryChanged;

            Loaded += ApplicationsView_Loaded;
        }

        private void ApplicationsView_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            AppServices.Repository.LoadRepository();
        }

        private void OnRepositoryChanged()
        {
            Dispatcher.Invoke(() =>
            {
                _vm.SyncFromService();
                _applicationsView?.Refresh();
            });
        }

        private void OnCurrentProfileChanged(VehicleProfile? profile)
        {
            Dispatcher.Invoke(() =>
            {
                _applicationsView?.Refresh();
            });
        }


        // =========================================================
        // ПОИСК
        // =========================================================

        private bool ApplicationFilter(object item)
        {
            if (item is not ApplicationInfo app)
                return false;

            VehicleProfile? profile =
                AppServices.Profiles.GetCurrentProfile();

            if (profile != null &&
                !app.IsCompatibleWithProfile(profile.Name))
            {
                return false;
            }

            string search =
                SearchTextBox?.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(search))
                return true;

            // Ищем по названию
            if (app.Name?.Contains(
                    search,
                    StringComparison.CurrentCultureIgnoreCase)
                == true)
            {
                return true;
            }

            // Ищем по PackageName
            if (app.PackageName?.Contains(
                    search,
                    StringComparison.OrdinalIgnoreCase)
                == true)
            {
                return true;
            }

            // Ищем по категории
            if (app.Category?.Contains(
                    search,
                    StringComparison.CurrentCultureIgnoreCase)
                == true)
            {
                return true;
            }

            // Ищем по версии
            if (app.Version?.Contains(
                    search,
                    StringComparison.CurrentCultureIgnoreCase)
                == true)
            {
                return true;
            }

            return false;
        }


        private void SearchTextBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            _applicationsView?.Refresh();
        }


        // =========================================================
        // СОРТИРОВКА
        // =========================================================

        private void SortComboBox_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            SortApplications();
        }


        private void SortApplications()
        {
            if (_applicationsView == null)
                return;

            string sort = "NameAsc";

            if (SortComboBox.SelectedItem
                is ComboBoxItem item)
            {
                sort =
                    item.Tag?.ToString()
                    ?? "NameAsc";
            }

            _applicationsView.SortDescriptions.Clear();

            switch (sort)
            {
                case "NameDesc":

                    _applicationsView.SortDescriptions.Add(
                        new SortDescription(
                            nameof(ApplicationInfo.Name),
                            ListSortDirection.Descending));

                    break;


                case "PackageAsc":

                    _applicationsView.SortDescriptions.Add(
                        new SortDescription(
                            nameof(ApplicationInfo.PackageName),
                            ListSortDirection.Ascending));

                    break;


                case "PackageDesc":

                    _applicationsView.SortDescriptions.Add(
                        new SortDescription(
                            nameof(ApplicationInfo.PackageName),
                            ListSortDirection.Descending));

                    break;


                default:

                    _applicationsView.SortDescriptions.Add(
                        new SortDescription(
                            nameof(ApplicationInfo.Name),
                            ListSortDirection.Ascending));

                    break;
            }

            _applicationsView.Refresh();
        }


        // =========================================================
        // УСТАНОВКА
        // =========================================================

        private void InstallButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            AppServices.Logger.Log(
                "Нажата кнопка установки.");

            var selectedApps =
                GetVisibleApplications()
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


            MessageBoxResult confirm =
                System.Windows.MessageBox.Show(
                    $"Будет установлено приложений: {selectedApps.Count}\n\n" +
                    "Продолжить?",
                    "Подтверждение установки",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes)
                return;


            int success = 0;
            int failed = 0;

            List<string> results = new();


            foreach (ApplicationInfo app
                     in selectedApps)
            {
                results.Add(
                    $"==============================\n" +
                    $"{app.Name}\n" +
                    $"{app.PackageName}\n");


                string apkPath =
                    AppServices.Repository
                        .GetApplicationPath(app);


                if (!System.IO.File.Exists(apkPath))
                {
                    failed++;

                    results.Add(
                        "❌ APK не найден:\n" +
                        apkPath);

                    continue;
                }


                results.Add(
                    "Установка APK...");


                string adbResult =
                    AppServices.Adb.InstallApk(
                        apkPath);


                if (!adbResult.Contains(
                        "Success",
                        StringComparison.OrdinalIgnoreCase))
                {
                    failed++;

                    results.Add(
                        "❌ Ошибка установки:\n" +
                        adbResult);

                    continue;
                }


                success++;

                results.Add(
                    "✅ APK установлен.");


                // Выполняем дополнительные ADB-команды
                ExecuteApplicationCommands(
                    app,
                    results);


                app.IsSelected = false;
            }


            results.Add(
                "\n==============================");


            results.Add(
                $"Установка завершена.\n" +
                $"Успешно: {success}\n" +
                $"Ошибок: {failed}");


            System.Windows.MessageBox.Show(
                string.Join("\n", results),
                "Результат установки",
                MessageBoxButton.OK,
                failed == 0
                    ? MessageBoxImage.Information
                    : MessageBoxImage.Warning);
        }


        // =========================================================
        // ДОПОЛНИТЕЛЬНЫЕ ADB-КОМАНДЫ
        // =========================================================

        private void ExecuteApplicationCommands(
            ApplicationInfo app,
            List<string> output)
        {
            if (app.AdbCommands == null ||
                app.AdbCommands.Count == 0)
            {
                output.Add(
                    "ADB-команды: не требуются.");

                return;
            }


            output.Add(
                $"ADB-команд: {app.AdbCommands.Count}");


            int index = 1;


            foreach (var adbCommand
                     in app.AdbCommands)
            {
                output.Add(
                    $"\nКоманда {index}:");


                if (!string.IsNullOrWhiteSpace(
                    adbCommand.Description))
                {
                    output.Add(
                        adbCommand.Description);
                }


                output.Add(
                    $"> adb shell {adbCommand.Command}");


                string result =
                    AppServices.Adb
                        .ExecuteShellCommand(
                            adbCommand.Command);


                output.Add(result);

                index++;
            }
        }

        // =========================================================
        // ВЫБРАТЬ ВСЕ
        // =========================================================

        private IEnumerable<ApplicationInfo> GetVisibleApplications()
        {
            if (_applicationsView != null)
            {
                return _applicationsView.Cast<ApplicationInfo>();
            }

            return _vm.Applications;
        }


        private void SelectAllButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            var visibleApps =
                GetVisibleApplications().ToList();

            bool selectAll = visibleApps.Any(
                app => !app.IsSelected);

            foreach (ApplicationInfo app in visibleApps)
            {
                app.IsSelected = selectAll;
            }

            ApplicationsList.Items.Refresh();

            SelectAllButton.Content =
                selectAll
                    ? "Снять выбор"
                    : "Выбрать все";
        }


    }
}