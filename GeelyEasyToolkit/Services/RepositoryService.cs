using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using GeelyEasyToolkit.Models;
using System.Text.Json.Serialization;

namespace GeelyEasyToolkit.Services
{
    public class RepositoryService
    {
        public RepositoryModel? Repository { get; private set; }

        public event Action? RepositoryChanged;

        public string RepositoryFolder
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(
                    AppServices.Settings.ApkFolder))
                {
                    return AppServices.Settings.ApkFolder;
                }

                return Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Repository");
            }
        }

        public string RepositoryJsonPath =>
            Path.Combine(RepositoryFolder, "repository.json");

        public bool LoadRepository()
        {
            string path = RepositoryJsonPath;
            AppServices.Logger.Debug($"Попытка загрузить репозиторий из: {path}");
            return LoadRepository(path);
        }

        public bool LoadRepository(string path)
        {
            if (!File.Exists(path))
            {
                AppServices.Logger.Warning($"Файл репозитория не найден: {path}");
                Repository ??= new RepositoryModel();
                RepositoryChanged?.Invoke();
                return false;
            }

            try
            {
                string json = File.ReadAllText(path);

                Repository =
                    JsonSerializer.Deserialize<RepositoryModel>(json);

                AppServices.Logger.Log($"✓ Репозиторий загружен: {path}");
                RepositoryChanged?.Invoke();

                return Repository != null;
            }
            catch (Exception ex)
            {
                AppServices.Logger.Error($"Ошибка при загрузке репозитория: {ex.Message}");
                return false;
            }
        }

        public bool SaveRepository(string? path = null)
        {
            path ??= RepositoryJsonPath;

            if (Repository == null)
                return false;

            try
            {
                string? directory =
                    Path.GetDirectoryName(path);

                if (!string.IsNullOrWhiteSpace(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                JsonSerializerOptions options =
                    new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        DefaultIgnoreCondition =
                            JsonIgnoreCondition.Never
                    };

                string json =
                    JsonSerializer.Serialize(
                        Repository,
                        options);

                File.WriteAllText(
                    path,
                    json);

                RepositoryChanged?.Invoke();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public string GetApplicationPath(ApplicationInfo app)
        {
            return Path.Combine(
                RepositoryFolder,
                app.Category,
                app.FileName);
        }

        public List<ApplicationInfo> GetApplications()
        {
            return Repository?.Applications
                ?? new List<ApplicationInfo>();
        }
    }
}
