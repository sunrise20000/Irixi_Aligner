using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Irixi_Aligner_Common.Interfaces;
using System.Threading;
using System.ComponentModel;
using Irixi_Aligner_Common.Configuration;
using Irixi_Aligner_Common.Classes.BaseClass;

namespace Irixi_Aligner_Common.MotionControllerEntities
{
    public class AxisBase : IAxis, INotifyPropertyChanged
    {
        #region Variables
        int _abs_pos = 0, _rel_pos = 0;
        int _cwl = 0, _ccwl = 0;
        bool
            _is_enabled = false,
            _is_homed = false,
            _is_manual_enabled = false,
            _is_abs_mode = false,
            _is_busy = false;

        /// <summary>
        /// determine whether the axis is not used by another thread
        /// </summary>
        SemaphoreSlim _axis_lock;

        #endregion

        #region Constructor
        public AxisBase()
        {
            _axis_lock = new SemaphoreSlim(1);
        }

        public AxisBase(int AxisIndex, ConfigPhysicalAxis Config, IMotionController Controller)
        {
            _axis_lock = new SemaphoreSlim(1);
            SetParameters(AxisIndex, Config, Controller);
        }
        
        #endregion

        #region Properties
        public int AxisIndex { get; private set; }

        public string AxisName { private set; get; }

        public bool IsBusy
        {
            get
            {
                return _is_busy;
            }
            set
            {
                UpdateProperty<bool>(ref _is_busy, value);
            }
        }

        public bool IsEnabled
        {
            get
            {
                return _is_enabled;
            }
            set
            {
                UpdateProperty<bool>(ref _is_enabled, value);
            }
        }

        public bool IsManualEnabled
        {
            set
            {
                UpdateProperty<bool>(ref _is_manual_enabled, value);
            }
            get
            {
                return _is_manual_enabled;
            }
        }

        public bool IsAbsMode
        {
            set
            {
                UpdateProperty<bool>(ref _is_abs_mode, value);
            }
            get
            {
                return _is_abs_mode;
            }
        }

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

        public int InitPosition { get; private set; }

        public int AbsPosition
        {
            get
            {
                return _abs_pos;
            }
            set
            {

                // calculate relative postion once the absolute position was changed
                this.RelPosition += (value - _abs_pos);

                UpdateProperty<int>(ref _abs_pos, value);

                // convert steps to real world distance
                this.UnitHelper.SetAbsPosition(_abs_pos);
            }
        }

        public int RelPosition
        {
            get
            {
                return _rel_pos;
            }
            private set
            {
                UpdateProperty<int>(ref _rel_pos, value);
            }
        }

        public object Tag { get; set; }

        public double MaxStroke { private set; get; }

        public int CWL
        {
            get
            {
                return _cwl;
            }
            set
            {
                UpdateProperty<int>(ref _cwl, value);
            }
        }

        public int CCWL
        {
            get
            {
                return _ccwl;
            }
            set
            {
                UpdateProperty<int>(ref _ccwl, value);
            }
        }

        public RealworldDistanceUnitHelper UnitHelper { protected set; get; }

        public string LastError { set; get; }

        public IMotionController ParentController { get; private set; }

        #endregion

        #region Methods
        /// <summary>
        /// the axis must be locked before using it.
        /// </summary>
        /// <returns></returns>
        public bool Lock()
        {
            return _axis_lock.Wait(100);
        }

        /// <summary>
        /// the axis must be released after using it.
        /// </summary>
        public void Unlock()
        {
            _axis_lock.Release();
        }

        public void SetParameters(int AxisIndex, ConfigPhysicalAxis Config, IMotionController Controller)
        {
            this.AxisIndex = AxisIndex;

            if (Config == null)
            {
                this.IsEnabled = false;
                this.AxisName = "Fake";
            }
            else
            {
                this.AxisName = Config.Name;
                this.IsEnabled = Config.Enabled;
                this.InitPosition = Config.OffsetAfterHome;
                this.CWL = Config.CWL;
                this.CCWL = Config.CCWL;
                this.MaxStroke = Config.MaxStroke;
                this.UnitHelper = new RealworldDistanceUnitHelper(Config.CWL, Config.MaxStroke, Config.Unit, Config.Digits);
                this.ParentController = Controller;
            }
        }
        
        public bool CheckSoftLimitation(int TargetPosition)
        {
            if (TargetPosition < this.CCWL || TargetPosition > this.CWL)
                return false;
            else
                return true;
        }

        public virtual Task<bool> Home()
        {
            return this.ParentController.Home(this);
        }

        public virtual Task<bool> Move(MoveMode Mode, int Speed, int Steps)
        {
            return this.ParentController.Move(this, Mode, Speed, Steps);
        }

        public virtual Task<bool> Move(MoveMode Mode, int Speed, double Distance)
        {
            return this.ParentController.Move(this, Mode, Speed, this.UnitHelper.ConvertToSteps(Distance));
        }

        public virtual Task<bool> MoveWithTrigger(MoveMode Mode, int Speed, int Steps, int Interval, int Channel)
        {
            return this.ParentController.MoveWithTrigger(
                this, 
                Mode, 
                Speed, 
                Steps, 
                Interval, 
                Channel);
        }

        public virtual Task<bool> MoveWithTrigger(MoveMode Mode, int Speed, double Distance, double Interval, int Channel)
        {
            return this.ParentController.MoveWithTrigger(
                this, 
                Mode, 
                Speed, 
                this.UnitHelper.ConvertToSteps(Distance),
                this.UnitHelper.ConvertToSteps(Interval), 
                Channel);
        }

        public virtual Task<bool> MoveWithInnerADC(MoveMode Mode, int Speed, int Steps, int Interval, int Channel)
        {
            return this.ParentController.MoveWithInnerADC(
                this, 
                Mode, 
                Speed, 
                Steps, 
                Interval, 
                Channel);
        }

        public virtual Task<bool> MoveWithInnerADC(MoveMode Mode, int Speed, double Distance, double Interval, int Channel)
        {
            return this.ParentController.MoveWithInnerADC(this,
                Mode,
                Speed,
                this.UnitHelper.ConvertToSteps(Distance),
                this.UnitHelper.ConvertToSteps(Interval),
                Channel);
        }

        public virtual void Stop()
        {
            this.ParentController.Stop();
        }

        public void ToggleMoveMode()
        {
            if(this.IsAbsMode)  // changed move mode from ABS to REL
            {
                ClearRelPosition();
                this.IsAbsMode = false;
            }
            else  // change move mode from REL to ABS
            {
                this.RelPosition = this.AbsPosition;
                this.IsAbsMode = true;
            }
        }

        public void ClearRelPosition()
        {
            this.RelPosition = 0;
        }

        public override string ToString()
        {
            return string.Format("*{0}@{1}*", this.AxisName, this.ParentController.DevClass);
        }
        #endregion

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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
                
        }
        #endregion
    }
}
