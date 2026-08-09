using System;
using System.Collections.ObjectModel;
using System.IO;
using GeelyEasyToolkit.Models;
using GeelyEasyToolkit.Services;

namespace GeelyEasyToolkit.ViewModels
{
    public class ApplicationsViewModel
    {
        public ObservableCollection<ApplicationInfo> Applications { get; } = new();

        public ApplicationsViewModel()
        {
            string repositoryPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Repository",
                "repository.json");

            if (AppServices.Repository.LoadRepository(repositoryPath))
            {
                foreach (var app in AppServices.Repository.Repository!.Applications)
                {
                    Applications.Add(app);
                }
            }
        }
    }
}