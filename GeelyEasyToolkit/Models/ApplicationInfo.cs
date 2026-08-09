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
    }
}