using Irixi_Aligner_Common.Interfaces;
using Irixi_Aligner_Common.MotionControllerEntities;
using IrixiStepperControllerHelper;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System;
using System.Threading.Tasks;

namespace Irixi_Aligner_Common.Equipments
{
    public class CylinderController : IBaseEquipment, INotifyPropertyChanged
    {
        #region Variables

        #endregion
        
        #region Constructors
        public CylinderController(IrixiEE0017 Controller, int Pedal, int Fiber, int Lens, int PLC, int POD)
        {
            this.IsEnabled = true;
            this.IsInitialized = false;
            this.LastError = "";

            this.Controller = Controller;
            this.PedalInputPort = Pedal;
            this.FiberClampOutputPort = Fiber;
            this.LensVacuumOutputPort = Lens;
            this.PLCVacuumOutputPort = PLC;
            this.PODVacuumOutputPort = POD;

            // detect the input gpio of the pedal state
            this.Controller.OnInputStateChanged += ((s, e) =>
            {
                if (e.Channel == Pedal && e.State == InputState.Triggered)
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

        bool _enabled;
        public bool IsEnabled
        {
            set
            {
                UpdateProperty<bool>(ref _enabled, value);
            }
            get
            {
                return _enabled;
            }
        }

        bool _is_init;
        public bool IsInitialized
        {
            set
            {
                UpdateProperty<bool>(ref _is_init, value);
            }
            get
            {
                return _is_init;
            }
        }

        public string LastError
        {
            private set;
            get;
        }

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
        public Task<bool> Init()
        {
            return new Task<bool>(() =>
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
            });
        }

        public override string ToString()
        {
            return string.Format("*{0}@{1}*", "Cylinder Controller", this.Controller.DevClass);
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
            //if (object.Equals(OldValue, NewValue))  // To save resource, if the value is not changed, do not raise the notify event
            //    return;

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


        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~CylinderController() {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
