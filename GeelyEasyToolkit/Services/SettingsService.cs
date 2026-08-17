using System;
using System.IO;
using System.Text.Json;

namespace GeelyEasyToolkit.Services
{
    public class SettingsService
    {
        public string AdbPath { get; set; } = "";

        public string ApkFolder { get; set; } = "";

        public double WindowWidth { get; set; } = 1400;

        public double WindowHeight { get; set; } = 1000;

        public double WindowLeft { get; set; } = -1;

        public double WindowTop { get; set; } = -1;

        public bool WindowMaximized { get; set; } = false;

        public bool ShowDeveloperTab { get; set; } = false;

        private readonly string _settingsFile;

        public SettingsService()
        {
            string folder = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.ApplicationData),
                "GeelyEasyToolkit");

            Directory.CreateDirectory(folder);

            _settingsFile =
                Path.Combine(folder, "settings.json");

            Load();
        }

        private void Load()
        {
            try
            {
                if (!File.Exists(_settingsFile))
                    return;

                string json =
                    File.ReadAllText(_settingsFile);

                SettingsData? data =
                    JsonSerializer.Deserialize<SettingsData>(json);

                if (data == null)
                    return;

                AdbPath = data.AdbPath ?? "";
                ApkFolder = data.ApkFolder ?? "";

                WindowWidth = data.WindowWidth;
                WindowHeight = data.WindowHeight;

                WindowLeft = data.WindowLeft;
                WindowTop = data.WindowTop;

                WindowMaximized =
                    data.WindowMaximized;

                ShowDeveloperTab =
                    data.ShowDeveloperTab;
            }
            catch
            {
                // Используем значения по умолчанию.
            }
        }

        internal void Save()
        {
            try
            {
                SettingsData data = new SettingsData
                {
                    AdbPath = AdbPath,
                    ApkFolder = ApkFolder,

                    WindowWidth = WindowWidth,
                    WindowHeight = WindowHeight,

                    WindowLeft = WindowLeft,
                    WindowTop = WindowTop,

                    WindowMaximized =
                        WindowMaximized,

                    ShowDeveloperTab =
                        ShowDeveloperTab
                };

                JsonSerializerOptions options =
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    };

                string json =
                    JsonSerializer.Serialize(
                        data,
                        options);

                File.WriteAllText(
                    _settingsFile,
                    json);
            }
            catch
            {
                // Ошибка сохранения не должна
                // ломать программу.
            }
        }

        private class SettingsData
        {
            public string? AdbPath { get; set; }

            public string? ApkFolder { get; set; }

            public double WindowWidth { get; set; }

            public double WindowHeight { get; set; }

            public double WindowLeft { get; set; }

            public double WindowTop { get; set; }

            public bool WindowMaximized { get; set; }

            public bool ShowDeveloperTab { get; set; }
        }
    }
}