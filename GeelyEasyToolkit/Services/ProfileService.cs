using GeelyEasyToolkit.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace GeelyEasyToolkit.Services
{
    public class ProfileService
    {
        private readonly ILogger<ProfileService>? _logger;

        public VehicleProfile? CurrentProfile { get; private set; }

        public event Action<VehicleProfile?>? CurrentProfileChanged;

        public List<VehicleProfile> Profiles { get; private set; } = new();

        public ProfileService(ILogger<ProfileService>? logger = null)
        {
            _logger = logger;
        }

        private string ProfilesFolder =>
            Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Profiles");

        public void LoadProfiles()
        {
            Profiles.Clear();

            Directory.CreateDirectory(ProfilesFolder);

            string[] files = Directory.GetFiles(ProfilesFolder, "*.json");

            foreach (string file in files)
            {
                try
                {
                    string json = File.ReadAllText(file);

                    VehicleProfile? profile = JsonSerializer.Deserialize<VehicleProfile>(
                        json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (profile == null || string.IsNullOrWhiteSpace(profile.Name))
                        continue;

                    Profiles.Add(profile);
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning($"Ошибка загрузки профиля из {file}: {ex.Message}");
                }
            }

            // Сортируем без перезаписи свойства
            var sorted = Profiles
                .OrderBy(p => p.Name, StringComparer.CurrentCultureIgnoreCase)
                .ToList();

            Profiles.Clear();
            Profiles.AddRange(sorted);
        }

        public bool SaveProfile(VehicleProfile profile)
        {
            try
            {
                if (profile == null || string.IsNullOrWhiteSpace(profile.Name))
                    return false;

                Directory.CreateDirectory(ProfilesFolder);

                string safeName = string.Join(
                    "_",
                    profile.Name.Split(Path.GetInvalidFileNameChars()));

                string path = Path.Combine(ProfilesFolder, $"{safeName}.json");

                string json = JsonSerializer.Serialize(
                    profile,
                    new JsonSerializerOptions { WriteIndented = true });

                File.WriteAllText(path, json);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Ошибка сохранения профиля: {ex.Message}");
                return false;
            }
        }

        public bool DeleteProfile(VehicleProfile profile)
        {
            try
            {
                if (profile == null || string.IsNullOrWhiteSpace(profile.Name))
                    return false;

                string[] files = Directory.GetFiles(ProfilesFolder, "*.json");

                foreach (string file in files)
                {
                    try
                    {
                        string json = File.ReadAllText(file);

                        VehicleProfile? existing = JsonSerializer.Deserialize<VehicleProfile>(
                            json,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (existing == null)
                            continue;

                        if (!string.Equals(
                                existing.Name,
                                profile.Name,
                                StringComparison.OrdinalIgnoreCase))
                            continue;

                        File.Delete(file);

                        // Исправленное сравнение по имени
                        if (CurrentProfile != null &&
                            string.Equals(CurrentProfile.Name, profile.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            CurrentProfile = null;
                        }

                        return true;
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning($"Ошибка чтения {file}: {ex.Message}");
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Ошибка удаления профиля: {ex.Message}");
                return false;
            }
        }

        public void LoadProfile(
    VehicleProfile profile)
        {
            CurrentProfile = profile;

            CurrentProfileChanged?.Invoke(CurrentProfile);
        }

        public VehicleProfile? DetectProfile(DeviceInfo info)
        {
            if (info == null)
                return null;

            string model = info.Model?.ToLowerInvariant() ?? "";
            string profileName = model switch
            {
                var m when m.Contains("cityray") || m.Contains("fy11") => "Cityray",
                var m when m.Contains("atlas") => "Atlas",
                var m when m.Contains("preface") => "Preface",
                _ => null
            };

            if (string.IsNullOrEmpty(profileName))
                return null;

            // Ищем существующий профиль
            return Profiles.FirstOrDefault(p =>
                string.Equals(p.Name, profileName, StringComparison.OrdinalIgnoreCase))
                ?? new VehicleProfile
                {
                    Name = profileName,
                    Manufacturer = "Geely",
                    SupportsWirelessAdb = profileName == "Cityray" ? false : true,
                    RequiresDeveloperMode = true
                };

        }

        public VehicleProfile? GetCurrentProfile()
        {
            return CurrentProfile;
        }
    }
}