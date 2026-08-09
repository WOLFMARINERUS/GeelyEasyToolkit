using GeelyEasyToolkit.Models;
using System;
using System.Windows.Threading;

namespace GeelyEasyToolkit.Services
{
    public class DeviceMonitorService
    {
        private readonly DispatcherTimer _timer;

        public bool IsConnected { get; private set; }

        public event Action<bool>? ConnectionChanged;

        public DeviceMonitorService()
        {
            _timer = new DispatcherTimer();

            _timer.Interval = TimeSpan.FromSeconds(2);

            _timer.Tick += Timer_Tick;
        }

        public void Start()
        {
            CheckConnection();

            _timer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            CheckConnection();
        }

        private void CheckConnection()
        {
            bool connected = AppServices.Adb.IsDeviceConnected();
                        
            if (connected)
            {
                DeviceInfo = AppServices.Adb.GetDeviceInfo();
            }
            else
            {
                DeviceInfo = null;
            }

            if (connected != IsConnected)
            {
                IsConnected = connected;

                ConnectionChanged?.Invoke(IsConnected);
            }
        }
        public DeviceInfo? DeviceInfo { get; private set; }
    }
}