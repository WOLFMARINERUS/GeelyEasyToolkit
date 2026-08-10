using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using GeelyEasyToolkit.Models;

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