using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Irixi_Aligner_Common.Equipments;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace Irixi_Aligner_Common.ViewModel
{
    public class ViewNewport2832C : ViewModelBase
    {
        #region Variables

        Newport2832C.EnumRange range;
        Newport2832C.EnumUnits unit;
        int lambda;

        #endregion

        #region Constructors

        public ViewNewport2832C(Newport2832C DeviceInstance)
        {
            this.InstrInstance = DeviceInstance;
        }

        #endregion

        #region Properties

        public string[] Ranges
        {
            get
            {
                return Enum.GetNames(typeof(Newport2832C.EnumRange));
            }
        }

        public string[] Units
        {
            get
            {
                return Enum.GetNames(typeof(Newport2832C.EnumUnits));
            }
        }

        public Newport2832C InstrInstance { get; }

       
        #endregion

        #region Commands

        public RelayCommand SetActiveChannelToA
        {
            get
            {
                return new RelayCommand (() =>
                {
                    try
                    {
                        InstrInstance.SetDisplayChannel(Newport2832C.EnumChannel.A);
                    }
                    catch (Exception ex)
                    {
                        Messenger.Default.Send(new NotificationMessage<string>(string.Format("unable to set active channel to A, {0}", ex.Message), "ERROR"));
                    }
                });
            }
        }

        public RelayCommand SetActiveChannelToB
        {
            get
            {
                return new RelayCommand(() =>
                {
                    try
                    {
                        InstrInstance.SetDisplayChannel(Newport2832C.EnumChannel.B);
                    }
                    catch (Exception ex)
                    {
                        Messenger.Default.Send(new NotificationMessage<string>(string.Format("unable to set active channel to B, {0}", ex.Message), "ERROR"));
                    }
                });
            }
        }

        public RelayCommand<int> SetLambdaA
        {
            get
            {
                return new RelayCommand<int>(lambda =>
                {
                    int snap = InstrInstance.MetaProperties[(int)Newport2832C.EnumChannel.A].Lambda;

                    try
                    {
                        InstrInstance.SetLambda(Newport2832C.EnumChannel.A, lambda);
                    }
                    catch (Exception ex)
                    {
                        Messenger.Default.Send(new NotificationMessage<string>(string.Format("Unable to set unit, {0}", ex.Message), "ERROR"));

                        InstrInstance.SetLambda(Newport2832C.EnumChannel.A, snap);
                    }
                });
            }
        }

        public RelayCommand<int> SetLambdaB
        {
            get
            {
                return new RelayCommand<int>(lambda =>
                {
                    try
                    {
                        InstrInstance.SetLambda(Newport2832C.EnumChannel.B, lambda);
                    }
                    catch (Exception ex)
                    {
                        Messenger.Default.Send(new NotificationMessage<string>(string.Format("Unable to set unit, {0}", ex.Message), "ERROR"));
                    }
                });
            }
        }

        public RelayCommand<Newport2832C.EnumRange> SetRangeA
        {
            get
            {
                return new RelayCommand<Newport2832C.EnumRange>(range =>
                {
                    try
                    {
                        InstrInstance.SetMeasurementRange(Newport2832C.EnumChannel.A, range);
                    }
                    catch (Exception ex)
                    {
                        Messenger.Default.Send(new NotificationMessage<string>(string.Format("Unable to set range, {0}", ex.Message), "ERROR"));
                    }
                });
            }
        }

        public RelayCommand<Newport2832C.EnumRange> SetRangeB
        {
            get
            {
                return new RelayCommand<Newport2832C.EnumRange>(range =>
                {
                    try
                    {
                        InstrInstance.SetMeasurementRange(Newport2832C.EnumChannel.B, range);
                    }
                    catch (Exception ex)
                    {
                        Messenger.Default.Send(new NotificationMessage<string>(string.Format("Unable to set range, {0}", ex.Message), "ERROR"));
                    }
                });
            }
        }


        public RelayCommand<Newport2832C.EnumUnits> SetUnitA
        {
            get
            {
                return new RelayCommand<Newport2832C.EnumUnits>(unit =>
                {
                    try
                    {
                        InstrInstance.SetUnit(Newport2832C.EnumChannel.A, unit);
                    }
                    catch (Exception ex)
                    {
                        Messenger.Default.Send(new NotificationMessage<string>(string.Format("Unable to set unit, {0}", ex.Message), "ERROR"));
                    }
                });
            }
        }

        public RelayCommand<Newport2832C.EnumUnits> SetUnitB
        {
            get
            {
                return new RelayCommand<Newport2832C.EnumUnits>(unit =>
                {
                    try
                    {
                        InstrInstance.SetUnit(Newport2832C.EnumChannel.B, unit);
                    }
                    catch (Exception ex)
                    {
                        Messenger.Default.Send(new NotificationMessage<string>(string.Format("Unable to set unit, {0}", ex.Message), "ERROR"));
                    }
                });
            }
        }

        #endregion
    }

    /// <summary>
    /// Auto tuning the unit of the measurement value in this class
    /// </summary>
    public class FormatNewport2832CMeasurementValue : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            string ret = "", prefix = "";
            Newport2832C.EnumUnits unit;

            if (double.TryParse(value[0].ToString(), out double measval))
            {
                unit = (Newport2832C.EnumUnits)value[1];

                if (unit == Newport2832C.EnumUnits.A || unit == Newport2832C.EnumUnits.W)
                {
                    // assuming the incoming value is in A or W
                    if (measval < 0.00000105) // convert to nA/nW     
                    {
                        measval *= 1000000000;
                        prefix = "n";
                    }
                    else if (measval < 0.00105) // convert to uA/uW     
                    {
                        measval *= 1000000;
                        prefix = "u";
                    }
                    else if (measval < 1.05) // convert to mA/mW
                    {
                        measval *= 1000;
                        prefix = "m";
                    }

                    ret = string.Format("{0:F6} {1}{2}", measval, prefix, unit);
                }
                else
                {
                    ret = string.Format("{0} {1}", measval, unit);
                }

                return ret;
            }
            else
            {
                return "MEAS ERR";
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
