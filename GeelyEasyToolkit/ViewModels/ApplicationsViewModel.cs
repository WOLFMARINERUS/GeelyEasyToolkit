using System.Collections.ObjectModel;
using GeelyEasyToolkit.Models;
using GeelyEasyToolkit.Services;

namespace GeelyEasyToolkit.ViewModels
{
    public class ApplicationsViewModel
    {
        public ObservableCollection<ApplicationInfo> Applications { get; } = new();

        public void SyncFromService()
        {
            Applications.Clear();

            foreach (var app in AppServices.Repository.GetApplications())
            {
                Applications.Add(app);
            }
        }
    }
}
