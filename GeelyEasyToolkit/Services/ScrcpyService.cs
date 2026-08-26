using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace GeelyEasyToolkit.Services
{
    public class ScrcpyService
    {
        private Process? _scrcpyProcess;

        public static readonly string SCRCPY_GITHUB = "https://github.com/Genymobile/scrcpy";

        public string ScrcpyExecutableName => 
            System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                System.Runtime.InteropServices.OSPlatform.Windows) 
            ? "scrcpy.exe" 
            : "scrcpy";

        public string GetScrcpyPath()
        {
            // Сначала проверяем пользовательский путь из настроек
            if (!string.IsNullOrWhiteSpace(AppServices.Settings.ScrcpyPath))
            {
                string customPath = Path.Combine(
                    AppServices.Settings.ScrcpyPath,
                    ScrcpyExecutableName);

                if (File.Exists(customPath))
                {
                    AppServices.Logger.Debug($"Найден Scrcpy в пользовательской папке: {customPath}");
                    return customPath;
                }

                AppServices.Logger.Warning($"Scrcpy не найден в указанной папке: {AppServices.Settings.ScrcpyPath}");
            }

            // Проверяем папку по умолчанию рядом с приложением
            string defaultPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Scrcpy",
                ScrcpyExecutableName);

            if (File.Exists(defaultPath))
            {
                AppServices.Logger.Debug($"Найден Scrcpy в папке по умолчанию: {defaultPath}");
                return defaultPath;
            }

            AppServices.Logger.Warning($"Scrcpy не найден ни в пользовательской папке, ни в папке по умолчанию");
            return "";
        }

        public bool IsScrcpyAvailable()
        {
            return !string.IsNullOrWhiteSpace(GetScrcpyPath());
        }

        public async Task<bool> LaunchScrcpy()
        {
            try
            {
                // Проверяем, что устройство подключено
                if (!AppServices.Adb.IsDeviceConnected())
                {
                    AppServices.Logger.Warning("Попытка запустить Scrcpy: устройство не подключено");
                    return false;
                }

                string scrcpyPath = GetScrcpyPath();
                if (string.IsNullOrWhiteSpace(scrcpyPath))
                {
                    AppServices.Logger.Error("Scrcpy не найден в системе");
                    return false;
                }

                // Проверяем, что экземпляр уже не запущен
                if (_scrcpyProcess != null && !_scrcpyProcess.HasExited)
                {
                    AppServices.Logger.Warning("Scrcpy уже запущен, закройте текущее окно перед запуском нового");
                    return false;
                }

                // Запускаем Scrcpy
                _scrcpyProcess = new Process();
                _scrcpyProcess.StartInfo.FileName = scrcpyPath;
                _scrcpyProcess.StartInfo.UseShellExecute = true;
                _scrcpyProcess.StartInfo.CreateNoWindow = false;

                _scrcpyProcess.Start();

                AppServices.Logger.Log($"✓ Scrcpy запущен успешно");

                // Ждем небольшое время, чтобы убедиться, что процесс запустился
                await Task.Delay(500);

                if (_scrcpyProcess.HasExited)
                {
                    AppServices.Logger.Error("Scrcpy запустился, но сразу закрылся. Проверьте, подключено ли устройство");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                AppServices.Logger.Error($"Ошибка при запуске Scrcpy: {ex.Message}");
                return false;
            }
        }

        public void StopScrcpy()
        {
            try
            {
                if (_scrcpyProcess != null && !_scrcpyProcess.HasExited)
                {
                    _scrcpyProcess.Kill();
                    _scrcpyProcess.WaitForExit(2000);
                    AppServices.Logger.Log("Scrcpy остановлен");
                }
            }
            catch (Exception ex)
            {
                AppServices.Logger.Warning($"Ошибка при остановке Scrcpy: {ex.Message}");
            }
        }

        public bool IsScrcpyRunning()
        {
            return _scrcpyProcess != null && !_scrcpyProcess.HasExited;
        }
    }
}
