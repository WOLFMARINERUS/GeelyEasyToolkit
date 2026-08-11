namespace GeelyEasyToolkit.Services
{
    public class AppServices
    {
        public static NavigationService Navigation { get; } = new NavigationService();

        public static ProfileService Profiles { get; } = new ProfileService();

        public static RepositoryService Repository { get; } = new RepositoryService();

        public static SettingsService Settings { get; } = new SettingsService();

        public static LoggerService Logger { get; } = new LoggerService();

        public static AdbService Adb { get; } = new AdbService();

        public static AdbActivationService AdbActivation { get; }
    = new AdbActivationService();

        public static DeviceMonitorService DeviceMonitor { get; } = new DeviceMonitorService();

        public static ApplicationCacheService ApplicationCache { get; } =
    new ApplicationCacheService();
    }
}