using System.Windows.Data;
using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MCenters
{
    
    public enum InstallScreenModeEnum { Start, Uninstall }
    public enum ErrorTypeEnum { Common, ReportDll, NetworkError, FileError };
    enum ErrorScreenResultEnum { pending, cancel, copy, retry }

    public static class Functions
    {
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
    }
    public class ColorBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double progress = (double)value;
            if (progress == 0.0)
                return "";
            if (progress == 100.0)
                return "Completed";
            return ((int)progress).ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    class MCentersFileStream : FileStream
    {
        public MCentersFileStream(string path, FileMode mode) : base(path, mode)
        {

        }

        public void Write(byte[] array)
        {
            base.Write(array, 0, array.Length);
        }
    }

}
