using System;
using System.IO;

namespace GeelyEasyToolkit.Services
{
    public class LoggerService
    {
        private readonly string _logFile =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "log.txt");

        public void Log(string message)
        {
            string folder = Path.GetDirectoryName(_logFile)!;

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            File.AppendAllText(
                _logFile,
                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}");
        }
    }
}