using System.Windows.Data;
using System;
using System.IO;

namespace MCenters
{
    public enum InstallScreenModeEnum { Start, Uninstall }
    public enum ErrorTypeEnum { Common, ReportDll, NetworkError, FileError };
    enum ErrorScreenResultEnum { pending, cancel, copy, retry }
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
