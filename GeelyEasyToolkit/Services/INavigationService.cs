using System.Windows.Controls;

namespace GeelyEasyToolkit.Services
{
    public interface INavigationService
    {
        void Initialize(ContentControl contentControl);

        void Navigate(System.Windows.Controls.UserControl page);
    }
}