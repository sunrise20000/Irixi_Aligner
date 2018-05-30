using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Irixi_Aligner_Common.Classes.Converters
{
    public class SystemStatusToScriptCompileButtonImage : IValueConverter
    {
        private BitmapFrame image = null;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ScriptState state = (ScriptState)value;
            if (state== ScriptState.IDLE)
                image=(BitmapFrame)Application.Current.TryFindResource("Scriptcompile_32x32");
            else
                image = (BitmapFrame)Application.Current.TryFindResource("Scriptcompile_dis32x32");
            return image;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
