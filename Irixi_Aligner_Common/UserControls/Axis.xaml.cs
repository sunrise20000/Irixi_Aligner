using Irixi_Aligner_Common.Classes;
using Irixi_Aligner_Common.Classes.BaseClass;
using Irixi_Aligner_Common.Configuration;
using Irixi_Aligner_Common.MotionControllerEntities.BaseClass;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Irixi_Aligner_Common.UserControls
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


        public LogicalAxis LogicalAxis
        {
            get { return (LogicalAxis)GetValue(LogicalAxisProperty); }
            set { SetValue(LogicalAxisProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LogicalAxis.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LogicalAxisProperty =
            DependencyProperty.Register("LogicalAxis", typeof(LogicalAxis), typeof(Axis), new PropertyMetadata());

        #endregion

        #region Events
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
            else if (double.TryParse(txtDistance.Text, out double _distance) == false)
            {
                MessageBox.Show("The distance is not numerical.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                // convert distance to stpes
                int steps = this.LogicalAxis.PhysicalAxisInst.UnitHelper.ConvertToSteps(_distance);

                // if move backword, the distance should be minus
                if (_direction == "CCW")
                {
                    steps *= -1;
                }

                // call the move function of the systemservice
                this.LogicalAxis.Service.MoveLogicalAxis(
                    this.LogicalAxis,
                    new Classes.BaseClass.MoveArgs()
                    {
                        Mode = _mode,
                        Speed = _speed,
                        Distance = steps
                    });
            }
        }

        private void tbtnAbsMode_Click(object sender, RoutedEventArgs e)
        {
            this.LogicalAxis.Service.ToggleAxisMoveMode(this.LogicalAxis.PhysicalAxisInst);
        }

        #endregion
    }

    class AxisUserControlViewModel
    {
        public SystemService Service { get; set; }
        public ConfigLogicalAxis LogicalAxis { get; set; }
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
                return "Y";
            }
            else
            {
                return "N";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Convert steps to real world distance
    /// </summary>
    class ConvertStepsToRealWorldDistance : IMultiValueConverter
    {

        public object Convert(object []value, Type targetType, object parameter, CultureInfo culture)
        {
            // position in steps
            if (int.TryParse(value[0].ToString(), out int pos))
            {
                if (value[1] is RealworldDistanceUnitHelper helper)
                {
                    return helper.ConvertToRealworldDistance(pos).ToString();
                }
                else
                {
                    return "Error";
                }
            }
            else
            {
                return "Design Mode";
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
