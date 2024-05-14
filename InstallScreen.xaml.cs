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
            if (result == InvokeResults.errorOccured) return;
            
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
            if (systemDllVersionCheckTaskResult == InvokeResults.errorOccured) return;





            bool isVersionAvailable = false;
            var DllMethodAvailabitityCheckerTask = new MCenterTask(() =>
            {


                isVersionAvailable = Methods.DllMethod.IsAvailable(systemDllVersion);


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
            if (DllMethodAvailabitityCheckerTaskResult == InvokeResults.errorOccured) return;

            if (isVersionAvailable)
            {
                Methods.Method method = new Methods.DllMethod(systemDllVersion);
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
                    if (DllMethodDownloaderTaskResult == InvokeResults.errorOccured) return;




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
                if (dllMethodInstallTaskResult == InvokeResults.errorOccured) return;
                

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
                    Screens.DllErrorScreen.ErrorTitle = "Unsupported Version " + systemDllVersion;
                    Screens.DllErrorScreen.ErrorSubTitle = "";
                    Screens.DllErrorScreen.ErrorDescription = "MCenters currently does not support your version of Windows.\nYou can retry, if you think it was a network issue.\nIf this is not a network issue then hit Submit Dlls to report your version to MCenters";
                    Screens.SetScreen(Screens.DllErrorScreen);

                });
                Version = systemDllVersion;
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

            StringCollection files = new StringCollection();


            if (File.Exists(Methods.Method.Dllx64))
            {
                var fileName = $"{Version} x64.dll";
                fileName = Path.Combine(Methods.Method.ClipboardFolder, fileName);


                if (File.Exists(fileName))
                {
                    var task = new MCenterTask(() => File.Delete(fileName))
                    {
                        ErrorDescriptionBuilder =
                                                 (ex) => { return $"An error occured while deleting ${fileName}"; }
                    };
                retry:;

                    var result = await task.Invoke(
                               new Type[] { typeof(IOException), typeof(UnauthorizedAccessException)
                               });
                    while (result == InvokeResults.busy) goto retry;
                    if (result == InvokeResults.errorOccured) return;

                }
                File.Copy(Methods.Method.Dllx64, fileName);
                await Task.Delay(15000);
                files.Add(fileName);
            }

            if (File.Exists(Methods.Method.Dllx86))
            {
                var fileName = $"{Version} x86.dll";
                fileName = Path.Combine(Methods.Method.ClipboardFolder, fileName);
                if (File.Exists(fileName))
                {

                    var task = new MCenterTask(() => File.Delete(fileName))
                    {
                        ErrorDescriptionBuilder =
                                              (ex) => { return $"An error occured while deleting ${fileName}"; }
                    };
                retry:;

                    var result = await task.Invoke(
                               new Type[] { typeof(IOException), typeof(UnauthorizedAccessException)
                               });
                    while (result == InvokeResults.busy) goto retry;
                    if (result == InvokeResults.errorOccured) return;




                }
                File.Copy(Methods.Method.Dllx86, fileName);
                files.Add(fileName);
            }
            Clipboard.SetFileDropList(files);
            response = ErrorScreenResultEnum.copy;
            Functions.OpenBrowser("https://discord.gg/sU8qSdP5wP");
            return;


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
