using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeelyEasyToolkit.Services
{
    public class SettingsService
    {
        /// <summary>
        /// Путь к adb.exe
        /// </summary>
        public string AdbPath { get; set; } = "";

        /// <summary>
        /// Папка с APK
        /// </summary>
        public string ApkFolder { get; set; } = "";

        public double WindowWidth { get; set; } = 1400;

        public double WindowHeight { get; set; } = 1000;

        public double WindowLeft { get; set; } = -1;

        public double WindowTop { get; set; } = -1;

        public bool WindowMaximized { get; set; } = false;
    }
}
