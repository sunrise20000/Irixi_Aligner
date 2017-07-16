using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using USBHIDDRIVER;

namespace IrixiStepperControllerHelper
{
    public class IrixiMotionController : INotifyPropertyChanged, IDisposable
    {
        #region Variables

        private static object _lock = new object();

        /// <summary>
        /// Implement INotifyPropertyChanged
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The event rasise while the hid report updated
        /// </summary>
        public event EventHandler<DeviceStateReport> OnReportUpdated;

        /// <summary>
        /// The event raises while the connection status changed
        /// </summary>
        public event EventHandler<ConnectionEventArgs> OnConnectionStatusChanged;

        /// <summary>
        /// The event raises while the status of the input IO changed
        /// </summary>
        public event EventHandler<InputEventArgs> OnInputIOStatusChanged;

        const string VID = "vid_0483";
        const string PID = "pid_574e";

        /// <summary>
        /// Report ID 1: report contains device status
        /// </summary>
        const int REPORT_ID_DEVICESTATE = 0x1;

        /// <summary>
        /// Report ID 2: report contains firmware information
        /// </summary>
        const int REPORT_ID_FWINFO = 0x2;

        /// <summary>
        /// The total steps which is used to acceleration and deceleration
        /// </summary>
        const int ACC_DEC_STEPS = 1000;

        /// <summary>
        /// The maximum drive veloctiy
        /// The real velocity is Velocity_Set * MAX_VELOCITY
        /// </summary>
        const int MAX_VELOCITY = 10000;

        USBInterface _hid_device;

        bool _is_connected = false; // whether the contoller is connected
        string _last_err = string.Empty, _serial_number = "";

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="DeviceSN">The serial number of the controller to be connected</param>
        /// <param name="MaxDistance"></param>
        /// <param name="PosAfterHome"></param>
        /// <param name="SCCWLS">Soft CCW limitation sensor</param>
        /// <param name="SCWLS">Soft CW limitation sensor</param>
        public IrixiMotionController(string DeviceSN = "")
        {
            // Generate the instance of the state report object
            this.Report = new DeviceStateReport();
            this.FirmwareInfo = new FimwareInfo();
            this.TotalAxes = -1;
            this.SerialNumber = DeviceSN;
            this.AxisCollection = new ObservableCollection<Axis>();
            BindingOperations.EnableCollectionSynchronization(this.AxisCollection, _lock);

            _hid_device = new USBInterface(VID, PID, DeviceSN);
            _hid_device.EnableUsbBufferEvent(OnReportReceived);
            _hid_device.EnableUsbDisconnectEvent(OnDisconnected);
        }

        #endregion

        #region Properties
        /// <summary>
        /// Get the last error information
        /// </summary>
        public string LastError
        {
            private set
            {
                UpdateProperty<string>(ref _last_err, value);
            }
            get
            {
                return _last_err;
            }
        }

        /// <summary>
        /// Get the number of axes that the HID Controller supports
        /// </summary>
        public int TotalAxes
        {
            private set;
            get;
        }

        /// <summary>
        /// Get the serial number of HID stepper controller
        /// </summary>
        public string SerialNumber
        {
            private set
            {
                UpdateProperty<string>(ref _serial_number, value);
            }
            get
            {
                return _serial_number;
            }
        }

        /// <summary>
        /// Get whether the HID Controller is connected
        /// </summary>
        public bool IsConnected
        {
            private set
            {
                UpdateProperty<bool>(ref _is_connected, value);
            }
            get
            {
                return _is_connected;
            }
        }

        /// <summary>
        /// Get the state report from the HID Controller
        /// </summary>
        public DeviceStateReport Report
        {
            private set;
            get;
        }

        /// <summary>
        /// Get the infomation of the firmware which consists of verion and compiled date
        /// </summary>
        public FimwareInfo FirmwareInfo
        {
            private set;
            get;
        }

        /// <summary>
        /// Get the axis collection instance of the device
        /// </summary>
        public ObservableCollection<Axis> AxisCollection
        {
            private set;
            get;
        }

        #endregion

        #region Methods
        /// <summary>
        /// Read the controllers' serial number and output as a string list
        /// </summary>
        /// <returns></returns>
        public static string[] GetDeviceList()
        {
            USBInterface hid = new USBInterface(PID, VID);
            return hid.GetDeviceList();
        }

