using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GeelyEasyToolkit.Services
{
    public class AdbActivationService
    {

        private readonly TelnetService _telnet =
            new TelnetService();


        public event Action<string>? Log;



        private void Write(string text)
        {
            // Логируем в глобальный logger
            AppServices.Logger.Log(text);

            // Также отправляем событие для совместимости
            Log?.Invoke(text);
        }



        public async Task<bool> Activate()
        {

            Write("=== Активация ADB Geely Cityray ===");


            Write("Поиск головного устройства...");



            string? host =
                await FindDevice();



            if (string.IsNullOrWhiteSpace(host))
            {
                Write(
                    "❌ Головное устройство не найдено.");

                return false;
            }



            Write(
                $"✔ Найдено устройство: {host}");



            Write(
                "Подключение Telnet...");



            bool connected =
                await _telnet.Connect(host);



            if (!connected)
            {
                Write(
                    "❌ Не удалось подключиться Telnet.");

                return false;
            }



            Write(
                "✔ Telnet подключен.");



            Write(
                "Отправка команды активации...");



            string response =
                await _telnet.SendCommand(
                    "setprop persist.service.adb.button.visible ON");



            Write(
                "Ответ устройства:");

            Write(response);



            _telnet.Disconnect();



            Write("");

            Write(
                "✔ Команда выполнена.");

            Write("");

            Write(
                "Теперь включите режим ADB через инженерное меню ГУ.");



            return true;

        }





        private async Task<string?> FindDevice()
        {

            try
            {

                Write(
                    "Проверка android.local...");


                IPHostEntry entry =
                    await Dns.GetHostEntryAsync(
                        "android.local");



                if (entry.AddressList.Length > 0)
                {

                    return
                        entry.AddressList[0]
                        .ToString();

                }


            }
            catch
            {

                Write(
                    "android.local не найден.");

            }



            Write(
                "Попытка поиска IPv6...");



            string? ipv6 =
                await FindIPv6();



            return ipv6;

        }







        private async Task<string?> FindIPv6()
        {

            try
            {

                Ping ping =
                    new Ping();



                PingReply reply =
                    await ping.SendPingAsync(
                        "android.local",
                        3000);



                if (reply.Address != null)
                {

                    return
                        reply.Address.ToString();

                }

            }
            catch
            {

            }



            return null;

        }


    }
}