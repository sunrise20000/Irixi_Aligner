using Irixi_Aligner_Common.Configuration;
using Irixi_Aligner_Common.Interfaces;
using Irixi_Aligner_Common.MotionControllerEntities;
using IrixiStepperControllerHelper;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Irixi_Aligner_Common.Equipments
{
    public class CylinderController : EquipmentBase
    {
        #region Variables

        #endregion
        
        #region Constructors
        public CylinderController(ConfigurationCylinder Config, IrixiEE0017 ControllerAttached) : base(Config)
        {
            this.Controller = ControllerAttached;
            this.PedalInputPort = Config.PedalInput;
            this.FiberClampOutputPort = Config.FiberClampOutput;
            this.LensVacuumOutputPort = Config.LensVacuumOutput;
            this.PLCVacuumOutputPort = Config.PLCVacuumOutput;
            this.PODVacuumOutputPort = Config.PODVacuumOutput;

            // detect the input gpio of the pedal state
            this.Controller.OnInputStateChanged += ((s, e) =>
            {
                if (e.Channel == this.PedalInputPort && e.State == InputState.Triggered)
                    DoPedalTriggerd();
            });

            // the hid report is received, flush the cylinders' state (Output)
            this.Controller.OnHIDReportReceived += ((s, e) =>
            {
                this.FiberClampState = e.GetOutputState(this.FiberClampOutputPort);
                this.LensVacuumState = e.GetOutputState(this.LensVacuumOutputPort);
                this.PlcVacuumState = e.GetOutputState(this.PLCVacuumOutputPort);
                this.PodVacuumState = e.GetOutputState(this.PODVacuumOutputPort);
            });
            
        }
        #endregion

        #region Properties
        public IrixiEE0017 Controller
        {
            private set;
            get;
        }

        public int PedalInputPort
        {
            private set;
            get;
        }

        public int FiberClampOutputPort
        {
            private set;
            get;
        }

        public int LensVacuumOutputPort
        {
            private set;
            get;
        }

        public int PLCVacuumOutputPort
        {
            private set;
            get;
        }

        public int PODVacuumOutputPort
        {
            private set;
            get;
        }

        OutputState _fiber_clamp_state;
        /// <summary>
        /// Get the state of fiber clamp
        /// </summary>
        /// <value>The FiberClampState property gets/sets the value of the OutputState field, _fiber_clamp_state.</value>
        public OutputState FiberClampState
        {
            private set
            {
                UpdateProperty<OutputState>(ref _fiber_clamp_state, value);
            }
            get
            {
                return _fiber_clamp_state;
            }
        }

        OutputState _lens_vacuum;
        /// <summary>
        /// Get the state of Lens Vacuum
        /// </summary>
        public OutputState LensVacuumState
        {
            private set
            {
                UpdateProperty<OutputState>(ref _lens_vacuum, value);
            }
            get
            {
                return _lens_vacuum;
            }
        }


        OutputState _plc_vacuum;
        /// <summary>
        /// Get the state of PLC Vacuum
        /// </summary>
        public OutputState PlcVacuumState
        {
            private set
            {
                UpdateProperty<OutputState>(ref _plc_vacuum, value);
            }
            get
            {
                return _plc_vacuum;
            }
        }

        OutputState _pod_vacuum;
        /// <summary>
        /// Get the state of POD Vacuum
        /// </summary>
        public OutputState PodVacuumState
        {
            private set
            {
                UpdateProperty<OutputState>(ref _pod_vacuum, value);
            }
            get
            {
                return _pod_vacuum;
            }
        }

        #endregion

        #region Methods
        private void DoPedalTriggerd()
        {
            this.Controller.ToggleOutput(this.PedalInputPort);
        }

        public void SetFiberClampState(OutputState State)
        {
            this.Controller.SetOutput(this.FiberClampOutputPort, State);
        }

        public void SetLensVacuumState(OutputState State)
        {
            this.Controller.SetOutput(this.LensVacuumOutputPort, State);
        }

        public void SetPodVacuumState(OutputState State)
        {
            this.Controller.SetOutput(this.PODVacuumOutputPort, State);
        }

        public void SetPlcVacuumState(OutputState State)
        {
            this.Controller.SetOutput(this.PLCVacuumOutputPort, State);
        }
        #endregion

        #region Methods
        public override bool Init()
        {
            if (this.IsEnabled) // the controller is configured to be disabled in the config file 
            {
                if (this.Controller.IsInitialized)
                {
                    this.IsInitialized = true;
                    return true;
                }
                else
                {
                    this.LastError = "the corresponding motion controller is not available";
                    return false;
                }
            }
            else
            {
                this.LastError = "it is configured to be disabled";
                return false;
            }
        }
        #endregion
    }
}
