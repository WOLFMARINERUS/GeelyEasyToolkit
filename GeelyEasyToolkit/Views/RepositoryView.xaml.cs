using GeelyEasyToolkit.Models;
using GeelyEasyToolkit.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GeelyEasyToolkit.Views
{
    public partial class RepositoryView : System.Windows.Controls.UserControl
    {
        private List<RepositoryApplicationItem> _applications = new();

        private string RepositoryFolder =>
            AppServices.Repository.RepositoryFolder;

        private void AddApplicationButton_Click(
    object sender,
    RoutedEventArgs e)
        {
            ApplicationInfo application =
                new ApplicationInfo();

            RepositoryApplicationEditor editor =
                new RepositoryApplicationEditor(
                    application,
                    true);

            editor.Owner =
                Window.GetWindow(this);

            if (editor.ShowDialog() != true)
                return;

            if (AppServices.Repository.Repository == null)
                return;

            AppServices.Repository.Repository.Applications
                .Add(application);

            SaveRepository();

            LoadRepository();
        }

        public RepositoryView()
        {
            InitializeComponent();

            Loaded += RepositoryView_Loaded;
        }


        private void RepositoryView_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            Loaded -= RepositoryView_Loaded;

            LoadRepository();
        }

        private void RepositoryList_MouseDoubleClick(
    object sender,
    System.Windows.Input.MouseButtonEventArgs e)
        {
            EditApplicationButton_Click(
                sender,
                new RoutedEventArgs());
        }

        private void EditApplicationButton_Click(
    object sender,
    RoutedEventArgs e)
        {
            if (RepositoryList.SelectedItem
                is not RepositoryApplicationItem selected)
            {
                System.Windows.MessageBox.Show(
                    "Выберите приложение для редактирования.",
                    "Geely Easy Toolkit",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            if (AppServices.Repository.Repository == null)
                return;

            ApplicationInfo? application =
                AppServices.Repository.Repository.Applications
                    .FirstOrDefault(
                        a =>
                            a.Name == selected.Name &&
                            a.Category == selected.Category &&
                            a.FileName == Path.GetFileName(
                                selected.ApkPath));

            if (application == null)
            {
                System.Windows.MessageBox.Show(
                    "Не удалось найти приложение в repository.json.",
                    "Geely Easy Toolkit",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }

            RepositoryApplicationEditor editor =
                new RepositoryApplicationEditor(
                    application,
                    false);

            editor.Owner =
                System.Windows.Window.GetWindow(this);

            if (editor.ShowDialog() != true)
                return;

            SaveRepository();

            LoadRepository();
        }

        private void LoadRepository()
        {
            try
            {
                StatusText.Text = "Загрузка репозитория...";

                string repositoryPath =
                    Path.Combine(
                        RepositoryFolder,
                        "repository.json");

                RepositoryPathText.Text =
                    $"Путь: {RepositoryFolder}";


                if (!File.Exists(repositoryPath))
                {
                    _applications.Clear();

                    RepositoryList.ItemsSource =
                        null;

                    CountText.Text =
                        "Приложений: 0";

                    StatusText.Text =
                        "Файл repository.json не найден.";

                    return;
                }


                bool loaded =
                    AppServices.Repository.LoadRepository(
                        repositoryPath);


                if (!loaded ||
                    AppServices.Repository.Repository == null)
                {
                    _applications.Clear();

                    RepositoryList.ItemsSource =
                        null;

                    CountText.Text =
                        "Приложений: 0";

                    StatusText.Text =
                        "Не удалось загрузить repository.json.";

                    return;
                }


                _applications =
                    AppServices.Repository.Repository.Applications
                        .Select(CreateRepositoryItem)
                        .ToList();


                SortApplications();


                StatusText.Text =
                    "Репозиторий загружен.";

                CountText.Text =
                    $"Приложений: {_applications.Count}";
            }
            catch (Exception ex)
            {
                StatusText.Text =
                    "Ошибка загрузки репозитория.";

                System.Windows.MessageBox.Show(
                    $"Не удалось загрузить репозиторий.\n\n{ex.Message}",
                    "Geely Easy Toolkit",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        private RepositoryApplicationItem CreateRepositoryItem(
            ApplicationInfo app)
        {
            string apkPath =
                AppServices.Repository.GetApplicationPath(app);

            bool exists =
                File.Exists(apkPath);


            return new RepositoryApplicationItem
            {
                Name = app.Name,
                Category = app.Category,
                Version = app.Version,
                PackageName = app.PackageName,
                ApkStatus = exists
                    ? "✓ Найден"
                    : "✕ Нет",
                ApkPath = apkPath
            };
        }


        private void SortApplications()
        {
            if (_applications == null)
                return;


            string sort =
                "NameAsc";


            if (SortComboBox.SelectedItem
                is ComboBoxItem item)
            {
                sort =
                    item.Tag?.ToString()
                    ?? "NameAsc";
            }


            IEnumerable<RepositoryApplicationItem> sorted;


            switch (sort)
            {
                case "NameDesc":

                    sorted =
                        _applications
                            .OrderByDescending(
                                a => a.Name,
                                StringComparer.CurrentCultureIgnoreCase);

                    break;


                case "PackageAsc":

                    sorted =
                        _applications
                            .OrderBy(
                                a => a.PackageName,
                                StringComparer.OrdinalIgnoreCase);

                    break;


                case "PackageDesc":

                    sorted =
                        _applications
                            .OrderByDescending(
                                a => a.PackageName,
                                StringComparer.OrdinalIgnoreCase);

                    break;


                default:

                    sorted =
                        _applications
                            .OrderBy(
                                a => a.Name,
                                StringComparer.CurrentCultureIgnoreCase);

                    break;
            }


            ApplySearch(sorted);
        }

        private void SaveRepository()
        {
            string path =
                GetRepositoryJsonPath();


            bool saved =
                AppServices.Repository.SaveRepository(
                    path);


            if (!saved)
            {
                System.Windows.MessageBox.Show(
                    "Не удалось сохранить repository.json.",
                    "Geely Easy Toolkit",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }


            StatusText.Text =
                "Репозиторий сохранён.";
        }

        private string GetRepositoryJsonPath()
        {
            return Path.Combine(
                RepositoryFolder,
                "repository.json");
        }

        private void ApplySearch(
            IEnumerable<RepositoryApplicationItem> source)
        {
            string search =
                SearchTextBox.Text?.Trim()
                ?? "";


            if (!string.IsNullOrWhiteSpace(search))
            {
                source =
                    source.Where(a =>
                        (!string.IsNullOrWhiteSpace(a.Name) &&
                         a.Name.Contains(
                             search,
                             StringComparison.CurrentCultureIgnoreCase))
                        ||
                        (!string.IsNullOrWhiteSpace(a.PackageName) &&
                         a.PackageName.Contains(
                             search,
                             StringComparison.OrdinalIgnoreCase))
                        ||
                        (!string.IsNullOrWhiteSpace(a.Category) &&
                         a.Category.Contains(
                             search,
                             StringComparison.CurrentCultureIgnoreCase)));
            }


            List<RepositoryApplicationItem> result =
                source.ToList();


            RepositoryList.ItemsSource =
                result;


            CountText.Text =
                $"Показано: {result.Count} из {_applications.Count}";
        }


        private void SearchTextBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            SortApplications();
        }


        private void SortComboBox_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (!IsInitialized)
                return;

            SortApplications();
        }


        private void RefreshButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            LoadRepository();
        }


        private void OpenFolderButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                Directory.CreateDirectory(
                    RepositoryFolder);

                Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = RepositoryFolder,
                        UseShellExecute = true
                    });
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Не удалось открыть папку репозитория.\n\n{ex.Message}",
                    "Geely Easy Toolkit",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }


    public class RepositoryApplicationItem
    {
        public string Name { get; set; } = "";

        public string Category { get; set; } = "";

        public string Version { get; set; } = "";

        public string PackageName { get; set; } = "";

        public string ApkStatus { get; set; } = "";

        public string ApkPath { get; set; } = "";
    }
}