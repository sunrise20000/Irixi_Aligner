using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Irixi_Aligner_Common.Classes.Converters
{
    public class VisibilityToBoolean : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool? ret = null;
            if ((Visibility)value == Visibility.Visible)
                ret = true;
            else
                ret = false;

            return ret;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool? v = (bool?)value;
            if (v.HasValue && v.Value == true)
                return Visibility.Visible;
            else
                return Visibility.Hidden;
        }
    }
}
