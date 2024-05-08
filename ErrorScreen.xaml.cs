using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MCenters
{
    /// <summary>
    /// Interaction logic for ErrorScreen.xaml
    /// </summary>
    /// 


    public partial class ErrorScreen : Page
    {

        public ErrorScreen()
        {
            InitializeComponent();
            cancelButton.ConnectedImage = cancelIcon;
            copyButton.ConnectedImage = copyIcon;
            retryButton.ConnectedImage = retryIcon;
        }
        public void RemoveRetryHandles()
        {
            if (RetryClicked == null)
                return;
            try
            {
                foreach (var g in RetryClicked.GetInvocationList())
                {
                    RetryClicked -= (EventHandler)g;

                }
            }
            catch (System.NullReferenceException) { }
        }
        public string ErrorTitle { get { return errorTitle.Text; } set { errorTitle.Text = value; } }
        public string ErrorSubTitle { get { return errorLine.Text; } set { errorLine.Text = value; } }
        public string ErrorDescription { get { return errorDescription.Text; } set { errorDescription.Text = value; } }
        public event EventHandler CopyClicked;
        public event EventHandler RetryClicked;
        public event EventHandler CancelClicked;
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            CancelClicked?.Invoke(null, null);
        }
        
        readonly ErrorTypeEnum mode = ErrorTypeEnum.Common;
        public ErrorTypeEnum CurrentMode
        {
            get { return mode; }
            set
            {
                if (value == mode)
                    return;
                switch (value)
                {



                    case ErrorTypeEnum.ReportDll:
                        copyButton.Content = "Send Dlls to M Centers";
                        break;
                    default:
                        copyButton.Content = "Copy to ClipBoard";
                        break;

                }

            }
        }
        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            CopyClicked?.Invoke(null, null);
        }



        private void RetryButton_Click(object sender, RoutedEventArgs e)
        {
            RetryClicked?.Invoke(null, null);
        }




    }
}
