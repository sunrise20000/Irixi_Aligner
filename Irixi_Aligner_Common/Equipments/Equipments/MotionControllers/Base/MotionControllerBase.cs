using System;
using System.Collections.Generic;
using Irixi_Aligner_Common.Classes.BaseClass;
using Irixi_Aligner_Common.Configuration.MotionController;
using Irixi_Aligner_Common.Interfaces;

namespace Irixi_Aligner_Common.MotionControllers.Base
{
    public class MotionControllerBase<T> : Dictionary<string, T>, IMotionController, IDisposable
        where T:IAxis, new()
    {
        #region Variables

        public event EventHandler OnMoveBegin;

        public event EventHandler OnMoveEnd;

        private string lastError = "";
        protected ConfigPhysicalMotionController _config;

        private readonly object lockBusyAxesCount = new object();

        #endregion Variables

        #region Constructor

        public MotionControllerBase(ConfigPhysicalMotionController Config)
        {
            DeviceClass = Config.DeviceClass;
            Model = Config.Model;
            Port = Config.Port;
            IsEnabled = Config.Enabled;
            IsInitialized = false;
            BusyAxesCount = 0;


            //
            // Generate the items of AxisCollection according the config of the physical motion controller
            //
            int i = 0;
            foreach (var axisConfig in Config.AxisCollection)
            {
                T axis = new T();
                axis.SetParameters(i, axisConfig, this);

                //TODO What if the axis name had been existed?
                this.Add(axisConfig.Name, axis);

                i++;
            }
        }

        #endregion Constructor

        #region Properties

        public Guid DeviceClass { private set; get; }

        public MotionControllerType Model { private set; get; }

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

        //public Dictionary<string, IAxis>AxisCollection { private set; get; }

        /// <summary>
        /// The property indicates that how many axes are moving.
        /// If it is 0, fire the event #OnMoveBegin before moving, and fire the event #OnMoveEnd after moving,
        /// This feature is especially used to tell #SystemService whether I(Motion Controller) am busy or not,
        /// I'll be added to #BusyComponent list once the first axis was moving and be removed once the last axis was stopped.
        /// In order to execute #Stop command ASAP, #SystemService only stops the components which are in the #BusyComponent list.
        /// </summary>
        public int BusyAxesCount { private set; get; }


        public string HashString
        {
            get
            {
                return HashGenerator.GetHashSHA256(DeviceClass.ToString());
            }
        }

        #endregion Properties

        #region Methods

        public bool Init()
        {
            if (this.IsEnabled)
                return InitProcess();
            else
            {
                this.LastError = "the controller is disabled";
                return false;
            }
        }

        public IAxis GetAxisByName(string AxisName)
        {
            return this[AxisName];
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
                ret = HomeProcess(Axis);
                DecreaceBusyAxesCount();

                if (BusyAxesCount <= 0)
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
                Axis.LastError = "the controller is not initialized";
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
                if (BusyAxesCount <= 0)
                    OnMoveBegin?.Invoke(this, new EventArgs());

                IncreaceBusyAxesCount();

                ret = MoveProcess(Axis, Mode, Speed, Distance);

                DecreaceBusyAxesCount();

                if (BusyAxesCount <= 0)
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

        /// <summary>
        /// Customized process of initialization
        /// </summary>
        /// <returns></returns>
        protected virtual bool InitProcess()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Customized process to home
        /// </summary>
        /// <param name="Axis">The axis to home</param>
        /// <returns></returns>
        protected virtual bool HomeProcess(IAxis Axis)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Customized process to move
        /// </summary>
        /// <param name="Axis">The axis to move</param>
        /// <param name="Mode">Rel/Abs</param>
        /// <param name="Speed">0 ~ 100 in percent</param>
        /// <param name="Distance">The distance to move in steps</param>
        /// <returns></returns>
        protected virtual bool MoveProcess(IAxis Axis, MoveMode Mode, int Speed, int Distance)
        {
            throw new NotImplementedException();
        }

        public sealed override string ToString()
        {
            return string.Format("*{0}@{1}*", this.Model.ToString(), this.Port);
        }

        public virtual void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion Methods

        #region Private Methods

        private void IncreaceBusyAxesCount()
        {
            lock (lockBusyAxesCount)
            {
                BusyAxesCount++;
            }
        }

        private void DecreaceBusyAxesCount()
        {
            lock (lockBusyAxesCount)
            {
                BusyAxesCount--;
            }
        }

        #endregion Private Methods
    }
}