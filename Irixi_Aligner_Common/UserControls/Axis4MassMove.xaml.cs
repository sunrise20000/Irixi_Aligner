using Irixi_Aligner_Common.Classes.BaseClass;
using Irixi_Aligner_Common.MotionControllerEntities.BaseClass;
using System.Windows;
using System.Windows.Controls;
using System;
using System.Windows.Data;
using System.Globalization;

namespace Irixi_Aligner_Common.UserControls
{
    /// <summary>
    /// Interaction logic for Axis4MassMove.xaml
    /// </summary>
    public partial class Axis4MassMove : UserControl
    {
        public Axis4MassMove()
        {
            InitializeComponent();

            PresetAggregation = new PresetPositionItem()
            {
                Id = 0,
                IsAbsMode = true,
                Position = 0.0,
                Speed = 100,
                MoveOrder = 1
            };
        }

        #region Properties


        /// <summary>
        /// Get the total axes to fill the order combobox
        /// </summary>
        public int TotalAxes
        {
            get { return (int)GetValue(TotalAxesProperty); }
            set { SetValue(TotalAxesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TotalAxes.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TotalAxesProperty =
            DependencyProperty.Register("TotalAxes", typeof(int), typeof(Axis4MassMove), new PropertyMetadata(0, OnTotalAxesChanged));

        private static void OnTotalAxesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Axis4MassMove owner)
            {
                owner.cmbOrder.Items.Clear();

                for (int i = 1; i <= (int)e.NewValue; i++)
                {
                    owner.cmbOrder.Items.Add(i);
                }

                owner.cmbOrder.SelectedIndex = 0;
                owner.Order = 1;
            }
        }



        public string AxisName
        {
            get { return (string)GetValue(AxisNameProperty); }
            set { SetValue(AxisNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AxisName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AxisNameProperty =
            DependencyProperty.Register("AxisName", typeof(string), typeof(Axis4MassMove), new PropertyMetadata(""));


        public double Position
        {
            get { return (double)GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Position.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register("Position", typeof(double), typeof(Axis4MassMove), new PropertyMetadata(0.0, OnPositionChanged));

        private static void OnPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Axis4MassMove owner)
            {
                owner.PresetAggregation.Position = (double)e.NewValue;
            }
        }

        public bool IsAbsMode
        {
            get { return (bool)GetValue(IsAbsModeProperty); }
            set { SetValue(IsAbsModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsAbs.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsAbsModeProperty =
            DependencyProperty.Register("IsAbsMode", typeof(bool), typeof(Axis4MassMove), new PropertyMetadata(true, OnIsAbsModeChanged));

        private static void OnIsAbsModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Axis4MassMove owner)
            {
                if ((bool)e.NewValue == true)
                    owner.btnIsAbsMode.Content = "ABS";
                else
                    owner.btnIsAbsMode.Content = "REL";

                owner.PresetAggregation.IsAbsMode = (bool)e.NewValue;
            }
        }

        public int Speed
        {
            get { return (int)GetValue(SpeedProperty); }
            set { SetValue(SpeedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Speed.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SpeedProperty =
            DependencyProperty.Register("Speed", typeof(int), typeof(Axis4MassMove), new PropertyMetadata(100, OnSpeedChanged));

        private static void OnSpeedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Axis4MassMove owner)
            {
                owner.PresetAggregation.Speed = (int)e.NewValue;
            }
        }

        /// <summary>
        /// Get or set the move order
        /// </summary>
        public int Order
        {
            get { return (int)GetValue(OrderProperty); }
            set { SetValue(OrderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Order.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OrderProperty =
            DependencyProperty.Register("Order", typeof(int), typeof(Axis4MassMove), new PropertyMetadata(1, OnOrderChanged));

        private static void OnOrderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Axis4MassMove owner)
            {
                owner.PresetAggregation.MoveOrder = (int)e.NewValue;
            }
        }

        public LogicalAxis LogicalAxis
        {
            get { return (LogicalAxis)GetValue(LogicalAxisProperty); }
            set { SetValue(LogicalAxisProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LogicalAxis.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LogicalAxisProperty =
            DependencyProperty.Register("LogicalAxis", typeof(LogicalAxis), typeof(Axis4MassMove), new PropertyMetadata(null, OnLogicalAxisChanged));

        private static void OnLogicalAxisChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Axis4MassMove owner)
            {
                owner.PresetAggregation.HashCode = ((LogicalAxis)e.NewValue).GetHashCode();
            }
        }

        /// <summary>
        /// Get the preset position aggregation
        /// </summary>
        public PresetPositionItem PresetAggregation
        {
            private set;
            get;
        }
        #endregion
    }

    class ConvertSelectItemsToMoveOrder : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(int.TryParse(value.ToString(), out int order))
            {
                return order;
            }
            else
            {
                return -1;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }
    }
}
