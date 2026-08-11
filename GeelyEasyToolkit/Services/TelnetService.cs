using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GeelyEasyToolkit.Services
{
    public class TelnetService
    {

        private TcpClient? _client;
        private NetworkStream? _stream;



        public async Task<bool> Connect(
            string host,
            int port = 23)
        {
            try
            {
                _client = new TcpClient();


                await _client.ConnectAsync(
                    host,
                    port);


                _stream = _client.GetStream();


                return true;
            }
            catch
            {
                Disconnect();

                return false;
            }
        }




        public async Task<string> SendCommand(
            string command)
        {
            if (_stream == null)
                return "Нет подключения Telnet.";


            try
            {

                byte[] data =
                    Encoding.ASCII.GetBytes(
                        command + "\n");


                await _stream.WriteAsync(
                    data,
                    0,
                    data.Length);



                await Task.Delay(500);



                byte[] buffer =
                    new byte[4096];


                int count =
                    await _stream.ReadAsync(
                        buffer,
                        0,
                        buffer.Length);



                string response =
                    Encoding.ASCII.GetString(
                        buffer,
                        0,
                        count);



                return response;

            }
            catch (Exception ex)
            {
                return
                    "Ошибка Telnet:\n" +
                    ex.Message;
            }

        }





        public void Disconnect()
        {

            try
            {

                _stream?.Close();

                _client?.Close();

            }
            catch
            {

            }


            _stream = null;
            _client = null;

        }


    }
}