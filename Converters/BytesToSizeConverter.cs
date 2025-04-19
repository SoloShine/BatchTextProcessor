
using System;
using System.Globalization;
using System.Windows.Data;

namespace BatchTextProcessor.Converters
{
    public class BytesToSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "0 B";
            
            long bytes = System.Convert.ToInt64(value);
            const int scale = 1024;
            string[] units = { "B", "KB", "MB", "GB" };
            
            if (bytes == 0) return "0 B";
            
            int mag = (int)Math.Log(bytes, scale);
            double adjustedSize = Math.Round(bytes / Math.Pow(scale, mag), 2);
            
            return $"{adjustedSize} {units[mag]}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
