using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MCenters
{
    delegate string MCenterTaskStringBuilderDelegate(Exception exception);
    enum InvokeResults { busy, errorOccured, completed }
    internal class MCenterTask
    {
        private static Action CurrentlyRunningAction;
        private readonly Action Action;
        public MCenterTaskStringBuilderDelegate ErrorDescriptionBuilder { get; set; }
        public MCenterTaskStringBuilderDelegate ErrorTitleBuilder { get; set; }
        public MCenterTaskStringBuilderDelegate ErrorSubtitleBuilder { get; set; }


        public MCenterTask(Action newAction)
        {
            Action = newAction;

        }



        public async Task<InvokeResults> Invoke(Type[] handledExceptionTypes = null, bool HandleAllExceptions = false, bool retryable = true)
        {

            if (CurrentlyRunningAction != null) return InvokeResults.busy;
            CurrentlyRunningAction = Action;


            if (handledExceptionTypes == null) handledExceptionTypes = new Type[0];

            retry:;
            try
            {
                Action();
                CurrentlyRunningAction = null;
                return InvokeResults.completed;

            }
            catch (Exception ex) when ((HandleAllExceptions && IsCriticalException(ex)) || handledExceptionTypes.Contains(ex.GetType()))
            {
                //this code is for exceptions handled by handledexceptiontypes

                Screens.ErrorScreen.ErrorTitle = ErrorTitleBuilder == null ? ex.HResult.ToString() : ErrorTitleBuilder(ex);
                Screens.ErrorScreen.ErrorSubTitle = ErrorSubtitleBuilder == null ? ex.Message : ErrorSubtitleBuilder(ex);
                Screens.ErrorScreen.ErrorDescription = ErrorDescriptionBuilder == null ? ex.StackTrace : ErrorDescriptionBuilder(ex);
                Screens.ErrorScreen.copyButton.IsEnabled = true;
                Screens.ErrorScreen.copyButton.Content = "Copy and Open Discord";

                Screens.ErrorScreen.retryButton.Visibility = retryable ? Visibility.Visible : Visibility.Hidden;
                bool? ShouldRetry = null;


                var innerEx = ex.InnerException ?? null;


                var innerExceptionInfo = innerEx == null ? null : new { Type = innerEx.GetType().FullName, innerEx.HResult, innerEx.StackTrace };
                var innerExceptionInfoJson = innerExceptionInfo == null ? null :
$@"{{
          ""Type"":    ""{innerExceptionInfo.Type}"",
       ""HResult"":    ""{innerExceptionInfo.HResult}"",
    ""StackTrace"":    ""{innerExceptionInfo.StackTrace}""
                    
   }}
";
                var exceptionInfo = new { Type = ex.GetType().FullName, ex.HResult, ex.StackTrace };
                var clipboardMessage =
$@"```json
{{
          ""Type"":    ""{exceptionInfo.Type}"",
       ""HResult"":    ""{exceptionInfo.HResult}"",
    ""StackTrace"":    ""{exceptionInfo.StackTrace}"",
""InnerException"":    ""{innerExceptionInfoJson}""                
}}
```
";



                Screens.ErrorScreen.CancelClicked += (s, eventE) =>
                {
                    ShouldRetry = false;

                };
                Screens.ErrorScreen.RetryClicked += (s, eventE) =>
                {
                    ShouldRetry = true;
                };
                Screens.ErrorScreen.CopyClicked += (s, eventE) =>
                {

                    Clipboard.SetText(clipboardMessage);
                    Functions.OpenBrowser("https://discord.gg/sU8qSdP5wP");
                };
                var content = Screens.Window.Content;
                Screens.SetScreen(Screens.ErrorScreen);
                while (ShouldRetry == null) await Task.Delay(100);
                Screens.SetScreen(content);
                Screens.ErrorScreen.Reset();
                if (ShouldRetry == true) goto retry;
                CurrentlyRunningAction = null;
                return InvokeResults.errorOccured;

            }
            catch (Exception ex) when (IsCriticalException(ex))
            {
                Screens.ErrorScreen.ErrorTitle = ex.HResult.ToString();
                Screens.ErrorScreen.ErrorSubTitle = ex.Message;
                Screens.ErrorScreen.ErrorDescription = ex.StackTrace;
                Screens.ErrorScreen.copyButton.IsEnabled = true;
                Screens.ErrorScreen.copyButton.Content = "Copy and Open Discord";

                Screens.ErrorScreen.retryButton.Visibility = retryable ? Visibility.Visible : Visibility.Hidden;
                bool? ShouldRetry = null;
                var innerEx = ex.InnerException ?? null;


                var innerExceptionInfo = innerEx == null ? null : new { Type = innerEx.GetType().FullName, innerEx.HResult, innerEx.StackTrace };
                var innerExceptionInfoJson = innerExceptionInfo == null ? null :
$@"{{
          ""Type"":    {innerExceptionInfo.Type},
       ""HResult"":    {innerExceptionInfo.HResult},
    ""StackTrace"":    {innerExceptionInfo.StackTrace}
                    
   }}
";
                var exceptionInfo = new { Type = ex.GetType().FullName, ex.HResult, ex.StackTrace };
                var clipboardMessage =
$@"```json
{{
          ""Type"":    {exceptionInfo.Type},
       ""HResult"":    {exceptionInfo.HResult},
    ""StackTrace"":    {exceptionInfo.StackTrace},
""InnerException"":    {innerExceptionInfoJson}                
}}
";
                Screens.ErrorScreen.CancelClicked += (s, eventE) =>
                {
                    ShouldRetry = false;

                };
                Screens.ErrorScreen.RetryClicked += (s, eventE) =>
                {
                    ShouldRetry = true;
                };
                Screens.ErrorScreen.CopyClicked += (s, eventE) =>
                {


                    Clipboard.SetText(clipboardMessage);
                    Functions.OpenBrowser("https://discord.gg/sU8qSdP5wP");
                };
                Screens.SetScreen(Screens.ErrorScreen);
                while (ShouldRetry == null) await Task.Delay(100);
                Screens.ErrorScreen.Reset();
                if (ShouldRetry == true) goto retry;
                CurrentlyRunningAction = null;
                return InvokeResults.errorOccured;
            }
        }



        public static bool IsCriticalException(Exception ex)
        {
            // Check if the exception is of type SystemException or ApplicationException
            // You can customize this logic based on specific types you consider critical
            return ex is SystemException || ex is ApplicationException;
        }

    }
}
