namespace GeelyEasyToolkit.Models
{
    public class VehicleProfile
    {
        /// <summary>
        /// Название автомобиля
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Производитель
        /// </summary>
        public string Manufacturer { get; set; } = "Geely";

        /// <summary>
        /// Версия Android
        /// </summary>
        public string AndroidVersion { get; set; } = "";

        /// <summary>
        /// Версия прошивки
        /// </summary>
        public string Firmware { get; set; } = "";

        /// <summary>
        /// Метод подключения
        /// </summary>
        public string ConnectionMethod { get; set; } = "";

        /// <summary>
        /// Поддерживает ли беспроводной ADB
        /// </summary>
        public bool SupportsWirelessAdb { get; set; }

        /// <summary>
        /// Требуется ли режим разработчика
        /// </summary>
        public bool RequiresDeveloperMode { get; set; }
    }
}