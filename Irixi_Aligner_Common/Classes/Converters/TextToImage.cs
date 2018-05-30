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
    public class TextToImage : IValueConverter
    {
        private BitmapFrame image = null;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            switch (value.ToString())
            {
                case "Axis":
                    image = (BitmapFrame)Application.Current.TryFindResource("Axis_24x24");
                    break;
                case "IO":
                    image = (BitmapFrame)Application.Current.TryFindResource("Io_24x24");
                    break;
                case "System":
                    image = (BitmapFrame)Application.Current.TryFindResource("System_24x24");
                    break;
                case "Equipment":
                    image = (BitmapFrame)Application.Current.TryFindResource("Equipment_24x24");
                    break;
                case "ENUM":
                    image = (BitmapFrame)Application.Current.TryFindResource("Enum_24x24");
                    break;
                default:
                    image = (BitmapFrame)Application.Current.TryFindResource("Function_16x16");
                    break;
            }
            return image;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
