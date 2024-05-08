using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Resources;

namespace MCenters
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    
    public partial class MainWindow
    {
        public BindingExpression EnableUninstall;
        public BindingExpression EnableInstall;


        public static MediaElement RickRoll { get; set; }
        public static bool IsDoingRickRoll { get; set; }
        public static bool IsRickRollPaused { get; internal set; }

        public MainWindow()
        {
            Screens.InstallScreen = new InstallScreen();
            Screens.UninstallScreen = new InstallScreen
            {
                Mode = InstallScreenModeEnum.Uninstall
            };
            InitializeComponent();
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;
            Screens.SettingsScreen = new Setting_Screen();
            Screens.MainScreen = this.Content;
            Screens.ErrorScreen = new ErrorScreen();
            Screens.DllErrorScreen = new ErrorScreen
            {
                CurrentMode = ErrorTypeEnum.ReportDll
            };
            RickRoll = rickRoller;
            settingsButton.ConnectedImage = settingsLogo;
            installButton.ConnectedImage = installIcon;
            uninstallButton.ConnectedImage = uninstallIcon;
            Screens.Window = this;

            EnableUninstall = uninstallButton.GetBindingExpression(IsEnabledProperty);
            EnableInstall = installButton.GetBindingExpression(IsEnabledProperty);
            var mediaFiles = Directory.GetFiles("images/");
            mediaFiles = mediaFiles.Where((file, index) => file.EndsWith(".mp4") || file.EndsWith(".gif")).ToArray();
            if (mediaFiles.Length == 0) return;

            rickRoller.Source = new Uri(mediaFiles[0], UriKind.Relative);
            var time = new TimeSpan(0, 0, 0, 0, 1);
            int i = 1;

            rickRoller.MediaEnded += (sender, e) =>
            {
                if (IsRickRollPaused) return;
                if (++i >= mediaFiles.Length) i = 0;
                // When media ends, restart playback from the beginning
                rickRoller.Position = time;
                rickRoller.Source = new Uri(mediaFiles[i], UriKind.Relative);
                try
                {
                    
                    rickRoller.Play();
                }
                catch (InvalidOperationException)
                {


                }
            };
            rickRoller.Play();
            IsDoingRickRoll = true;
            IsRickRollPaused = false;
            
        }




        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Content = Screens.SettingsScreen;
        }



        private void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            Content = Screens.InstallScreen;
        }

        private void UninstallButton_Click(object sender, RoutedEventArgs e)
        {
            Content = Screens.UninstallScreen;
        }

    }
}
