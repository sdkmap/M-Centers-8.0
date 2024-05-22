using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
                    Screens.MainWindow.EnableUninstall.UpdateTarget();

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
                Screens.MainWindow.EnableUninstall.UpdateTarget();

                await Install();

                ButtonEnabled = true;
                StartEnabled.UpdateTarget();
                Screens.MainWindow.EnableUninstall.UpdateTarget();
            }
            if (Mode == InstallScreenModeEnum.Uninstall)
            {
                ButtonEnabled = false;
                StartEnabled.UpdateTarget();
                Screens.MainWindow.EnableInstall.UpdateTarget();

                await Uninstall();

                ButtonEnabled = true;
                StartEnabled.UpdateTarget();
                Screens.MainWindow.EnableInstall.UpdateTarget();
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
            var task = new MCenterTask(() =>
            {



                Methods.DllMethod.Uninstall();

            })
            {
                ErrorDescriptionBuilder =
             (ex) => { return $"An error occured while uninstalling Mod"; }
            };
        retry:;

            var asyncTask = task.Invoke(
                       );
            asyncTask.Wait();
            var result = asyncTask.Result;
            while (result == InvokeResults.busy) goto retry;
            if (result == InvokeResults.errorOccured)
            {
                Dispatcher.Invoke(() =>
                {
                    Status.Text = "Error occured last time while uninstalling dll method";
                    ProgressValue = 0;
                    progressRing.Value = 0;
                });
                
            };
            
            Methods.Method.ProgressChanged -= DllMethod_ProgressChanged;
        }
        void InstallDll()
        {


            Methods.Method.ProgressChanged += DllMethod_ProgressChanged;
        retry:;
            var systemDllVersion = "";
            var systemDllVersionCheckTask = new MCenterTask(() =>
            {


                systemDllVersion = Methods.DllMethod.GetVersion();


            })
            {
                ErrorDescriptionBuilder =
             (ex) => { return $"An error occured while fetching system dll version"; }
            };
        systemDllVersionCheckTaskRetryInvoke:;

            var systemDllVersionCheckTaskInvoker = systemDllVersionCheckTask.Invoke(
                       );
            systemDllVersionCheckTaskInvoker.Wait();
            var systemDllVersionCheckTaskResult = systemDllVersionCheckTaskInvoker.Result;
            while (systemDllVersionCheckTaskResult == InvokeResults.busy) goto systemDllVersionCheckTaskRetryInvoke;
            if (systemDllVersionCheckTaskResult == InvokeResults.errorOccured) 
            {
                Dispatcher.Invoke(() =>
                {
                    Status.Text = "Error occured last time while checking system dll version";
                    ProgressValue = 0;
                    progressRing.Value = 0;
                });
                goto exit;
            }





            bool isVersionAvailable = false;
            string provider=null;
            var DllMethodAvailabitityCheckerTask = new MCenterTask(() =>
            {


                isVersionAvailable = Methods.DllMethod.IsAvailable(systemDllVersion);
                
                if (!isVersionAvailable)
                {
                    provider = Methods.DllMethod.IsAvailableOnThirdParty(systemDllVersion);
                }

            })
            {
                ErrorDescriptionBuilder =
             (ex) => { return $"An error occured while checking version availability"; }
            };
        DllMethodAvailabitityCheckerTaskInvokeRetry:;

            var DllMethodAvailabitityCheckerTaskInvoker = DllMethodAvailabitityCheckerTask.Invoke(
                       );
            DllMethodAvailabitityCheckerTaskInvoker.Wait();
            var DllMethodAvailabitityCheckerTaskResult = DllMethodAvailabitityCheckerTaskInvoker.Result;
            while (DllMethodAvailabitityCheckerTaskResult == InvokeResults.busy) goto DllMethodAvailabitityCheckerTaskInvokeRetry;
            if (DllMethodAvailabitityCheckerTaskResult == InvokeResults.errorOccured)
            {
                Dispatcher.Invoke(() =>
                {
                    Status.Text = "Error occured last time while checking crack support for system dll";
                    ProgressValue = 0;
                    progressRing.Value = 0;
                });
                goto exit;
            }

            if (isVersionAvailable|| provider!=null)
            {
                var method = new Methods.DllMethod(systemDllVersion,provider);
                if (!method.IsDownloaded)
                {

                    var DllMethodDownloaderTask = new MCenterTask(() =>
                    {


                        method.Download();


                    })
                    {
                        ErrorDescriptionBuilder =
             (ex) => { return $"An error occured while download Dll method"; }
                    };
                DllMethodDownloaderTaskInvokeRetry:;

                    var DllMethodDownloaderTaskInvoker = DllMethodDownloaderTask.Invoke(
                               );
                    DllMethodDownloaderTaskInvoker.Wait();
                    var DllMethodDownloaderTaskResult = DllMethodDownloaderTaskInvoker.Result;
                    while (DllMethodDownloaderTaskResult == InvokeResults.busy) goto DllMethodDownloaderTaskInvokeRetry;
                    if (DllMethodDownloaderTaskResult == InvokeResults.errorOccured)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            Status.Text = "Error occured last time while downloading cracked dll";
                            ProgressValue = 0;
                            progressRing.Value = 0;
                        });
                        goto exit;
                    }




                }



                var dllMethodInstallTask = new MCenterTask(() =>
                {



                    method.Install();

                })
                {
                    ErrorDescriptionBuilder =
             (ex) => { return $"An error occured while installing dll method"; }
                };
            dllMethodInstallTaskInvokeRetry:;

                var dllMethodInstallTaskInvoker = dllMethodInstallTask.Invoke(
                           );
                dllMethodInstallTaskInvoker.Wait();
                var dllMethodInstallTaskResult = dllMethodInstallTaskInvoker.Result;
                while (dllMethodInstallTaskResult == InvokeResults.busy) goto dllMethodInstallTaskInvokeRetry;
                if (dllMethodInstallTaskResult == InvokeResults.errorOccured)
                {
                    Dispatcher.Invoke(() =>
                    {
                        Status.Text = "Error occured last time while installing dll method";
                        ProgressValue = 0;
                        progressRing.Value = 0;
                    });
                    goto exit;
                }


            }
            else
            {
                var response = Methods.DllMethod.ShowIncompatibility(systemDllVersion);
                switch (response)
                {
                    case ErrorScreenResultEnum.retry:
                        Application.Current.Dispatcher.Invoke(()
       =>
                        {


                            Screens.SetScreen(Screens.InstallScreen);
                            ProgressValue = 0;
                            progressRing.Value = 0;
                            Status.Text = "Waiting to Start";
                            
                            Screens.DllErrorScreen.RemoveAllHandles();

                        });
                        goto retry;

                    case ErrorScreenResultEnum.cancel:
                    case ErrorScreenResultEnum.copy:
                        Application.Current.Dispatcher.Invoke(()
      =>
                        {

                            Screens.SetScreen(Screens.MainScreen);
                            ProgressValue = 0;
                            progressRing.Value = 0;
                            Status.Text = "Waiting to Start";
                            Screens.DllErrorScreen.RemoveAllHandles();

                        });
                        break;



                }

            }
        exit:;
            Dispatcher.Invoke(() =>
            {
                var currentScreen = Screens.GetScreen();
                if (!ReferenceEquals(currentScreen, this))
                {
                    Screens.AddNotificationToQueue("Install DLL Method", Status.Text);
                }
            });
            Methods.Method.ProgressChanged -= DllMethod_ProgressChanged;

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
