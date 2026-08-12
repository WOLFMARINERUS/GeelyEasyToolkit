using GeelyEasyToolkit.Models;
using GeelyEasyToolkit.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace GeelyEasyToolkit.Views
{
    public partial class RepositoryApplicationEditor : Window
    {
        private readonly ApplicationInfo _application;

        private readonly List<AdbCommandInfo> _adbCommands = new();

        private readonly bool _isNew;
        private object editor;

        public string RepositoryFolder { get; private set; }
        public ListBox RepositoryList { get; private set; }

        public RepositoryApplicationEditor(
            ApplicationInfo application,
            bool isNew)
        {
            InitializeComponent();

            _application = application;
            _isNew = isNew;

            LoadApplication();
        }


        private void LoadApplication()
        {
            NameTextBox.Text =
                _application.Name;

            CategoryTextBox.Text =
                _application.Category;

            VersionTextBox.Text =
                _application.Version;

            PackageNameTextBox.Text =
                _application.PackageName;

            FileNameTextBox.Text =
                _application.FileName;


            CityrayCheckBox.IsChecked =
                _application.Compatible
                    .Contains(
                        "Cityray",
                        StringComparer.OrdinalIgnoreCase);

            AtlasCheckBox.IsChecked =
                _application.Compatible
                    .Contains(
                        "Atlas",
                        StringComparer.OrdinalIgnoreCase);

            PrefaceCheckBox.IsChecked =
                _application.Compatible
                    .Contains(
                        "Preface",
                        StringComparer.OrdinalIgnoreCase);


            _adbCommands.Clear();

            if (_application.AdbCommands != null)
            {
                _adbCommands.AddRange(
                    _application.AdbCommands.Select(
                        command => new AdbCommandInfo
                        {
                            Command = command.Command,
                            Description = command.Description
                        }));
            }

            RefreshAdbCommands();
        }


        private void RefreshAdbCommands()
        {
            AdbCommandsList.ItemsSource = null;

            AdbCommandsList.ItemsSource =
                _adbCommands.ToList();
        }


        private void SelectApk_Click(
            object sender,
            RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog =
                new Microsoft.Win32.OpenFileDialog
                {
                    Title = "Выберите APK",
                    Filter = "Android APK (*.apk)|*.apk",
                    CheckFileExists = true
                };


            if (dialog.ShowDialog() != true)
            {
                return;
            }

            {
                FileNameTextBox.Text =
                    Path.GetFileName(dialog.FileName);


                FileNameTextBox.Tag =
                    dialog.FileName;
            }
        }


        private void AddAdbCommand_Click(
            object sender,
            RoutedEventArgs e)
        {
            string command =
                AdbCommandTextBox.Text.Trim();


            if (string.IsNullOrWhiteSpace(command))
            {
                System.Windows.MessageBox.Show(
                    "Введите ADB-команду.",
                    "Geely Easy Toolkit",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }


            if (command.StartsWith(
                "adb shell ",
                StringComparison.OrdinalIgnoreCase))
            {
                command =
                    command.Substring(
                        "adb shell ".Length)
                    .Trim();
            }


            _adbCommands.Add(
                new AdbCommandInfo
                {
                    Command = command,
                    Description = ""
                });


            AdbCommandTextBox.Clear();

            RefreshAdbCommands();
        }


        private void RemoveAdbCommand_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not FrameworkElement element)
                return;


            if (element.DataContext
                is not AdbCommandInfo command)
                return;


            _adbCommands.Remove(command);

            RefreshAdbCommands();
        }

        private string GetRepositoryJsonPath()
        {
            return Path.Combine(
                RepositoryFolder,
                "repository.json");
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
                    false)
                {
                    Owner = System.Windows.Window.GetWindow(this)
                };


            if (editor.ShowDialog() != true)
                return;


            SaveRepository();

            LoadRepository();
        }

        private void AddApplicationButton_Click(
    object sender,
    RoutedEventArgs e)
        {
            ApplicationInfo application =
                new ApplicationInfo();

            RepositoryApplicationEditor editor =
                new RepositoryApplicationEditor(
                    application,
                    true)
                {
                    Owner = Window.GetWindow(this)
                };


            if (editor.ShowDialog() != true)
                return;


            if (AppServices.Repository.Repository == null)
                return;


            AppServices.Repository.Repository.Applications
                .Add(application);


            SaveRepository();

            LoadRepository();
        }

        private void LoadRepository()
        {
            throw new NotImplementedException();
        }

        private void SaveRepository()
        {
            throw new NotImplementedException();
        }

        private void Save_Click(
            object sender,
            RoutedEventArgs e)
        {
            string name =
                NameTextBox.Text.Trim();

            string category =
                CategoryTextBox.Text.Trim();

            string version =
                VersionTextBox.Text.Trim();

            string packageName =
                PackageNameTextBox.Text.Trim();

            string fileName =
                FileNameTextBox.Text.Trim();


            if (string.IsNullOrWhiteSpace(name))
            {
                System.Windows.MessageBox.Show(
                    "Введите название приложения.",
                    "Geely Easy Toolkit",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }


            if (string.IsNullOrWhiteSpace(category))
            {
                System.Windows.MessageBox.Show(
                    "Введите категорию.",
                    "Geely Easy Toolkit",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }


            if (string.IsNullOrWhiteSpace(fileName))
            {
                System.Windows.MessageBox.Show(
                    "Укажите APK-файл.",
                    "Geely Easy Toolkit",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }


            _application.Name =
                name;

            _application.Category =
                category;

            _application.Version =
                version;

            _application.PackageName =
                packageName;

            _application.FileName =
                fileName;


            _application.Compatible =
                new List<string>();


            if (CityrayCheckBox.IsChecked == true)
                _application.Compatible.Add("Cityray");

            if (AtlasCheckBox.IsChecked == true)
                _application.Compatible.Add("Atlas");

            if (PrefaceCheckBox.IsChecked == true)
                _application.Compatible.Add("Preface");


            _application.AdbCommands =
                _adbCommands
                    .Select(
                        command => new AdbCommandInfo
                        {
                            Command = command.Command,
                            Description = command.Description
                        })
                    .ToList();


            DialogResult = true;
        }


        private void Cancel_Click(
            object sender,
            RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}