using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Irixi_Aligner_Common.Controls
{
    /// <summary>
    /// Axis.xaml 的交互逻辑
    /// </summary>
    public partial class Axis : UserControl
    {
        public Axis()
        {
            InitializeComponent();
        }

        #region DP Logical Axis


        public Configuration.ConfigLogicalAxis LogicalAxis
        {
            get { return (Configuration.ConfigLogicalAxis)GetValue(LogicalAxisProperty); }
            set { SetValue(LogicalAxisProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LogicalAxis.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LogicalAxisProperty =
            DependencyProperty.Register("LogicalAxis", typeof(Configuration.ConfigLogicalAxis), typeof(Axis), new PropertyMetadata(null));


        #endregion

        #region DP System Service Class


        public Irixi_Aligner_Common.Classes.SystemService SystemService
        {
            get { return (Irixi_Aligner_Common.Classes.SystemService)GetValue(SystemServiceProperty); }
            set { SetValue(SystemServiceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SystemService.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SystemServiceProperty =
            DependencyProperty.Register("SystemService", typeof(Irixi_Aligner_Common.Classes.SystemService), typeof(Axis), new PropertyMetadata(null));



        #endregion

        private void btnMove_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            MoveMode _mode = this.LogicalAxis.PhysicalAxisInst.IsAbsMode ? MoveMode.ABS : MoveMode.REL;
            string _direction = btn.Tag.ToString();

            // assert the parameters
            if (int.TryParse(txtMoveSpeed.Text, out int _speed) == false)
            {
                MessageBox.Show("The speed is not numerical.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (int.TryParse(txtDistance.Text, out int _distance) == false)
            {
                MessageBox.Show("The distance is not numerical.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                // if move backword, the distance should be minus
                if (_direction == "CCW")
                {
                    _distance *= -1;
                }

                // call the move function of the systemservice
                this.SystemService.MoveLogicalAxis(
                    this.LogicalAxis,
                    new Classes.BaseClass.MoveArgs()
                    {
                        Mode = _mode,
                        Speed = _speed,
                        Distance = _distance
                    });
            }
        }

        private void tbtnAbsMode_Click(object sender, RoutedEventArgs e)
        {
            this.SystemService.ToggleAxisMoveMode(this.LogicalAxis.PhysicalAxisInst);
        }
    }

    /// <summary>
    /// The converter converts the IsCheck value to the caption text of the ABS/REL toggle button
    /// </summary>
    class ConvertAbsModeToCaption : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool ret = (bool)value;

            // if IsChecked == true, the ABS mode is available
            if (ret)
            {
                return "ABS";
            }
            else
            {
                return "REL";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class ConvertBoolToSolidColorBrush : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool ret = (bool)value;
            if(ret == true)  // ABS mode
            {
                return new SolidColorBrush(Color.FromRgb(0xFF, 0x7A, 0x1E));
            }
            else
            {
                return new SolidColorBrush(Colors.Black);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// The converter converts the IsCheck value to the caption text of the Manual toggle button
    /// </summary>
    class ConvertManualEnabledToCaption : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool ret = (bool)value;

            // if IsChecked == true, the axis is locked
            if (ret)
            {
                return "LOCK";
            }
            else
            {
                return "MAN";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
