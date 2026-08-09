using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;

namespace GeelyEasyToolkit.Services
{
    public class ApplicationCacheService
    {
        private readonly string _cachePath;

        private Dictionary<string, CachedApplication> _cache = new();

        public ApplicationCacheService()
        {
            string dataFolder = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Data");

            Directory.CreateDirectory(dataFolder);

            _cachePath = Path.Combine(
                dataFolder,
                "ApplicationCache.json");

            Load();
        }

        private void Load()
        {
            try
            {
                if (!File.Exists(_cachePath))
                    return;

                string json = File.ReadAllText(_cachePath);

                if (!string.IsNullOrWhiteSpace(json))
                {
                    _cache =
                        JsonSerializer.Deserialize<
                            Dictionary<string, CachedApplication>>(json)
                        ?? new Dictionary<string, CachedApplication>();
                }
            }
            catch
            {
                _cache = new Dictionary<string, CachedApplication>();
            }
        }

        private void Save()
        {
            try
            {
                string json = JsonSerializer.Serialize(
                    _cache,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

                File.WriteAllText(_cachePath, json);
            }
            catch
            {
                // Ошибка кэша не должна ломать программу.
            }
        }

        public bool TryGet(
            string packageName,
            out CachedApplication application)
        {
            return _cache.TryGetValue(
                packageName,
                out application!);
        }

        public void Set(
    string packageName,
    string name,
    string version,
    string versionCode = "")
        {
            _cache[packageName] =
                new CachedApplication
                {
                    Name = name,
                    Version = version,
                    VersionCode = versionCode
                };
        }

        public void SaveCache()
        {
            Save();
        }

        public void RemoveMissingPackages(
    HashSet<string> installedPackages)
        {
            List<string> packagesToRemove = _cache.Keys
                .Where(package => !installedPackages.Contains(package))
                .ToList();

            foreach (string package in packagesToRemove)
            {
                _cache.Remove(package);
            }
        }

        public void Remove(string packageName)
        {
            if (_cache.Remove(packageName))
            {
                Save();
            }
        }
    }

    public class CachedApplication
    {
        public string Name { get; set; } = "";

        public string Version { get; set; } = "";

        public string VersionCode { get; set; } = "";
    }
}