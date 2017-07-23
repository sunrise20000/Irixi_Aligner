using Irixi_Aligner_Common.Message;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Irixi_Aligner_Common.Classes.Converters
{
    public class MessageTypeToImage : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            BitmapImage image = null;

            MessageType state = (MessageType)value;
            switch (state)
            {
                case MessageType.Normal:
                    image = image = new BitmapImage();
                    break;

                case MessageType.Good:
                    image = new BitmapImage(new Uri("pack://application:,,,/Resources/images/icons/right.png"));
                    break;

                case MessageType.Warning:
                    image = new BitmapImage(new Uri("pack://application:,,,/Resources/images/icons/warning_32x32.png"));
                    break;

                case MessageType.Error:
                    image = new BitmapImage(new Uri("pack://application:,,,/Resources/images/icons/wrong.png"));
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
