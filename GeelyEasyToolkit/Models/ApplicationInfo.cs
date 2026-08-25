namespace GeelyEasyToolkit.Models
{

    public class ApplicationInfo
    {
        public string Name { get; set; } = "";

        public string Category { get; set; } = "";

        public string Version { get; set; } = "";

        public string FileName { get; set; } = "";

        public List<string> Compatible { get; set; } = new();

        public bool IsSelected { get; set; }

        public bool IsCompatibleWithProfile(string? profileName)
        {
            if (string.IsNullOrWhiteSpace(profileName))
                return true;

            if (Compatible == null || Compatible.Count == 0)
                return false;

            return Compatible.Contains(
                profileName,
                StringComparer.OrdinalIgnoreCase);
        }

        public string PackageName { get; set; } = "";

        public List<AdbCommandInfo> AdbCommands { get; set; } = new();
    }
}