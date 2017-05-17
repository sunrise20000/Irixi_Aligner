using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace IrixiStepperControllerHelper
{

    public class AxisState : INotifyPropertyChanged
    {
        int _abs_position = 0;
        int _axis_index = 0;
        bool _is_homed = false, _is_busy = false, _cwls = false, _ccwls = false, _org = false, _zero_out = false, _in_a = false, _in_b = false;
        int _error = 0;

        /// <summary>
        /// Get the absolut position
        /// </summary>
        public int AbsPosition
        {
            internal set
            {
                UpdateProperty<int>(ref _abs_position, value);
            }
            get
            {
                return _abs_position;
            }
        }

        /// <summary>
        /// Get the index of the current axis
        /// </summary>
        public int AxisIndex
        {
            internal set
            {
                UpdateProperty<int>(ref _axis_index, value);
            }
            get
            {
                return _axis_index;
            }

        }

        /// <summary>
        /// Get whether the axis has been home
        /// </summary>
        public bool IsHomed
        {
            internal set
            {
                UpdateProperty<bool>(ref _is_homed, value);
            }
            get
            {
                return _is_homed;
            }
        }

        /// <summary>
        /// Get whether the axis is busy
        /// </summary>
        public bool IsRunning
        {
            internal set
            {
                UpdateProperty<bool>(ref _is_busy, value);
            }

            get
            {
                return _is_busy;
            }
        }

        /// <summary>
        /// Get the error code that the controller returned
        /// </summary>
        public int Error
        {
            internal set
            {
                UpdateProperty<int>(ref _error, value);
            }
            get
            {
                return _error;
            }
        }

        /// <summary>
        /// Get whether the CW limitation sensor has been touched
        /// </summary>
        public bool CWLS
        {
            internal set
            {
                UpdateProperty<bool>(ref _cwls, value);
            }
            get
            {
                return _cwls;
            }
        }

        /// <summary>
        /// Get whether the CCW limitation sensor has been touched
        /// </summary>
        public bool CCWLS
        {
            internal set
            {
                UpdateProperty<bool>(ref _ccwls, value);
            }
            get
            {
                return _ccwls;
            }
        }

        /// <summary>
        /// Get whether the Orginal Limitation Sensor has been touched
        /// </summary>
        public bool ORG
        {
            internal set
            {
                UpdateProperty<bool>(ref _org, value);
            }
            get
            {
                return _org;
            }
        }

        /// <summary>
        /// Get whether the ZeroOut Pusle (TIMING Signal) has been detected
        /// </summary>
        public bool ZeroOut
        {
            internal set
            {
                UpdateProperty<bool>(ref _zero_out, value);
            }
            get
            {
                return _zero_out;
            }
        }

        /// <summary>
        /// Get the status of the input A
        /// </summary>
        public bool IN_A
        {
            internal set
            {
                UpdateProperty<bool>(ref _in_a, value);
            }
            get
            {
                return _in_a;
            }
        }

        /// <summary>
        /// Get the status of the input A
        /// </summary>
        public bool IN_B
        {
            internal set
            {
                UpdateProperty<bool>(ref _in_b, value);
            }
            get
            {
                return _in_b;
            }
        }


        #region RaisePropertyChangedEvent

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="OldValue"></param>
        /// <param name="NewValue"></param>
        /// <param name="PropertyName"></param>
        protected void UpdateProperty<T>(ref T OldValue, T NewValue, [CallerMemberName]string PropertyName = "")
        {
            if (object.Equals(OldValue, NewValue))  // To save resource, if the value is not changed, do not raise the notify event
                return;

            OldValue = NewValue;                // Set the property value to the new value
            OnPropertyChanged(PropertyName);    // Raise the notify event
        }

        protected void OnPropertyChanged([CallerMemberName]string PropertyName = "")
        {
            //PropertyChangedEventHandler handler = PropertyChanged;
            //if (handler != null)
            //    handler(this, new PropertyChangedEventArgs(PropertyName));
            //RaisePropertyChanged(PropertyName);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));

        }
        #endregion
    }

    public class DeviceStateReport : INotifyPropertyChanged
    {
        uint _counter;
        int _total_axes;
        int _is_busy;
        int _sys_error;
        bool _triggerinput0, _triggerinput1;

        public DeviceStateReport()
        {
            this.AxisStateCollection = new ObservableCollection<AxisState>();
        }

        /// <summary>
        /// Get the number of the counter of the report pack
        /// </summary>
        public uint Counter
        {
            internal set
            {
                UpdateProperty<uint>(ref _counter, value);
            }
            get
            {
                return _counter;
            }
        }

        /// <summary>
        /// Get the tatal axes the controller supports
        /// </summary>
        public int TotalAxes
        {
            internal set
            {
                UpdateProperty<int>(ref _total_axes, value);
            }
            get
            {
                return _total_axes;
            }
        }

        /// <summary>
        /// Get how many axes are moving
        /// </summary>
        public int IsBusy
        {
            internal set
            {
                UpdateProperty<int>(ref _is_busy, value);
            }
            get
            {
                return _is_busy;
            }
        }

        /// <summary>
        /// Indicates that the emergency button was pressed or the value of IsBusy was out of range
        /// 30: Emergency Button was pressed
        /// 255: IsBusy was out of range
        /// </summary>
        public int SystemError
        {
            internal set
            {
                UpdateProperty<int>(ref _sys_error, value);
            }
            get
            {
                return _sys_error;
            }
        }

        /// <summary>
        /// Get the state of the trigger input 0
        /// true: triggered
        /// false: not triggered
        /// </summary>
        public bool TriggerInput0
        {
            internal set
            {
                UpdateProperty<bool>(ref _triggerinput0, value);
            }
            get
            {
                return _triggerinput0;
            }
        }

        /// <summary>
        /// Get the state of the trigger input 1
        /// true: triggered
        /// false: not triggered
        /// </summary>
        public bool TriggerInput1
        {
            internal set
            {
                UpdateProperty<bool>(ref _triggerinput1, value);
            }
            get
            {
                return _triggerinput1;
            }
        }

        public ObservableCollection<AxisState> AxisStateCollection { internal set; get; }

        #region RaisePropertyChangedEvent

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Update the specified property and fire the ProrpertyChanged event to update the UI
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="OldValue"></param>
        /// <param name="NewValue"></param>
        /// <param name="PropertyName"></param>
        protected void UpdateProperty<T>(ref T OldValue, T NewValue, [CallerMemberName]string PropertyName = "")
        {
            if (object.Equals(OldValue, NewValue))  // To save resource, if the value is not changed, do not raise the notify event
                return;

            OldValue = NewValue;                // Set the property value to the new value
            OnPropertyChanged(PropertyName);    // Raise the notify event
        }

        protected void OnPropertyChanged([CallerMemberName]string PropertyName = "")
        {
            
            // Implement MVVMLight framework
            // RaisePropertyChanged(PropertyName);

            // Implement INotifyPropertyChanged interface
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));

        }

        #endregion
    }
}
