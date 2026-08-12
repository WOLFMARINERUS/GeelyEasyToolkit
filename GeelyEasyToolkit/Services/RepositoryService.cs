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

        public bool LoadRepository(string path)
        {
            if (!File.Exists(path))
                return false;

            string json = File.ReadAllText(path);

            Repository =
                JsonSerializer.Deserialize<RepositoryModel>(json);

            return Repository != null;
        }

        public bool SaveRepository(string path)
        {
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

                return true;
            }
            catch
            {
                return false;
            }
        }

        public string GetApplicationPath(ApplicationInfo app)
        {
            string repositoryFolder;

            if (!string.IsNullOrWhiteSpace(
                AppServices.Settings.ApkFolder))
            {
                repositoryFolder =
                    AppServices.Settings.ApkFolder;
            }
            else
            {
                repositoryFolder =
                    Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "Repository");
            }

            return Path.Combine(
                repositoryFolder,
                app.Category,
                app.FileName);
        }

        internal List<ApplicationInfo> GetApplications()
        {
            throw new NotImplementedException();
        }
    }
}