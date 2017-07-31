using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Irixi_Aligner_Common.Classes;
using Irixi_Aligner_Common.Classes.BaseClass;
using Irixi_Aligner_Common.Configuration;
using Irixi_Aligner_Common.Interfaces;
using System;

namespace Irixi_Aligner_Common.MotionControllerEntities.BaseClass
{
    public class LogicalAxis : ViewModelBase
    {
        public delegate void HomeRequestedEventHandle(object sender, EventArgs args);
        public delegate void MoveRequestedEventHandle(object sender, MoveArgs args);
        public delegate void StopRequestedEventHandle(object sender, EventArgs args);

        public event HomeRequestedEventHandle OnHomeRequsted;
        public event MoveRequestedEventHandle OnMoveRequsted;
        public event StopRequestedEventHandle OnStopRequsted;

        public LogicalAxis(SystemService Service, ConfigLogicalAxis Config, string ParentComponentName, int ID)
        {
            this.Service = Service;
            this.Config = Config;
            this.AxisName = Config.DisplayName;
            this.ParentName = ParentComponentName;
            this.ID = ID;
        }

        #region Properties
        /// <summary>
        /// Get the configuration of logical axis
        /// </summary>
        public ConfigLogicalAxis Config { private set; get; }

        /// <summary>
        /// Get the name display on the window
        /// </summary>
        public string AxisName { private set; get; }

        /// <summary>
        /// Get the index of the logical axis defined in the json file
        /// </summary>
        public int ID { private set; get; }

        /// <summary>
        /// Get the name of parent aliger
        /// </summary>
        public string ParentName { private set; get; }

        /// <summary>
        /// Get the instance of system service class
        /// </summary>
        public SystemService Service
        {
            private set;
            get;
        }

        /// <summary>
        /// Get the instance of physical axis
        /// </summary>
        public IAxis PhysicalAxisInst { get; set; }

        #endregion

        #region Commands
        public RelayCommand Home
        {
            get
            {
                return new RelayCommand(() =>
                {
                    OnHomeRequsted?.Invoke(this, new EventArgs());

                });
            }
        }

        public RelayCommand<MoveArgs> Move
        {
            get
            {
                return new RelayCommand<MoveArgs>(arg =>
                {
                    OnMoveRequsted?.Invoke(this, arg);
                });
            }
        }

        public RelayCommand Stop
        {
            get
            {
                return new RelayCommand(() =>
                {
                    OnStopRequsted?.Invoke(this, new EventArgs());
                });
            }
        }
        #endregion



        #region Overrided Methods
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("*{0}@{1}*", Config.DisplayName, ParentName);
        }
        #endregion
    }
}
