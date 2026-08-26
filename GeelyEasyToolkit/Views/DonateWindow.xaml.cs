using System.Windows;
using QRCoder;
using System;
using System.Diagnostics;
using System.Windows.Media.Imaging;

namespace GeelyEasyToolkit.Views
{
    public partial class DonateWindow : Window
    {
        private const string DonateUrl =
    "https://www.sberbank.ru/ru/choise_bank?requisiteNumber=79676509117&bankCode=100000000111";

        private void GenerateDonateQrCode()
        {
            using QRCodeGenerator qrGenerator = new QRCodeGenerator();

            using QRCodeData qrCodeData =
                qrGenerator.CreateQrCode(
                    DonateUrl,
                    QRCodeGenerator.ECCLevel.Q);

            PngByteQRCode qrCode =
                new PngByteQRCode(qrCodeData);

            byte[] qrBytes =
                qrCode.GetGraphic(10);

            BitmapImage bitmap =
                new BitmapImage();

            using (System.IO.MemoryStream stream =
                   new System.IO.MemoryStream(qrBytes))
            {
                bitmap.BeginInit();
                bitmap.CacheOption =
                    BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
                bitmap.Freeze();
            }

            DonateQrCodeImage.Source = bitmap;
        }

        private void DonateLinkButton_Click(
    object sender,
    RoutedEventArgs e)
        {
            try
            {
                Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = DonateUrl,
                        UseShellExecute = true
                    });
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Не удалось открыть страницу поддержки.\n\n{ex.Message}",
                    "Поддержать проект",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        public DonateWindow()
        {
            InitializeComponent();

            GenerateDonateQrCode();
        }


        private void CloseButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            Close();
        }
    }
}