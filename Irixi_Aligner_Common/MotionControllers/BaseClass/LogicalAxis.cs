using System;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Irixi_Aligner_Common.Classes.BaseClass;
using Irixi_Aligner_Common.Configuration.MotionController;
using Irixi_Aligner_Common.Interfaces;

namespace Irixi_Aligner_Common.MotionControllers.Base
{
    public class LogicalAxis : ViewModelBase, IHashable
    {
        #region Variables

        public delegate void HomeRequestedEventHandle(object sender, EventArgs args);
        public delegate void MoveRequestedEventHandle(object sender, AxisMoveArgs args);
        public delegate void StopRequestedEventHandle(object sender, EventArgs args);

        public event HomeRequestedEventHandle OnHomeRequsted;
        public event MoveRequestedEventHandle OnMoveRequsted;
        public event StopRequestedEventHandle OnStopRequsted;

        IAxis physicalAxis;

        #endregion

        #region Constructors

        public LogicalAxis(ConfigLogicalAxis Config, string ParentComponentName)
        {
            this.Config = Config;
            this.AxisName = Config.DisplayName;
            this.ParentName = ParentComponentName;
            this.MoveArgs = new AxisMoveArgs();
        } 

        #endregion

        #region Properties
        /// <summary>
        /// Get the configuration of logical axis
        /// </summary>
        public ConfigLogicalAxis Config { private set; get; }

        /// <summary>
        /// Get the name display on the window
        /// <see cref="Irixi_Aligner_Common.Configuration.MotionController.ConfigLogicalAxis.DisplayName"/>
        /// </summary>
        public string AxisName { private set; get; }

        /// <summary>
        /// Get the name of parent aliger
        /// </summary>
        public string ParentName { private set; get; }

        /// <summary>
        /// Get the instance of physical axis
        /// </summary>
        public IAxis PhysicalAxisInst
        { 
            get
            {
                return physicalAxis;
            }
            set
            {
                physicalAxis = value;
                MoveArgs.LogicalAxisHashString = GetHashString();
                MoveArgs.Unit = value.UnitHelper.ToString();
            }
        }

        /// <summary>
        /// Get or set the arguments to move the physical axis, which is also bound to the window
        /// </summary>
        public AxisMoveArgs MoveArgs { get; set; }

        #endregion

        #region Methods

        public void ToggleMoveMode()
        {
            PhysicalAxisInst.ToggleMoveMode();
            if (PhysicalAxisInst.IsAbsMode)
                MoveArgs.Mode = MoveMode.ABS;
            else
                MoveArgs.Mode = MoveMode.REL;
        }

        public string GetHashString()
        {
            return PhysicalAxisInst.GetHashString();
        }

        public override string ToString()
        {
            return string.Format("*{0}@{1}*", Config.DisplayName, ParentName);
        }

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

        public RelayCommand<AxisMoveArgs> Move
        {
            get
            {
                return new RelayCommand<AxisMoveArgs>(arg =>
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
    }
}
