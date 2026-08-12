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
        public VehicleProfile? CurrentProfile { get; private set; }

        public List<VehicleProfile> Profiles { get; private set; }
            = new();


        private string ProfilesFolder =>
            Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Profiles");


        public void LoadProfiles()
        {
            Profiles.Clear();

            Directory.CreateDirectory(
                ProfilesFolder);

            string[] files =
                Directory.GetFiles(
                    ProfilesFolder,
                    "*.json");

            foreach (string file in files)
            {
                try
                {
                    string json =
                        File.ReadAllText(file);

                    VehicleProfile? profile =
                        JsonSerializer.Deserialize<VehicleProfile>(
                            json,
                            new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });

                    if (profile == null)
                        continue;

                    if (string.IsNullOrWhiteSpace(profile.Name))
                        continue;

                    Profiles.Add(profile);
                }
                catch
                {
                    // Повреждённый профиль
                    // не должен ломать загрузку остальных.
                }
            }

            Profiles =
                Profiles
                    .OrderBy(
                        p => p.Name,
                        StringComparer.CurrentCultureIgnoreCase)
                    .ToList();
        }


        public void LoadProfile(
            VehicleProfile profile)
        {
            CurrentProfile = profile;
        }


        public VehicleProfile? DetectProfile(
            DeviceInfo info)
        {
            string model =
                info.Model?.ToLowerInvariant() ?? "";

            if (model.Contains("cityray") ||
                model.Contains("fy11"))
            {
                return new VehicleProfile
                {
                    Name = "Cityray",
                    Manufacturer = "Geely",
                    SupportsWirelessAdb = false,
                    RequiresDeveloperMode = true
                };
            }

            if (model.Contains("atlas"))
            {
                return new VehicleProfile
                {
                    Name = "Atlas",
                    Manufacturer = "Geely"
                };
            }

            if (model.Contains("preface"))
            {
                return new VehicleProfile
                {
                    Name = "Preface",
                    Manufacturer = "Geely"
                };
            }

            return null;
        }
    }
}