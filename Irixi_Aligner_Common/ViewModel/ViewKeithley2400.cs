using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Irixi_Aligner_Common.Equipments;
using System;
using System.Windows.Data;
using System.Globalization;

namespace Irixi_Aligner_Common.ViewModel
{
    public class ViewKeithley2400 : ViewModelBase
    {
        public enum EnumCurrDisplayUnit
        {
            A, mA, uA
        }

        public enum EnumVoltDisplayUnit
        {
            V, mV, uV
        }

        public ViewKeithley2400(Keithley2400 DeviceInstance)
        {
            this.K2400 = DeviceInstance;
        }

        #region Properties
        /// <summary>
        /// Get the instance of K2400
        /// </summary>
        public Keithley2400 K2400
        {
            private set;
            get;
        }

        EnumCurrDisplayUnit curr_unit = EnumCurrDisplayUnit.mA;
        public EnumCurrDisplayUnit DisplayUnitforCurrent
        {
            set
            {
                curr_unit = value;
                RaisePropertyChanged();
            }
            get
            {
                return curr_unit;
            }
        }

        EnumVoltDisplayUnit volt_unit = EnumVoltDisplayUnit.V;
        public EnumVoltDisplayUnit DisplayUnitforVoltage
        {
            set
            {
                volt_unit = value;
                RaisePropertyChanged();
            }
            get
            {
                return volt_unit;
            }
        }
        #endregion

        #region Commands
        /// <summary>
        /// Get the command to switch on/off the output
        /// </summary>
        public RelayCommand SetOutputEnabled
        {
            get
            {
                return new RelayCommand(() =>
                {
                    try
                    {
                        if (K2400.IsOutputEnabled == false)
                        {
                            K2400.SetOutputState(true);

                            // start fetch loop
                            K2400.StartAutoFetching();
                        }
                        else
                        {
                            // start fetch loop
                            K2400.StopAutoFetching();

                            K2400.SetOutputState(false);
                        }
                    }
                    catch (Exception ex)
                    {
                        Messenger.Default.Send(new NotificationMessage<string>(string.Format("unable to set output state, {0}", ex.Message), "ERROR"));
                    }
                });
            }
        }

        /// <summary>
        /// Switch In/Out terminal
        /// </summary>
        public RelayCommand<Keithley2400.EnumInOutTerminal> SetInOutTerminal
        {
            get
            {
                return new RelayCommand<Keithley2400.EnumInOutTerminal>(term =>
                {
                    try
                    {
                        K2400.SetInOutTerminal(term);
                    }
                    catch(Exception ex)
                    {
                        Messenger.Default.Send(new NotificationMessage<string>(string.Format("unable to switch In/Out terminal, {0}", ex.Message), "ERROR"));
                    }
                });
            }
        }

        /// <summary>
        /// Get the command to switch the mode to voltage source
        /// </summary>
        public RelayCommand SetToVoltageSource
        {
            get
            {
                return new RelayCommand(() =>
                {
                    try
                    {
                        K2400.SetToVoltageSource();
                    }
                    catch(Exception ex)
                    {
                        Messenger.Default.Send(new NotificationMessage<string>(string.Format("unable to switch to voltage source, {0}", ex.Message), "ERROR"));
                    }
                });
            }
        }

        /// <summary>
        /// Get the command to switch the mode to current source
        /// </summary>
        public RelayCommand SetToCurrentSource
        {
            get
            {
                return new RelayCommand(() =>
                {
                    try
                    {
                        K2400.SetToCurrentSource();
                    }
                    catch(Exception ex)
                    {
                        Messenger.Default.Send(new NotificationMessage<string>(string.Format("unable to switch to current source, {0}", ex.Message), "ERROR"));
                    }
                });
            }
        }

        /// <summary>
        /// Set compliance per current source mode
        /// </summary>
        public RelayCommand<double> SetCompliance
        {
            get
            {
                return new RelayCommand<double>(target =>
                {
                    try
                    {
                        if (K2400.SourceMode == Keithley2400.EnumSourceMode.CURR)
                        {
                            K2400.SetComplianceCurrent(Keithley2400.EnumComplianceLIMIT.REAL, target);
                        }
                        else if (K2400.SourceMode == Keithley2400.EnumSourceMode.VOLT)
                        {
                            K2400.SetComplianceVoltage(Keithley2400.EnumComplianceLIMIT.REAL, target);
                        }
                    }
                    catch(Exception ex)
                    {
                        Messenger.Default.Send(new NotificationMessage<string>(string.Format("unable to set compliance, {0}", ex.Message), "ERROR"));
                    }
                });
            }
        }

        /// <summary>
        /// Set measurement range per current measurement function
        /// </summary>
        public RelayCommand<double> SetMeasurementRange
        {
            get
            {
                return new RelayCommand<double>(target =>
                {
                    try
                    {
                        if (K2400.MeasurementFunc == Keithley2400.EnumMeasFunc.ONCURR)
                        {
                            K2400.SetMeasRangeOfAmps(Keithley2400.EnumMeasRange.REAL, target);
                        }
                        else if (K2400.MeasurementFunc == Keithley2400.EnumMeasFunc.ONVOLT)
                        {
                            K2400.SetMeasRangeOfVolts(Keithley2400.EnumMeasRange.REAL, target);
                        }
                    }
                    catch(Exception ex)
                    {
                        Messenger.Default.Send(new NotificationMessage<string>(string.Format("unable to set measurement range, {0}", ex.Message), "ERROR"));
                    }
                });
            }
        }

        #endregion
    }

    public class FormatMeasurementValue : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            string ret = "";

            if(double.TryParse(value[0].ToString(), out double measval))
            {
                var func = (Keithley2400.EnumMeasFunc)value[1];
                var unit_curr = (ViewKeithley2400.EnumCurrDisplayUnit)value[2];
                var unit_volt = (ViewKeithley2400.EnumVoltDisplayUnit)value[3];

                if (func == Keithley2400.EnumMeasFunc.ONCURR)
                {
                    switch(unit_curr)
                    {
                        case ViewKeithley2400.EnumCurrDisplayUnit.A:
                            ret = string.Format("{0:F6} {1}", measval, unit_curr);
                            break;

                        case ViewKeithley2400.EnumCurrDisplayUnit.mA:
                            ret = string.Format("{0:F6} {1}", measval * 1000, unit_curr);
                            break;

                        case ViewKeithley2400.EnumCurrDisplayUnit.uA:
                            ret = string.Format("{0:F6} {1}", measval * 1000000, unit_curr);
                            break;
                    }
                }
                else if (func == Keithley2400.EnumMeasFunc.ONVOLT)
                {
                    switch (unit_volt)
                    {
                        case ViewKeithley2400.EnumVoltDisplayUnit.V:
                            ret = string.Format("{0:F6} {1}", measval, unit_volt);
                            break;

                        case ViewKeithley2400.EnumVoltDisplayUnit.mV:
                            ret = string.Format("{0:F6} {1}", measval * 1000, unit_volt);
                            break;

                        case ViewKeithley2400.EnumVoltDisplayUnit.uV:
                            ret = string.Format("{0:F6} {1}", measval * 1000000, unit_volt);
                            break;
                    }
                }
                else
                {
                    ret = "Unknown Func";
                }

                return ret;
            }
            else
            {
                return "ERROR";
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
