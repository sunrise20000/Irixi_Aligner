using Irixi_Aligner_Common.Configuration.MotionController;
using Irixi_Aligner_Common.Interfaces;
using System;
using System.Collections.Generic;

namespace Irixi_Aligner_Common.MotionControllers.Base
{
    public class MotionControllerBase<T> : IMotionController, IDisposable
        where T : IAxis, new()
    {

        #region Variables

        public event EventHandler OnMoveBegin;
        public event EventHandler OnMoveEnd;
        
        string lastError = "";
        protected ConfigPhysicalMotionController _config;
        
        readonly object lockBusyAxesCount = new object();
        
        #endregion

        #region Constructor

        public MotionControllerBase(ConfigPhysicalMotionController Config)
        {
            _config = Config;
            DeviceClass = _config.DeviceClass;
            Model = _config.Model;
            Port = _config.Port;
            IsEnabled = Config.Enabled;
            IsInitialized = false;
            AxisCollection = new Dictionary<string, IAxis>();
            BusyAxesCount = 0;

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
                return lastError;
            }
            internal set
            {
                lastError = value;
            }
        }

        public Dictionary<string, IAxis>AxisCollection { private set; get; }

        /// <summary>
        /// The property indicates that how many axes are moving. 
        /// If it is 0, raise the event #OnMoveBegin before moving, 
        /// If it is 0, raise the event #OnMoveEnd after moving, 
        /// This feature is especially used to tell #SystemService whether I(Motion Controller) am busy or not, 
        /// I'll be added to #BusyComponent list once the first axis was moving and be removed once the last axis was stopped. 
        /// In order to execute #Stop command ASAP, #SystemService only stops the components which are in the #BusyComponent list.
        /// </summary>
        public int BusyAxesCount { private set; get; }
        
        #endregion

        #region Methods

        public bool Init()
        {
            if (this.IsEnabled)
                return CustomInitProcess();
            else
            {
                this.LastError = "the controller is disabled";
                return false;
            }
        }

        public bool Home(IAxis Axis)
        {
            bool ret = false;

            if (!this.IsEnabled) // the controller is configured to be disabled in the config file
            {
                Axis.LastError = "the controller is disabled";
            }
            else if (!this.IsInitialized)   // the controller is not initialized
            {
                Axis.LastError = "the controller is not initialized";
            }
            else if (!Axis.IsEnabled)   // the axis moved is disabled in the config file
            {
                Axis.LastError = "the axis is disabled";
            }
            else
            {
                if (BusyAxesCount <= 0)
                    OnMoveBegin?.Invoke(this, new EventArgs());

                IncreaceBusyAxesCount();
                ret = CustomHomeProcess(Axis);
                DecreaceBusyAxesCount();

                if(BusyAxesCount <= 0)
                    OnMoveEnd?.Invoke(this, new EventArgs());
            }



            return ret;
        }

        public bool HomeAll()
        {
            throw new NotImplementedException();
        }

        public bool Move(IAxis Axis, MoveMode Mode, int Speed, int Distance)
        {
            bool ret = false;

            if (!this.IsEnabled)
            {
                Axis.LastError = "the controller is disabled";
            }
            if (!this.IsInitialized)
            {
                Axis.LastError = "the controller has not been initialized";
            }
            else if (!Axis.IsEnabled)
            {
                Axis.LastError = "the axis is disabled";
            }
            else if (!Axis.IsHomed)
            {
                Axis.LastError = "the axis is not homed";
            }
            else
            {

                if(BusyAxesCount <= 0)
                    OnMoveBegin?.Invoke(this, new EventArgs());

                IncreaceBusyAxesCount();

                ret = CustomMoveProcess(Axis, Mode, Speed, Distance);

                DecreaceBusyAxesCount();

                if(BusyAxesCount <= 0)
                    OnMoveEnd?.Invoke(this, new EventArgs());
            }

            return ret;
        }

        public bool MoveWithTrigger(IAxis Axis, MoveMode Mode, int Speed, int Distance, int Interval, int Channel)
        {
            throw new NotImplementedException();
        }

        public bool MoveWithInnerADC(IAxis Axis, MoveMode Mode, int Speed, int Distance, int Interval, int Channel)
        {
            throw new NotImplementedException();
        }

        public virtual void Stop()
        {
            throw new NotImplementedException();
        }
        
        public override int GetHashCode()
        {
            return this.DeviceClass.GetHashCode();
        }

        protected virtual bool CustomInitProcess()
        {
            throw new NotImplementedException();
        }

        protected virtual bool CustomHomeProcess(IAxis Axis)
        {
            throw new NotImplementedException();
        }

        protected virtual bool CustomMoveProcess(IAxis Axis, MoveMode Mode, int Speed, int Distance)
        {
            throw new NotImplementedException();
        }

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

        sealed public override string ToString()
        {
            return string.Format("*{0}@{1}*", this.Model.ToString(), this.Port);
        }
        
        public virtual void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Private Methods
        void IncreaceBusyAxesCount()
        {
            lock(lockBusyAxesCount)
            {
                BusyAxesCount++;
            }
        }

        void DecreaceBusyAxesCount()
        {
            lock (lockBusyAxesCount)
            {
                BusyAxesCount--;
            }
        }
        #endregion
    }
}
