using System;
using System.Diagnostics;
using System.IO;
using GeelyEasyToolkit.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace GeelyEasyToolkit.Services
{
    public class AdbService
    {
        private string GetAdbPath()
        {
            if (!string.IsNullOrWhiteSpace(AppServices.Settings.AdbPath))
                return AppServices.Settings.AdbPath;

            return Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Tools",
                "adb",
                "adb.exe");
        }

        public bool IsAdbAvailable()
        {
            return File.Exists(GetAdbPath());
        }

        public string Execute(string arguments)
        {
            if (!IsAdbAvailable())
                return "ADB не найден.";

            Process process = new Process();

            process.StartInfo.FileName = GetAdbPath();
            process.StartInfo.Arguments = arguments;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            return string.IsNullOrWhiteSpace(error)
                ? output
                : error;

        }

        public string Version
        {
            get
            {
                try
                {
                    if (!IsAdbAvailable())
                        return "ADB не найден";

                    return Execute("version");
                }
                catch (Exception ex)
                {
                    return $"❌ Ошибка: {ex.Message}";
                }
            }
        }

        public bool IsDeviceConnected()
        {
            string result = Execute("devices");

            return result.Contains("\tdevice");
        }
        public string GetDevices()
        {
            return Execute("devices");
        }
        public string GetProp(string property)
        {
            return Execute($"shell getprop {property}").Trim();
        }
        public DeviceInfo GetDeviceInfo()
        {
            DeviceInfo info = new DeviceInfo();

            info.Connected = IsDeviceConnected();

            if (!info.Connected)
                return info;

            info.Model = GetProp("ro.product.model");
            info.Manufacturer = GetProp("ro.product.manufacturer");
            info.AndroidVersion = GetProp("ro.build.version.release");
            info.BuildId = GetProp("ro.build.display.id");
            info.FirmwareVersion = GetProp("ro.build.fingerprint");

            return info;
        }
        public string InstallApk(string apkPath)
        {
            if (!File.Exists(apkPath))
            {
                AppServices.Logger.Error($"Попытка установки APK: файл не найден - {apkPath}");
                return "Файл APK не найден.";
            }

            AppServices.Logger.Log($"Начало установки APK: {Path.GetFileName(apkPath)}");
            string result = Execute($"install -r \"{apkPath}\"");

            if (result.Contains("Success", StringComparison.OrdinalIgnoreCase))
                AppServices.Logger.Log($"✓ APK успешно установлен: {Path.GetFileName(apkPath)}");
            else
                AppServices.Logger.Error($"✗ Ошибка при установке APK: {result}");

            return result;
        }
        public bool Install(string apk)
        {
            string result = InstallApk(apk);

            return result.Contains("Success");
        }
        public List<InstalledApplication> GetInstalledApplications()
        {
            List<InstalledApplication> list = new();
            HashSet<string> installedPackages = new();

            string result = Execute(
                "shell pm list packages -3 --show-versioncode");

            foreach (string line in result.Split('\n'))
            {
                if (!line.StartsWith("package:"))
                    continue;

                string packageLine = line
                    .Replace("package:", "")
                    .Trim();

                if (string.IsNullOrWhiteSpace(packageLine))
                    continue;

                string packageName = packageLine;
                string versionCode = "";

                // Ищем versionCode в строке.
                int versionCodeIndex =
                    packageLine.IndexOf(" versionCode:");

                if (versionCodeIndex >= 0)
                {
                    packageName = packageLine
                        .Substring(0, versionCodeIndex)
                        .Trim();

                    versionCode = packageLine
                        .Substring(
                            versionCodeIndex + " versionCode:".Length)
                        .Trim();
                }

                if (string.IsNullOrWhiteSpace(packageName))
                    continue;
                installedPackages.Add(packageName);
                string name = "";
                string version = "";

                // Сначала смотрим постоянный кэш.
                if (AppServices.ApplicationCache.TryGet(
                        packageName,
                        out CachedApplication cached))
                {
                    name = cached.Name;

                    // Если версия в кэше совпадает,
                    // повторно запрашивать её не нужно.
                    if (!string.IsNullOrWhiteSpace(cached.VersionCode) &&
                        cached.VersionCode == versionCode)
                    {
                        version = cached.Version;
                    }
                }

                // Если название неизвестно —
                // используем существующий механизм AAPT2.
                if (string.IsNullOrWhiteSpace(name))
                {
                    name = GetApplicationName(packageName);
                }

                // Если версия неизвестна или изменилась —
                // получаем её через существующий метод.
                if (string.IsNullOrWhiteSpace(version))
                {
                    try
                    {
                        version = GetApplicationVersion(packageName);
                    }
                    catch
                    {
                        version = "";
                    }
                }

                // Обновляем постоянный кэш.
                AppServices.ApplicationCache.Set(
                    packageName,
                    name,
                    version,
                    versionCode);

                list.Add(new InstalledApplication
                {
                    Name = name,
                    PackageName = packageName,
                    Version = version
                });
            }

            AppServices.ApplicationCache.RemoveMissingPackages(
    installedPackages);

            AppServices.ApplicationCache.SaveCache();

            return list;
        }

        private string GetApplicationName(string packageName)
        {
            if (AppServices.ApplicationCache.TryGet(
        packageName,
        out CachedApplication cached))
            {
                return cached.Name;
            }
            if (_applicationNameCache.TryGetValue(packageName, out string cachedName))
                return cachedName;

            string aaptPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Tools",
                "aapt2",
                "aapt2.exe");

            if (!File.Exists(aaptPath))
                return packageName;

            string apkPathResult = Execute(
                $"shell pm path {packageName}");

            if (string.IsNullOrWhiteSpace(apkPathResult))
                return packageName;

            string remoteApkPath = "";

            foreach (string line in apkPathResult.Split('\n'))
            {
                string trimmed = line.Trim();

                if (trimmed.StartsWith("package:"))
                {
                    remoteApkPath = trimmed
                        .Substring("package:".Length)
                        .Trim();

                    break;
                }
            }

            if (string.IsNullOrWhiteSpace(remoteApkPath))
                return packageName;

            string safeFileName = packageName
                .Replace("/", "_")
                .Replace("\\", "_")
                .Replace(":", "_");

            string tempApkPath = Path.Combine(
                Path.GetTempPath(),
                safeFileName + ".apk");

            Execute($"pull \"{remoteApkPath}\" \"{tempApkPath}\"");

            if (!File.Exists(tempApkPath))
                return packageName;

            try
            {
                using Process process = new Process();

                process.StartInfo.FileName = aaptPath;
                process.StartInfo.Arguments =
                    $"dump badging \"{tempApkPath}\"";

                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                process.Start();

                string output = process.StandardOutput.ReadToEnd();

                process.WaitForExit();

                foreach (string line in output.Split('\n'))
                {
                    string trimmed = line.Trim();

                    if (trimmed.StartsWith("application-label:"))
                    {
                        int start = trimmed.IndexOf("'") + 1;
                        int end = trimmed.LastIndexOf("'");

                        if (start > 0 && end > start)
                        {
                            string name = trimmed.Substring(
                                start,
                                end - start);

                            if (!string.IsNullOrWhiteSpace(name))
                            {
                                _applicationNameCache[packageName] = name;

                                AppServices.ApplicationCache.Set(
                                    packageName,
                                    name,
                                    "");

                                return name;
                            }
                        }
                    }
                }
            }
            finally
            {
                try
                {
                    if (File.Exists(tempApkPath))
                        File.Delete(tempApkPath);
                }
                catch
                {
                }
            }

            return packageName;
        }

        private string GetApplicationVersion(string packageName)
        {
            string result = Execute(
                $"shell dumpsys package {packageName}");

            foreach (string line in result.Split('\n'))
            {
                string trimmed = line.Trim();

                if (trimmed.StartsWith("versionName="))
                {
                    return trimmed
                        .Replace("versionName=", "")
                        .Trim();
                }
            }

            return "Неизвестно";
        }

        public string UninstallApplication(string packageName)
        {
            if (string.IsNullOrWhiteSpace(packageName))
            {
                AppServices.Logger.Warning("Попытка деинсталляции: имя пакета не указано");
                return "Package Name не указан.";
            }

            if (!IsDeviceConnected())
            {
                AppServices.Logger.Warning($"Попытка деинсталляции {packageName}: устройство не подключено");
                return "Устройство не подключено.";
            }

            AppServices.Logger.Log($"Начало деинсталляции приложения: {packageName}");
            string result = Execute($"uninstall {packageName}");

            if (result.Contains("Success", StringComparison.OrdinalIgnoreCase))
                AppServices.Logger.Log($"✓ Приложение успешно удалено: {packageName}");
            else
                AppServices.Logger.Error($"✗ Ошибка при деинсталляции {packageName}: {result}");

            return result;
        }
        private readonly Dictionary<string, string> _applicationNameCache = new();

        public string LaunchApplication(string packageName)
        {
            if (string.IsNullOrWhiteSpace(packageName))
            {
                AppServices.Logger.Warning("Попытка запуска приложения: имя пакета не указано");
                return "Package приложения не указан.";
            }

            if (!IsDeviceConnected())
            {
                AppServices.Logger.Warning($"Попытка запуска {packageName}: устройство не подключено");
                return "Устройство не подключено.";
            }

            AppServices.Logger.Log($"Запуск приложения: {packageName}");
            string result = Execute(
                $"shell monkey -p {packageName} " +
                $"-c android.intent.category.LAUNCHER 1");

            if (IsSuccessfulLaunchResult(result))
                AppServices.Logger.Log($"✓ Приложение запущено: {packageName}");
            else
                AppServices.Logger.Warning($"⚠ Проблема при запуске {packageName}: {result}");

            return result;
        }

        public bool IsSuccessfulLaunchResult(string result)
        {
            if (string.IsNullOrWhiteSpace(result))
                return true;

            string lower =
                result.ToLowerInvariant();

            if (lower.Contains("no activities found"))
                return false;

            if (lower.Contains("error"))
                return false;

            if (lower.Contains("exception"))
                return false;

            if (lower.Contains("unable to resolve"))
                return false;

            if (lower.Contains("securityexception"))
                return false;

            return true;
        }


        public string ExecuteShellCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                AppServices.Logger.Warning("Попытка выполнения команды: команда не указана");
                return "Команда не указана.";
            }

            AppServices.Logger.Debug($"Выполнение команды: {command}");
            string result = Execute($"shell {command}");

            if (!string.IsNullOrWhiteSpace(result) && !result.Contains("error", StringComparison.OrdinalIgnoreCase))
                AppServices.Logger.Debug($"✓ Команда выполнена успешно");

            return result;
        }

        public string InstallApplication(string fileName)
        {
            return InstallApk(fileName);
        }

        public string GetVersion()
        {
            try
            {
                if (!IsAdbAvailable())
                    return "ADB не найден";

                return Execute("version");
            }
            catch (Exception ex)
            {
                return $"❌ Ошибка: {ex.Message}";
            }
        }
    }
}