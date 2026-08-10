using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeelyEasyToolkit.Models
{
    public class InstalledApplication
    {
        public bool IsSelected { get; set; }

        public string Name { get; set; } = "";

        public string PackageName { get; set; } = "";

        public string Version { get; set; } = "";
    }
}