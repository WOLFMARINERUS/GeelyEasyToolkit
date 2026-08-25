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
            return LoadRepository(RepositoryJsonPath);
        }

        public bool LoadRepository(string path)
        {
            if (!File.Exists(path))
            {
                Repository ??= new RepositoryModel();
                RepositoryChanged?.Invoke();
                return false;
            }

            string json = File.ReadAllText(path);

            Repository =
                JsonSerializer.Deserialize<RepositoryModel>(json);

            RepositoryChanged?.Invoke();

            return Repository != null;
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
