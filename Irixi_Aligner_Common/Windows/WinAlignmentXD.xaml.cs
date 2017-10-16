using Irixi_Aligner_Common.AlignmentArithmetic;
using Irixi_Aligner_Common.Interfaces;
using Irixi_Aligner_Common.MotionControllerEntities.BaseClass;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Irixi_Aligner_Common.Windows
{
    /// <summary>
    /// Interaction logic for WinAlignmentXD.xaml
    /// </summary>
    public partial class WinAlignmentXD : Window
    {
        public WinAlignmentXD()
        {
            InitializeComponent();
        }
    }

    //public class ConvertToAlignmentXDArgs : IMultiValueConverter
    //{
    //    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        LogicalMotionComponent mc = null;
    //        if (values[0] != null)
    //            mc = values[0] as LogicalMotionComponent;

    //        IMeasurementInstrument instr = null;
    //        if (values[1] != null)
    //            instr = values[1] as IMeasurementInstrument;

    //        double.TryParse(values[2].ToString(), out double target);
    //        int.TryParse(values[3].ToString(), out int max_cycle);

    //        ObservableCollection<Alignment1DArgs> axis_params = null;
    //        if (values[4] != null)
    //        {
    //            axis_params = new List<Alignment1DArgs>((ObservableCollection<Alignment1DArgs>)values[4]);
    //        }

    //        var alignmentxd_args = new AlignmentXDArgs()
    //        {
    //            Instrument = instr,
    //            MotionComponent = mc,
    //            Target = target,
    //            MaxCycles = max_cycle,
    //            AxisParamCollection = axis_params
    //        };

    //        return alignmentxd_args;
    //    }

    //    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}
