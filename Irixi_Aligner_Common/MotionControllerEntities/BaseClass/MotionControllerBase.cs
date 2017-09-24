using Irixi_Aligner_Common.Configuration;
using Irixi_Aligner_Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Irixi_Aligner_Common.MotionControllerEntities
{
    public class MotionControllerBase<T> : IMotionController, IDisposable
        where T : IAxis, new()
    {

        #region Variables
        public event EventHandler<string> OnErrorOccurred;
        public event EventHandler<object> OnHomeCompleted;

        string _lasterr = "";
        protected ConfigPhysicalMotionController _config;

        /// <summary>
        /// lock while operating the property of RunningAxesSum
        /// </summary>
        object _lock_runingaxessum = new object();

        #endregion

        #region Constructor

        public MotionControllerBase(ConfigPhysicalMotionController Config)
        {
            _config = Config;
            this.DeviceClass = _config.DeviceClass;
            this.Model = _config.Model;
            this.Port = _config.Port;
            this.IsEnabled = Config.Enabled;
            this.IsInitialized = false;
            this.AxisCollection = new Dictionary<string, IAxis>();
            this.RunningAxesSum = 0;

            //
            // Generate the items of AxisCollection according the config of the physical motion controller
            //
            int i = 0;
            foreach (var _axis_cfg in Config.AxisCollection)
            {
                T _axis = new T();
                _axis.SetParameters(i, _axis_cfg, this);

                this.AxisCollection.Add(_axis_cfg.Name, _axis);
                i++;
            }
        } 
        #endregion

        #region Properties
        public Guid DeviceClass { private set; get; }

        public MotionControllerModel Model { private set; get; }

        public string Port { private set; get; }

        public bool IsEnabled { private set; get; }

        public bool IsInitialized { protected set; get; }

        public string LastError
        {
            get
            {
                return _lasterr;
            }
            internal set
            {
                _lasterr = value;
            }
        }

        public Dictionary<string, IAxis>AxisCollection { private set; get; }

        public int RunningAxesSum { private set; get; }

        #endregion

        #region Methods

        public virtual Task<bool> Init()
        {
            return new Task<bool>(() =>
            {
                if (this.IsEnabled) // the controller is configured to be disabled in the config file 
                    return true;
                else
                {
                    this.LastError = "the controller is disabled";
                    return false;
                }
            });
        }

        public virtual Task<bool> Home(IAxis Axis)
        {
            return new Task<bool>(() =>
            {
                bool ret = false;

                if (!this.IsEnabled) // the controller is configured to be disabled in the config file
                {
                    Axis.LastError = "the controller is disabled";
                }
                else if (!this.IsInitialized)   // the controller is not initialized
                    {
                    Axis.LastError =  "the controller is not initialized";
                }
                else if (!Axis.IsEnabled)   // the axis moved is disabled in the config file
                {
                    Axis.LastError = "the axis is disabled";
                }
                else
                    ret = true;

                return ret;
            });
        }

        public virtual Task<bool> HomeAll()
        {
            throw new NotImplementedException();
        }

        public virtual Task<bool> Move(IAxis Axis, MoveMode Mode, int Speed, int Distance)
        {
            return new Task<bool>(() =>
            {
                bool ret = false;

                if (!this.IsEnabled)
                {
                    Axis.LastError = "the controller is disabled";
                }
                if(!this.IsInitialized)
                {
                    Axis.LastError = "the controller has not been initialized";
                }
                else if (!Axis.IsEnabled)
                {
                    Axis.LastError = "the axis is disabled";
                }
                else if(!Axis.IsHomed)
                {
                    Axis.LastError = "the axis is not homed";
                }
                else
                    ret = true;

                return ret;
            });
        }

        public virtual Task<bool> MoveWithTrigger(IAxis Axis, MoveMode Mode, int Speed, int Distance, int Interval, int Channel)
        {
            throw new NotImplementedException();
        }

        public virtual Task<bool> MoveWithInnerADC(IAxis Axis, MoveMode Mode, int Speed, int Distance, int Interval, int Channel)
        {
            throw new NotImplementedException();
        }

        public virtual void Stop()
        {
            throw new NotImplementedException();
        }

        public void IncreaseRunningAxes()
        {
            lock(_lock_runingaxessum)
            {
                this.RunningAxesSum++;
            }
        }

        public void DecreaseRunningAxes()
        {
            lock (_lock_runingaxessum)
            {
                this.RunningAxesSum--;
            }
        }

        public override string ToString()
        {
            return string.Format("*{0}@{1}*", this.Model.ToString(), this.Port);
        }

        public override int GetHashCode()
        {
            return this.DeviceClass.GetHashCode();
        }

        #endregion

        #region Internal Methods
        /// <summary>
        /// Find the axis by name property
        /// </summary>
        /// <param name="Name">Axis Name</param>
        /// <returns></returns>
        public IAxis FindAxisByName(string Name)
        {
            //// Get the axis with the unit property equels the specified value
            //var axis = from c in this.AxisCollection where (c.AxisName.ToLower() == Name.ToLower()) select c;

            //if (axis.Any()) // If the axis which was specified by name was found
            //    return axis.ToList()[0];
            //else
            //    return null;
            try
            {
                return (T)this.AxisCollection[Name];
            }
            catch
            {
                return default(T);
            }
        }

        /// <summary>
        /// Pass error to system service class
        /// </summary>
        public void RaiseOnErrorOccurredEvent()
        {
            OnErrorOccurred?.Invoke(this, this.LastError);
        }

        /// <summary>
        /// Report home completion to system service class
        /// </summary>
        public void RasiseOnHomeCompletedEvent()
        {
            OnHomeCompleted?.Invoke(this, new EventArgs());
        }

        public virtual void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
