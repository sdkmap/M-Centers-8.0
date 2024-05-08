using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace MCenters
{
    /// <summary>
    /// Interaction logic for InstallScreen.xaml
    /// </summary>
    /// 



    public partial class InstallScreen : Page
    {


        InstallScreenModeEnum mode = InstallScreenModeEnum.Start;
        public InstallScreenModeEnum Mode
        {
            get { return mode; }
            set
            {
                if (value == mode)
                    return;
                if (value == InstallScreenModeEnum.Uninstall)

                    TitleHead.Text = "Uninstall";
                else
                    TitleHead.Text = "Install";
                mode = value;




            }
        }
        public BindingExpression StartEnabled;
        readonly ColorBrushConverter converter;
        bool enabled = true;
        public bool ButtonEnabled
        {
            get { return enabled; }
            set
            {
                enabled = value;
            }
        }
        double progressValue = 0;
        public double ProgressValue
        {
            get { return progressValue; }
            set
            {
                if (this.progressValue == 100)
                {
                    ButtonEnabled = true;
                    StartEnabled.UpdateTarget();
                    Screens.Window.EnableUninstall.UpdateTarget();

                }
                progressValue = value;

            }
        }
        public InstallScreen()
        {

            InitializeComponent();
            backButton.ConnectedImage = backIcon;
            startButton.ConnectedImage = startIcon;
            StartEnabled = startButton.GetBindingExpression(IsEnabledProperty);
            converter = new ColorBrushConverter();
            progressCaption.SetBinding(TextBlock.TextProperty, new Binding("Value") { Source = progressRing, Converter = converter });
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {


            Screens.SetScreen(Screens.MainScreen);
        }


        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Status.Text = "Waiting to Start";
            ProgressValue = 0;
            if (Mode == InstallScreenModeEnum.Start)
            {
                ButtonEnabled = false;
                StartEnabled.UpdateTarget();
                Screens.Window.EnableUninstall.UpdateTarget();

                await Install();

                ButtonEnabled = true;
                StartEnabled.UpdateTarget();
                Screens.Window.EnableUninstall.UpdateTarget();
            }
            if (Mode == InstallScreenModeEnum.Uninstall)
            {
                ButtonEnabled = false;
                StartEnabled.UpdateTarget();
                Screens.Window.EnableInstall.UpdateTarget();

                await Uninstall();

                ButtonEnabled = true;
                StartEnabled.UpdateTarget();
                Screens.Window.EnableInstall.UpdateTarget();
            }

        }
        async Task Uninstall()
        {

            var th = new Thread(UninstallMode) { Priority = ThreadPriority.Highest };
            th.SetApartmentState(ApartmentState.MTA);
            th.Start();
            while (th.ThreadState != ThreadState.Stopped)
                await Task.Delay(5000);
        }
        void UninstallMode()
        {
            Methods.Method.ProgressChanged += DllMethod_ProgressChanged;
            Methods.DllMethod.Uninstall();
            Methods.Method.ProgressChanged -= DllMethod_ProgressChanged;
        }
        void InstallDll()
        {

            Methods.Method.ProgressChanged += DllMethod_ProgressChanged;
        retry:;
            var k = Methods.DllMethod.GetVersion();
            if (Methods.DllMethod.IsAvailable(k))
            {
                Methods.Method method = new Methods.DllMethod(k);
                if (!method.IsDownloaded)
                    method.Download();
                method.Install();
                method.Close();
            }
            else
            {
                response = ErrorScreenResultEnum.pending;
                Dispatcher.Invoke(()
          =>
                {
                    Screens.DllErrorScreen.CopyClicked += DllErrorScreen_CopyClicked;
                    Screens.DllErrorScreen.RetryClicked += DllErrorScreen_RetryClicked;
                    Screens.DllErrorScreen.CancelClicked += DllErrorScreen_CancelClicked;
                    Screens.DllErrorScreen.ErrorTitle = "Unsupported Version " + k;
                    Screens.DllErrorScreen.ErrorSubTitle = "";
                    Screens.DllErrorScreen.ErrorDescription = "MCenters currently does not support your version of Windows.\nYou can retry, if you think it was a network issue.\nIf this is not a network issue then hit Submit Dlls to report your version to MCenters";
                    Screens.SetScreen(Screens.DllErrorScreen);

                });
                Version = k;
                while (response == ErrorScreenResultEnum.pending)
                    Thread.Sleep(200);


                switch (response)
                {
                    case ErrorScreenResultEnum.retry:
                        Dispatcher.Invoke(()
       =>
                        {
                            Status.Text = "Waiting to Start";
                            progressRing.Value = 0;
                            Screens.SetScreen(Screens.InstallScreen);
                            Screens.DllErrorScreen.CopyClicked -= DllErrorScreen_CopyClicked;
                            Screens.DllErrorScreen.RetryClicked -= DllErrorScreen_RetryClicked;
                            Screens.DllErrorScreen.CancelClicked -= DllErrorScreen_CancelClicked;

                        });
                        goto retry;
                    case ErrorScreenResultEnum.cancel:
                    case ErrorScreenResultEnum.copy:
                        Dispatcher.Invoke(()
      =>
                        {
                            Status.Text = "Waiting to Start";
                            progressRing.Value = 0;
                            Screens.SetScreen(Screens.MainScreen);
                            Screens.DllErrorScreen.CopyClicked -= DllErrorScreen_CopyClicked;
                            Screens.DllErrorScreen.RetryClicked -= DllErrorScreen_RetryClicked;
                            Screens.DllErrorScreen.CancelClicked -= DllErrorScreen_CancelClicked;

                        });
                        break;



                }

            }

            Methods.Method.ProgressChanged -= DllMethod_ProgressChanged;

        }
        string Version;
        
        ErrorScreenResultEnum response = ErrorScreenResultEnum.pending;
        private void DllErrorScreen_CancelClicked(object sender, EventArgs e)
        {
            response = ErrorScreenResultEnum.cancel;
        }

        private void DllErrorScreen_RetryClicked(object sender, EventArgs e)
        {
            response = ErrorScreenResultEnum.retry;
        }
        async Task PostData()
        {
            using (HttpClient client = new HttpClient())
            {
                MultipartFormDataContent g = new MultipartFormDataContent
                {
                    { new ByteArrayContent(Encoding.ASCII.GetBytes($"Unsupported Version **{Version}**")), "content" }
                };
                if (File.Exists(Methods.Method.Dllx64))
                {
                    var k = File.ReadAllBytes(Methods.Method.Dllx64);
                    if (Environment.Is64BitProcess)
                    {
                        g.Add(new ByteArrayContent(k), "files[0]", "x64.dll");
                        if (File.Exists(Methods.Method.Dllx86))
                        {
                            var gs = File.ReadAllBytes(Methods.Method.Dllx86);
                            g.Add(new ByteArrayContent(gs), "files[1]", "x86.dll");
                        }

                    }
                    else
                        g.Add(new ByteArrayContent(k), "files[0]", "x86.dll");

                }
                try
                {


                    Screens.ErrorScreen.retryButton.IsEnabled = false;
                    Screens.ErrorScreen.copyButton.IsEnabled = false;
                    Screens.ErrorScreen.cancelButton.IsEnabled = false;
                    Screens.DllErrorScreen.retryButton.IsEnabled = false;
                    Screens.DllErrorScreen.cancelButton.IsEnabled = false;
                    Screens.DllErrorScreen.copyButton.IsEnabled = true;
                    await client.PostAsync("https://discord.com/api/webhooks/924639290045116426/cnzRDxwcsnATV-IMw1QuMwfzvgnMBW7TeUJi59_hV90iNVYwIeaFM6AMCmBdSigIkJAS", g);
                    Screens.ErrorScreen.retryButton.IsEnabled = true;
                    Screens.ErrorScreen.copyButton.IsEnabled = true;
                    Screens.ErrorScreen.cancelButton.IsEnabled = true;
                    Screens.DllErrorScreen.retryButton.IsEnabled = true;
                    Screens.DllErrorScreen.cancelButton.IsEnabled = true;
                    Screens.DllErrorScreen.copyButton.IsEnabled = true;
                    response = ErrorScreenResultEnum.copy;
                }
                catch (HttpRequestException)
                {
                    Screens.ErrorScreen.ErrorTitle = "Networking Error Occured";
                    Screens.ErrorScreen.ErrorSubTitle = "";
                    Screens.ErrorScreen.ErrorDescription = "A network error occured while posting the request";
                    Screens.ErrorScreen.copyButton.IsEnabled = false;
                    Screens.ErrorScreen.RemoveRetryHandles();
                    Screens.ErrorScreen.RetryClicked += RetyFormSend;
                    Screens.ErrorScreen.CancelClicked += CancelPostRequest;
                    Screens.SetScreen(Screens.ErrorScreen);
                }



            }
        }

        private void CancelPostRequest(object sender, EventArgs e)
        {
            response = ErrorScreenResultEnum.cancel;
        }

        private async void RetyFormSend(object sender, EventArgs e)
        {
            await PostData();
        }

        private async void DllErrorScreen_CopyClicked(object sender, EventArgs e)
        {


            await PostData();

        }

        async Task Install()
        {

            var th = new Thread(InstallDll) { Priority = ThreadPriority.Highest };
            th.Start();
            while (th.ThreadState != ThreadState.Stopped)
                await Task.Delay(5000);
        }

        private void DllMethod_ProgressChanged(object sender, Methods.ProgressEventArgs e)
        {
            this.Dispatcher.Invoke((
                ) =>
            {
                progressRing.Value = e.Progress;
                Status.Text = e.Status;


            });

        }


    }
}
