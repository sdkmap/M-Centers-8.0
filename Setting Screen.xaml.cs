using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using SourceChord.FluentWPF;

namespace MCenters
{
    /// <summary>
    /// Interaction logic for Setting_Screen.xaml
    /// </summary>
    public partial class Setting_Screen : Page
    {
        public Setting_Screen()
        {
            InitializeComponent();
            backButton.ConnectedImage = backIcon;
            addButton.ConnectedImage = addIcon;
            viewButton.ConnectedImage = viewIcon;
            dllButton.ConnectedImage = dllIcon;
            logsFolderButton.ConnectedImage = logsFolderIcon;
            youtubeButton.ConnectedImage = youtubeIcon;
            discordButton.ConnectedImage = discordIcon;
            agreementButton.ConnectedImage = agreementIcon;
            policyButton.ConnectedImage = policyIcon;
            thirdPartyBox.SetBinding(AcrylicPanel.IsEnabledProperty, new Binding("IsChecked") { Source = thirdPartyCheckBox });
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {


            Screens.SetScreen(Screens.MainScreen);
        }



        private void AddButton_Click(object sender, RoutedEventArgs e)
        {

        }



        private void ViewButton_Click(object sender, RoutedEventArgs e)
        {

        }



        private void DllButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", Methods.Method.baseDllPath);
        }



        private void ErrorButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", Methods.Method.LogDirectory);
        }


        public static void OpenBrowser(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }


        private void YoutubeButton_Click(object sender, RoutedEventArgs e)
        {
            OpenBrowser("https://www.youtube.com/@tinedpakgamer");
        }



        private void DiscordButton_Click(object sender, RoutedEventArgs e)
        {
            OpenBrowser("https://discord.gg/sU8qSdP5wP");
        }



        private void AgreementButton_Click(object sender, RoutedEventArgs e)
        {

        }



        private void PolicyButton_Click(object sender, RoutedEventArgs e)
        {

        }


    }
}
