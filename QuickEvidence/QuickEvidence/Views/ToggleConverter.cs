using System;
using System.Globalization;
using System.Windows.Data;

namespace QuickEvidence.Views
{
    public class ToggleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value != null && ((string)value).Equals((string)parameter))
            {
                return true;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return parameter;
            }
            return null;
        }
    }
}
