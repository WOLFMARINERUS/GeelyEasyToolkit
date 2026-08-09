using System.Collections.Generic;
using System.Windows.Controls;

namespace GeelyEasyToolkit.Services
{
    public class NavigationService : INavigationService
    {
        private ContentControl? _contentControl;

        private readonly Dictionary<string, System.Windows.Controls.UserControl> _pages = new();

        public void Initialize(ContentControl contentControl)
        {
            _contentControl = contentControl;
        }

        public void Register(string key, System.Windows.Controls.UserControl page)
        {
            _pages[key] = page;
        }

        public void Navigate(string key)
        {
            if (_contentControl == null)
                return;

            if (_pages.TryGetValue(key, out System.Windows.Controls.UserControl page))
            {
                _contentControl.Content = page;
            }
        }

        // Оставляем старый метод для совместимости
        public void Navigate(System.Windows.Controls.UserControl page)
        {
            if (_contentControl != null)
            {
                _contentControl.Content = page;
            }
        }
    }
}