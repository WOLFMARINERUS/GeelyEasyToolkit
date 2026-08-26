using System.Diagnostics;
using System.Windows;

namespace GeelyEasyToolkit.Views
{
    public partial class ThanksWindow : Window
    {
        public ThanksWindow()
        {
            InitializeComponent();
        }


        private void GeelyDocsButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            OpenUrl(
                "https://github.com/wirthus/geely-docs/tree/main");
        }


        private void ScrcpyButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            OpenUrl(
                "https://github.com/Genymobile/scrcpy/releases");
        }


        private void CloseButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            Close();
        }


        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
            }
            catch
            {
                System.Windows.MessageBox.Show(
                    "Не удалось открыть ссылку.",
                    "Geely Easy Toolkit",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }
    }
}