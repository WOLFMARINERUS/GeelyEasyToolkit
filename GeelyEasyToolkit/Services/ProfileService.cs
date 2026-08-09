using GeelyEasyToolkit.Models;

namespace GeelyEasyToolkit.Services
{
    public class ProfileService
    {
        public VehicleProfile? CurrentProfile { get; private set; }

        public void LoadProfile(VehicleProfile profile)
        {
            CurrentProfile = profile;
        }
                
        public VehicleProfile? DetectProfile(DeviceInfo info)
        {
            string model = info.Model?.ToLowerInvariant() ?? "";

            if (model.Contains("cityray") || model.Contains("fy11"))
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