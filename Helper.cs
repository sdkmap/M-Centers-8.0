
using System;


namespace MCenters
{


    public static class Screens
    {
        public static MainWindow Window { get; set; }
        public static void SetScreen(object screen)
        {
            if(object.ReferenceEquals(screen, Window.Content)) return;
            if (object.ReferenceEquals(screen, MainScreen) && MainWindow.IsDoingRickRoll)
            {
                MainWindow.IsRickRollPaused = false;
                MainWindow.RickRoll.Play();
                Window.Content = screen;
                return;
            }
            if(object.ReferenceEquals(Window.Content, MainScreen) && MainWindow.IsDoingRickRoll)
            {
                MainWindow.IsRickRollPaused=true;
                MainWindow.RickRoll.Stop();
                MainWindow.RickRoll.Position = new TimeSpan(0, 0, 0, 0, 1);
            }
            Window.Content = screen;

        }
        public static object MainScreen { get; set; }
        public static object SettingsScreen { get; set; }
        public static ErrorScreen ErrorScreen { get; set; }
        public static InstallScreen InstallScreen { get; set; }
        public static InstallScreen UninstallScreen { get; set; }
        public static ErrorScreen DllErrorScreen { get; set; }

    }
}
