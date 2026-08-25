using System;
using System.IO;
using System.Text;

namespace GeelyEasyToolkit.Services
{
    public class LoggerService
    {
        private readonly string _logsFolder;
        private readonly string _logFile;

        private readonly object _lock = new object();

        public string LogFilePath => _logFile;

        public LoggerService()
        {
            string appDataFolder =
                Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.ApplicationData),
                    "GeelyEasyToolkit");

            _logsFolder =
                Path.Combine(
                    appDataFolder,
                    "Logs");

            Directory.CreateDirectory(_logsFolder);

            _logFile =
                Path.Combine(
                    _logsFolder,
                    "GeelyEasyToolkit.log");
        }


        public void Log(string message)
        {
            Write("INFO", message);
        }


        public void Warning(string message)
        {
            Write("WARNING", message);
        }


        public void Error(string message)
        {
            Write("ERROR", message);
        }


        public void Debug(string message)
        {
            Write("DEBUG", message);
        }


        private void Write(
            string level,
            string message)
        {
            try
            {
                string time =
                    DateTime.Now.ToString(
                        "yyyy-MM-dd HH:mm:ss");

                string line =
                    $"[{time}] [{level}] {message}";

                lock (_lock)
                {
                    File.AppendAllText(
                        _logFile,
                        line +
                        Environment.NewLine,
                        Encoding.UTF8);
                }
            }
            catch
            {
                // Ошибка журнала не должна
                // ломать основную программу.
            }
        }


        public string ReadAll()
        {
            try
            {
                if (!File.Exists(_logFile))
                    return "";

                lock (_lock)
                {
                    return File.ReadAllText(
                        _logFile,
                        Encoding.UTF8);
                }
            }
            catch
            {
                return "";
            }
        }


        public void Clear()
        {
            try
            {
                lock (_lock)
                {
                    if (File.Exists(_logFile))
                    {
                        File.WriteAllText(
                            _logFile,
                            "",
                            Encoding.UTF8);
                    }
                }
            }
            catch
            {
            }
        }
    }
}