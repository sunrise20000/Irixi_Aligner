using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace StepperControllerDebuger.Converter
{
    public class CvtBooleanToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (bool.TryParse(value.ToString(), out bool ret))
            {
                return ret ? new SolidColorBrush(Colors.Lime) : new SolidColorBrush(Colors.Red);
            }
            else
                return new SolidColorBrush(Colors.DimGray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