        /// <summary>
        /// Open the controller
        /// </summary>
        public bool OpenDevice()
        {
            int _reconn_counter = 0;
            this.IsConnected = false;

            while (true)
            {
                try
                {
                    _reconn_counter++;

                    if (_hid_device.Connect())
                    {
                        // Start the received task on the UI thread
                        OnConnectionStatusChanged?.Invoke(this, new ConnectionEventArgs(ConnectionEventArgs.EventType.ConnectionSuccess, null));

                        // start to read hid report from usb device in the background thread
                        ////_hid_device.StartRead();

                        // to realize the mechanism of timeout, save the time when the initialization process is started
                        DateTime _init_start_time = DateTime.Now;
                        
                        // Wait the first report from HID device in order to get the 'TotalAxes'
                        do
                        {
                            // read hid report
                            byte[] report = _hid_device.Read();
                            if (report != null)
                            {
                                this.Report.ParseRawData(report);

                                this.TotalAxes = this.Report.TotalAxes;
                            }

                            // Don't check it so fast, the interval of two adjacent report is normally 20ms but not certain
                            Thread.Sleep(100);

                            // check whether the initialization process is timeout
                            if((DateTime.Now - _init_start_time).TotalSeconds > 5)
                            {
                                this.LastError = "unable to get the total axes";
                                break;
                            }


                        } while (this.TotalAxes <= 0);

                        // TotalAxes <= 0 indicates that no axis was found within 5 seconds, exit initialization process
                        if (this.TotalAxes <= 0)
                        {
                            break;
                        }

                        // the total number of axes returned, generate the instance of each axis
                        this.Report.AxisStateCollection.Clear();

                        // create the axis collection according the TotalAxes property in the hid report
                        for (int i = 0; i < this.TotalAxes; i++)
                        {
                            // generate axis state object to the controller report class
                            this.Report.AxisStateCollection.Add(new AxisState()
                            {
                                AxisIndex = i
                            });

                            // generate axis control on the user window
                            this.AxisCollection.Add(new Axis()
                            {
                                // set the properties to the default value
                                MaxDistance = 15000,
                                SoftCCWLS = 0,
                                SoftCWLS = 15000,
                                PosAfterHome = 0
                            });
                        }

                        // start to read the hid report repeatedly
                        _hid_device.StartRead();

                        
                        this.IsConnected = true;

                        // initialize this.Report property on UI thread
                        OnConnectionStatusChanged?.Invoke(this, new ConnectionEventArgs(ConnectionEventArgs.EventType.TotalAxesReturned, this.TotalAxes));
                        break;

                    }
                    else
                    {
                        // pass the try-times to UI thread
                        OnConnectionStatusChanged?.Invoke(this, new ConnectionEventArgs(ConnectionEventArgs.EventType.ConnectionRetried, _reconn_counter));
                        Thread.Sleep(500);

                        // check if reaches the max re-connect times
                        if(_reconn_counter > 10)
                        {
                            this.LastError = "the initialization process was timeout";
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.LastError = ex.Message;
                    break;
                }
            }

            return IsConnected;
        }
        
        /// <summary>
        /// Open the controller asynchronously
        /// </summary>
        /// <returns></returns>
        public Task<bool> OpenDeviceAsync()
        {
            return Task.Run<bool>(() =>
            {
                return OpenDevice();
               
            });
        }
        
        /// <summary>
        /// Close the controller
        /// </summary>
        public void CloseDevice()
        {
            _hid_device.Disconnect();
        }

        /// <summary>
        /// Read firmware information
        /// after the information is returned, get the detail from FirmwareInfo property
        /// </summary>
        /// <returns></returns>
        public bool ReadFWInfo()
        {
            this.FirmwareInfo.SetToDefault();

            CommandStruct cmd = new CommandStruct()
            {
                Command = EnumCommand.FWINFO
            };

            _hid_device.Write(cmd.ToBytes());

            bool _timeout = false;
            DateTime _start = DateTime.Now;
            do
            {
                if((DateTime.Now - _start).TotalSeconds > 3)
                {
                    _timeout = true;
                    break;
                }

                Thread.Sleep(100);

            } while (FirmwareInfo.Validate() == false);

            if (_timeout)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Read firmware information asynchronously
        /// </summary>
        /// <returns></returns>
        public Task<bool> ReadFWInfoAsync()
        {
            return Task.Run<bool>(()=>
            {
                return ReadFWInfo();
            });
        }

        /// <summary>
        /// Home the specified axis synchronously
        /// </summary>
        /// <param name="AxisIndex">The axis index, this parameter should be 0 ~ 2</param>
        /// <returns></returns>
        public bool Home(int AxisIndex)
        {
            if(AxisIndex >= this.Report.TotalAxes)
            {
                this.LastError = string.Format("The param of axis index if error.");
                return false;
            }
            // if the controller is not connected, return
            else if (!this.IsConnected)
            {
                this.LastError = string.Format("The controller is not connected.");
                return false;
            }

            // If the axis is busy, return.
            if (this.Report.AxisStateCollection[AxisIndex].IsRunning)
            {
                this.LastError = string.Format("Axis {0} is busy.", AxisIndex);
                return false;
            }

            // start to home process
            try
            {
                // write the 'home' command to controller
                CommandStruct cmd = new CommandStruct()
                {
                    Command = EnumCommand.HOME,
                    AxisIndex = AxisIndex
                };
                _hid_device.Write(cmd.ToBytes());

                //! Wait for 2 report packages to ensure that the move command has been 
                //! executed by the device.
                uint _report_counter = this.Report.Counter + 2;

                do
                {
                    Thread.Sleep(10);
                } while (this.Report.Counter <= _report_counter);

                // the TRUE value of the IsRunning property indicates that the axis is running
                // wait until the running process is done
                bool _timeout = false;
                DateTime _start = DateTime.Now;
                while (this.Report.AxisStateCollection[AxisIndex].IsRunning == true)
                {
                    Thread.Sleep(100);
                    if ((DateTime.Now - _start).TotalSeconds > 60)
                    {
                        _timeout = true;
                        break;
                    }
                }

                if (_timeout)
                {
                    this.LastError = "timeout occured while checking the IsHoming flag.";
                    return false;
                }
                else
                {
                    Thread.Sleep(50);

                    if (this.Report.AxisStateCollection[AxisIndex].IsHomed)
                    {
                        return true;
                    }
                    else
                    {
                        this.LastError = string.Format("error code {0:d}", Report.AxisStateCollection[AxisIndex].Error);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                this.LastError = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Home the speicified axis asynchronously
        /// </summary>
        /// <param name="Axis"></param>
        /// <returns></returns>
        public Task<bool> HomeAsync(int AxisIndex)
        {
            return Task.Run<bool>(() =>
            {
                return Home(AxisIndex);
            });
        }

        /// <summary>
        /// Move the specified axis synchronously
        /// </summary>
        /// <param name="AxisIndex"></param>
        /// <param name="Velocity"></param>
        /// <param name="Distance"></param>
        /// <param name="Mode"></param>
        /// <returns></returns>
        public bool Move(int AxisIndex, int Velocity, int Distance, MoveMode Mode)
        {
            int _curr_pos = this.Report.AxisStateCollection[AxisIndex].AbsPosition;   // Get current ABS position
            int _pos_aftermove = 0;

            if(AxisIndex >= this.TotalAxes)
            {
                this.LastError = string.Format("The param of axis index if error.");
                return false;
            }
            // if the controller is not connected, return
            else if (!this.IsConnected)
            {
                this.LastError = string.Format("The controller is not connected.");
                return false;
            }

            // If the axis is not homed, return.
            if (this.Report.AxisStateCollection[AxisIndex].IsHomed == false)
            {
                this.LastError = string.Format("Axis {0} is not homed.", AxisIndex);
                return false;
            }

            // If the axis is busy, return.
            if (this.Report.AxisStateCollection[AxisIndex].IsRunning)
            {
                this.LastError = string.Format("Axis {0} is busy.", AxisIndex);
                return false;
            }

            if (Velocity < 1 || Velocity > 100)
            {
                this.LastError = string.Format("The velocity should be 1 ~ 100.");
                return false;
            }

            //
            // Validate the parameters restricted in the config file
            //
            // MaxDistance > 0
            if (this.AxisCollection[AxisIndex].MaxDistance <= 0)
            {
                this.LastError = string.Format("The value of the Max Distance has not been set.");
                return false;
            }

            // SoftCWLS > SoftCCWLS
            if (this.AxisCollection[AxisIndex].SoftCWLS <= this.AxisCollection[AxisIndex].SoftCCWLS)
            {
                this.LastError = string.Format("The value of the SoftCWLS should be greater than the value of the SoftCCWLS.");
                return false;
            }

            // SoftCWLS >= MaxDistance
            if (this.AxisCollection[AxisIndex].SoftCWLS < this.AxisCollection[AxisIndex].MaxDistance)
            {
                this.LastError = string.Format("The value of the SoftCWLS should be greater than the value of the Max Distance.");
                return false;
            }

            // SoftCCWLS <= PosAfterHome <= SoftCWLS
            if ((this.AxisCollection[AxisIndex].SoftCCWLS > this.AxisCollection[AxisIndex].PosAfterHome) ||
            (this.AxisCollection[AxisIndex].PosAfterHome > this.AxisCollection[AxisIndex].SoftCWLS))
            {
                this.LastError = string.Format("The value of the PosAfterHome exceeds the soft limitaion.");
                return false;
            }

            //
            // Validate the position after moving,
            // if the position exceeds the soft limitation, do not move
            //
            if (Mode == MoveMode.ABS)
            {
                if (Distance < this.AxisCollection[AxisIndex].SoftCCWLS || Distance > this.AxisCollection[AxisIndex].SoftCWLS)
                {
                    this.LastError = string.Format("The abs position you are going to move exceeds the soft limitaion.");
                    return false;
                }
                else
                {
                    _pos_aftermove = Distance;

                    Distance = Distance - _curr_pos;
                }
            }
            else
            {
                _pos_aftermove = (int)(_curr_pos + Distance);

                if (Distance > 0) // CW
                {
                    if (_pos_aftermove > this.AxisCollection[AxisIndex].SoftCWLS)
                    {
                        this.LastError = string.Format("The position you are going to move exceeds the soft CW limitation.");
                        return false;
                    }
                }
                else // CCW
                {
                    if (_pos_aftermove < this.AxisCollection[AxisIndex].SoftCCWLS)
                    {
                        this.LastError = string.Format("The position you are going to move exceeds the soft CCW limitation.");
                        return false;
                    }
                }
            }

            try
            {
                // No need to move
                if (Distance == 0)
                    return true;

                // write the 'move' command to the controller
                CommandStruct cmd = new CommandStruct()
                {
                    Command = EnumCommand.MOVE,
                    AxisIndex = AxisIndex,
                    AccSteps = ACC_DEC_STEPS,
                    DriveVelocity = Velocity * MAX_VELOCITY / 100,
                    TotalSteps = Distance
                };
                _hid_device.Write(cmd.ToBytes());

                //! Wait for 2 report packages to ensure that the move command has been 
                //! executed by the device.
                uint _report_counter = this.Report.Counter + 2;

                do
                {
                    Thread.Sleep(10);
                } while (this.Report.Counter <= _report_counter);

                // the TRUE value of the IsRunning property indicates that the axis is running
                // wait until the running process is done
                while (this.Report.AxisStateCollection[AxisIndex].IsRunning)
                {
                    Thread.Sleep(10);
                }

                if (Report.AxisStateCollection[AxisIndex].Error != 0)
                {
                    this.LastError = string.Format("error code {0:d}", Report.AxisStateCollection[AxisIndex].Error);
                    return false;
                }
                else
                {
                    return true;
                }


            }
            catch (Exception ex)
            {
                this.LastError = ex.Message;
                return false;
            }
        }

       /// <summary>
       /// Move the speified axis asynchronously
       /// </summary>
       /// <param name="AxisIndex"></param>
       /// <param name="Acceleration"></param>
       /// <param name="Velocity"></param>
       /// <param name="Distance"></param>
       /// <param name="Direction"></param>
       /// <returns></returns>
        public Task<bool> MoveAsync(int AxisIndex, int Velocity, int Distance, MoveMode Mode)
        {
            return Task.Run<bool>(() =>
            {
                return Move(AxisIndex, Velocity, Distance, Mode);
            });
        }

        /// <summary>
        /// Stop the movement immediately
        /// </summary>
        /// <param name="AxisIndex">-1: Stop all axis; Otherwise, stop the specified axis</param>
        /// <returns></returns>
        public bool Stop(int AxisIndex)
        {
            // if the controller is not connected, return
            if (!this.IsConnected)
            {
                this.LastError = string.Format("The controller is not connected.");
                return false;
            }

            try
            {
                CommandStruct cmd = new CommandStruct()
                {
                    Command = EnumCommand.STOP,
                    AxisIndex = AxisIndex
                };
                _hid_device.Write(cmd.ToBytes());
               
                return true;
            }
            catch (Exception ex)
            {
                this.LastError = ex.Message;
                return false;
            }
        }
        
        /// <summary>
        /// Get the state of specified output port
        /// </summary>
        /// <param name="Channel"></param>
        /// <returns></returns>
        public OutputState GetGeneralOutputState(int Channel)
        {
            int axis_id = Channel / 2;
            int port = Channel % 2;

            if (port == 0)
                return this.Report.AxisStateCollection[axis_id].OUT_A;
            else
                return this.Report.AxisStateCollection[axis_id].OUT_B;
        }

        public void Dispose()
        {
            _hid_device.StopRead();
            _hid_device.Disconnect();
        }

        /// <summary>
        /// Set the state of the general output I/O
        /// </summary>
        /// <param name="Channel">This should be 0 to 7</param>
        /// <param name="State">OFF/ON</param>
        /// <returns></returns>
        public bool SetGeneralOutput(int Channel, OutputState State)
        {
            // if the controller is not connected, return
            if (!this.IsConnected)
            {
                this.LastError = string.Format("The controller is not connected.");
                return false;
            }

            try
            {
                CommandStruct cmd = new CommandStruct()
                {
                    Command = EnumCommand.GENOUT,
                    AxisIndex = Channel / 2,        // calculate axis index by channel
                    GenOutPort = Channel % 2,    // calculate port by channel
                    GenOutState = State

                };
                _hid_device.Write(cmd.ToBytes());

                return true;
            }
            catch (Exception ex)
            {
                this.LastError = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Flip the specified output port 
        /// </summary>
        /// <param name="Channel"></param>
        /// <returns></returns>
        public bool ToggleGeneralOutput(int Channel)
        {
            // if the controller is not connected, return
            if (!this.IsConnected)
            {
                this.LastError = string.Format("The controller is not connected.");
                return false;
            }

            try
            {
                // flip the state of output port
                OutputState state = GetGeneralOutputState(Channel);
                if (state == OutputState.Disabled)
                    state = OutputState.Enabled;
                else
                    state = OutputState.Disabled;

                CommandStruct cmd = new CommandStruct()
                {
                    Command = EnumCommand.GENOUT,
                    AxisIndex = Channel / 2,        // calculate axis index by channel
                    GenOutPort = Channel % 2,    // calculate port by channel
                    GenOutState = state

                };
                _hid_device.Write(cmd.ToBytes());

                return true;
            }
            catch (Exception ex)
            {
                this.LastError = ex.Message;
                return false;
            }
        }

        #endregion

        #region Events
        /// <summary>
        /// The connected hid device was disconnected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDisconnected(object sender, EventArgs e)
        {
            this.IsConnected = false;
            this.AxisCollection.Clear();
            this.Report.AxisStateCollection.Clear();

            OnConnectionStatusChanged?.Invoke(this, new ConnectionEventArgs(ConnectionEventArgs.EventType.ConnectionLost, null));
        }

        /// <summary>
        /// rasie this event when a data pack containing up-to-date hid report is received
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnReportReceived(object sender, byte[] e)
        {
            // the first byte is report id
            int _report_id = (int)e[0];

            // report of device state
            if (_report_id == REPORT_ID_DEVICESTATE)
            {

                // copy the previous report before parsing the new report raw data
                DeviceStateReport _previous_report = this.Report.Clone() as DeviceStateReport;

                // parse the report from the up-to-date raw data 
                this.Report.ParseRawData(e);

                // raise the event
                OnReportUpdated?.Invoke(this, this.Report);

                // if the state of input changes, raise the event
                for (int i = 0; i < this.Report.AxisStateCollection.Count; i++)
                {
                    if (this.Report.AxisStateCollection[i].IN_A != _previous_report.AxisStateCollection[i].IN_A)
                        OnInputIOStatusChanged?.Invoke(this, new InputEventArgs(i * 2, this.Report.AxisStateCollection[i].IN_A));

                    if (this.Report.AxisStateCollection[i].IN_B != _previous_report.AxisStateCollection[i].IN_B)
                        OnInputIOStatusChanged?.Invoke(this, new InputEventArgs(i * 2 + 1, this.Report.AxisStateCollection[i].IN_B));

                }
            }
            // report of firmware information
            else if (_report_id == REPORT_ID_FWINFO)
            {
                this.FirmwareInfo.VerMajor = BitConverter.ToInt32(e, 1);
                this.FirmwareInfo.VerMinor = BitConverter.ToInt32(e, 5);
                this.FirmwareInfo.VerRev = BitConverter.ToInt32(e, 9);
                
                int year = BitConverter.ToInt32(e, 13);
                int month = BitConverter.ToInt32(e, 17);
                int day = BitConverter.ToInt32(e, 21);

                this.FirmwareInfo.SetCompiledDate(year, month, day);
            }
        }
        #endregion

        #region RaisePropertyChangedEvent
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
}
