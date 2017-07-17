using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Irixi_Aligner_Common.Configuration
{
    public class WorkSpaceLayout
    {
        public PanelLayout PanelLocation { get; set; }

    }

    class ConvertStringToPoint : IValueConverter
    {
        /// <summary>
        /// Convert String to MDILocation(Type of System.Point)
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string _str_location = value.ToString();
            try
            {
                string[] _point = _str_location.Split(',');
                if (_point.Length != 2)
                {
                    return new Point(0, 0);
                }
                else
                {
                    if (double.TryParse(_point[0], out double x) == false)
                        x = 0;

                    if (double.TryParse(_point[1], out double y) == false)
                        y = 0;

                    return new Point(x, y);
                }
            }
            catch
            {
                return new Point(0, 0);
            }
        }

        /// <summary>
        /// Convert MDILocation to String
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Point _point = (Point)value;

            return string.Format("{0},{1}", _point.X, _point.Y);

        }
    }
}
