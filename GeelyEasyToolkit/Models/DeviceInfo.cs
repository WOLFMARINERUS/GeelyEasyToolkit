namespace GeelyEasyToolkit.Models
{
    public class DeviceInfo
    {
        public bool Connected { get; set; }

        public string Serial { get; set; } = "";

        public string Model { get; set; } = "";

        public string Manufacturer { get; set; } = "";

        public string AndroidVersion { get; set; } = "";

        public string BuildId { get; set; } = "";

        public string FirmwareVersion { get; set; } = "";
    }
}