using IrixiStepperControllerHelper;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Irixi_Aligner_Common.Classes.Converters
{
    public class OutputStateToBoolean : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            OutputState v = (OutputState)value;
            if (v == OutputState.Disabled)
                return false;
            else
                return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
