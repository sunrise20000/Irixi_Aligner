using Irixi_Aligner_Common.Classes.BaseClass;
using Irixi_Aligner_Common.Configuration.MotionController;
using Irixi_Aligner_Common.Interfaces;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Irixi_Aligner_Common.MotionControllers.Base
{
    /*
     * NOTE: 
     * All parameters in this class that related position are in 'steps' except UnitHelper
     */
    public class AxisBase : IAxis, INotifyPropertyChanged
    {
        #region Variables
        int absPosition = 0, relPosition = 0;
        int scwl = 0, sccwl = 0;
        bool
            isEnabled = false,
            isAligner = true,
            isHomed = false,
            isManualEnabled = false,
            isAbsMode = false,
            isBusy = false;
        
        ManualResetEvent _axis_lock;

        #endregion

        #region Constructors

        private void DoConstruct()
        {
            _axis_lock = new ManualResetEvent(true);

            this.IsEnabled = false;
            this.AxisName = "N/A";
        }

        /// <summary>
        /// If you create this object without any parameters, the SetParameters() function MUST BE implemented
        /// </summary>
        public AxisBase()
        {
            DoConstruct();
        }

        public AxisBase(int AxisIndex, ConfigPhysicalAxis Config, IMotionController Controller)
        {
            DoConstruct();
            SetParameters(AxisIndex, Config, Controller);
        }

        #endregion

        #region Properties

        public IMotionController Parent { get; private set; }

        public int AxisIndex { get; private set; }

        public string AxisName { private set; get; }

        public bool IsBusy
        {
            get
            {
                return isBusy;
            }
            set
            {
                UpdateProperty<bool>(ref isBusy, value);
            }
        }

        public bool IsEnabled
        {
            get
            {
                return isEnabled;
            }
            set
            {
                UpdateProperty<bool>(ref isEnabled, value);
            }
        }

        public bool IsAligner
        {
            get
            {
                return isAligner;
            }
            set
            {
                UpdateProperty<bool>(ref isAligner, value);
            }
        }

        public bool IsManualEnabled
        {
            set
            {
                UpdateProperty<bool>(ref isManualEnabled, value);
            }
            get
            {
                return isManualEnabled;
            }
        }

        public bool IsAbsMode
        {
            set
            {
                UpdateProperty<bool>(ref isAbsMode, value);
            }
            get
            {
                return isAbsMode;
            }
        }

        public bool IsHomed
        {
            internal set
            {
                UpdateProperty<bool>(ref isHomed, value);
            }
            get
            {
                return isHomed;
            }
        }

        public int InitPosition { get; private set; }

        public int AbsPosition
        {
            get
            {
                return absPosition;
            }
            set
            {
                // calculate relative postion once the absolute position was changed
                this.RelPosition += (value - absPosition);

                UpdateProperty<int>(ref absPosition, value);

                // convert steps to real-world distance
                this.UnitHelper.AbsPosition = this.UnitHelper.ConvertStepsToPosition(absPosition);
            }
        }

        public int RelPosition
        {
            get
            {
                return relPosition;
            }
            private set
            {
                UpdateProperty<int>(ref relPosition, value);

                // convert steps to real-world distance
                this.UnitHelper.RelPosition = this.UnitHelper.ConvertStepsToPosition(relPosition);
            }
        }

        public object Tag { get; set; }

        /// <summary>
        /// Get the maximum speed
        /// </summary>
        public int MaxSpeed { get; protected set; }

        /// <summary>
        /// Get how many steps used to accelerate
        /// </summary>
        public int AccelerationSteps { private set; get; }
        
        /// <summary>
        /// Get the soft CCW limit in config file (normal zero point)
        /// </summary>
        public int SCCWL
        {
            get
            {
                return sccwl;
            }
            protected set
            {
                UpdateProperty(ref sccwl, value);
            }
        }

        /// <summary>
        /// Get the soft CW limit in config file
        /// </summary>
        public int SCWL
        {
            get
            {
                return scwl;
            }
            protected set
            {
                UpdateProperty(ref scwl, value);
            }
        }

        public RealworldUnitManager UnitHelper { protected set; get; }
        
        public string LastError { set; get; }

        #endregion

        #region Methods

        public virtual void SetParameters(int AxisIndex, ConfigPhysicalAxis Config, IMotionController Controller)
        {
            this.AxisIndex = AxisIndex;

            if (Config == null)
            {
                this.IsEnabled = false;
                this.AxisName = "N/A";
            }
            else
            {
                this.AxisName = Config.Name;
                this.IsEnabled = Config.Enabled;
                this.InitPosition = Config.OffsetAfterHome;
                this.MaxSpeed = Config.MaxSpeed;
                this.AccelerationSteps = Config.Acceleration;
                this.Parent = Controller;

                this.UnitHelper = new RealworldUnitManager(
                    Config.MotorizedStageProfile.TravelDistance,
                    Config.MotorizedStageProfile.Resolution,
                    Config.MotorizedStageProfile.Unit,
                    Config.DecimalPlacesDisplayed);

                this.SCCWL = 0;
                this.SCWL = this.UnitHelper.MaxSteps;
            }
        }

        public bool Lock()
        {
            return _axis_lock.WaitOne(500);
        }
        
        public void Unlock()
        {
            _axis_lock.Set();
        }
        
        public bool CheckSoftLimitation(int TargetPosition)
        {
            if (TargetPosition < this.SCCWL || TargetPosition > this.SCWL)
                return false;
            else
                return true;
        }

        public virtual bool Home()
        {
            return this.Parent.Home(this);
        }
        
        public virtual bool Move(MoveMode Mode, int Speed, double Distance)
        {
            return Parent.Move(this, Mode, Speed, this.UnitHelper.ConvertPositionToSteps(Distance));
        }

        public virtual bool MoveWithTrigger(MoveMode Mode, int Speed, double Distance, double Interval, int Channel)
        {
            return Parent.MoveWithTrigger(
                this, 
                Mode, 
                Speed, 
                this.UnitHelper.ConvertPositionToSteps(Distance),
                this.UnitHelper.ConvertPositionToSteps(Interval), 
                Channel);
        }

        public virtual bool MoveWithInnerADC(MoveMode Mode, int Speed, double Distance, double Interval, int Channel)
        {
            return Parent.MoveWithInnerADC(this,
                Mode,
                Speed,
                this.UnitHelper.ConvertPositionToSteps(Distance),
                this.UnitHelper.ConvertPositionToSteps(Interval),
                Channel);
        }

        public virtual void Stop()
        {
            this.Parent.Stop();
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
                this.UnitHelper.RelPosition = this.UnitHelper.AbsPosition;
                this.RelPosition = this.AbsPosition;
                this.IsAbsMode = true;
            }
        }

        public void ClearRelPosition()
        {
            this.RelPosition = 0;
            this.UnitHelper.RelPosition = 0;
        }

        public override string ToString()
        {
            return string.Format("*{0}@{1}*", this.AxisName, this.Parent.DeviceClass);
        }

        public string GetHashString()
        {
            var factor = string.Join("", new object[]
            {
                AxisName,
                UnitHelper.GetHashString(),
                Parent.DeviceClass
            });

            return HashGenerator.GetHashSHA256(factor);
        }

        #endregion

        #region RaisePropertyChangedEvent
        public event PropertyChangedEventHandler PropertyChanged;

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
