using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCenters
{
    public static class Screens
    {
        public static MainWindow Window { get; set; }
        public static object MainScreen { get; set; }
        public static object SettingsScreen { get; set; }
        public static ErrorScreen ErrorScreen { get; set; }
        public static InstallScreen InstallScreen { get; set; }
        public static InstallScreen UninstallScreen { get; set; }
        public static ErrorScreen DllErrorScreen { get; set; }

    }
}
