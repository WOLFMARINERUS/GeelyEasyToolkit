namespace GeelyEasyToolkit.Services
{
    public interface ILogger<T>
    {
        void LogError(string v);
        void LogWarning(string v);
    }
}
