using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Irixi_Aligner_Common.Classes.BaseClass;
using Irixi_Aligner_Common.Configuration;
using Irixi_Aligner_Common.Interfaces;
using Irixi_Aligner_Common.Message;
using Irixi_Aligner_Common.MotionControllerEntities;
using Irixi_Aligner_Common.ViewModel;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Irixi_Aligner_Common.Classes
{
    public class SystemService : ViewModelBase
    {
        #region Variables
        
        SystemState _state = SystemState.IDLE;

        CMessageItem _lastmsg = null;
        CMessageHelper _msg_helper = new CMessageHelper();
        
        /// <summary>
        /// lock while increase/decrease the home counter
        /// </summary>
        readonly object _home_counter_lock = new object();
        readonly object _move_counter_lock = new object();

        /// <summary>
        /// lock while set or get this.State
        /// </summary>
        readonly object _sys_state_lock = new object();

        #endregion


        #region Constructor

        public SystemService()
        {
            // read the configuration from the file named SystemCfg.json
            // the file is located in \Configuration
            var locator = Application.Current.Resources["Locator"] as ViewModelLocator;
            ConfigManager configmgr = locator.Configuration;

            // whether output the log
            LogHelper.LogEnabled= configmgr.MotionController.LogEnabled;

            // initialize the properties
            this.PhysicalMotionControllerCollection = new Dictionary<Guid, IMotionController>();
            this.LogicalAxisCollection = new ObservableCollectionEx<ConfigLogicalAxis>();
            this.LogicalAlignerlayout = configmgr.MotionController.LogicalLayout;
            this.State = SystemState.BUSY;


            StringBuilder sb = new StringBuilder();
            sb.Append("\r\n");
            sb.Append("> =================================================================\r\n");
            sb.Append("> =                 4x25G/10x10G Alignment System                 =\r\n");
            sb.Append("> =                    Copyright (C) 2017 Irixi                   =\r\n");
            sb.Append("> =================================================================\r\n");
            LogHelper.WriteLine(sb.ToString());

            this.LastMessage = new CMessageItem(MessageType.Normal, "System startup ...");

            // enumerate all physical motion controllers defined in the config file
            foreach (var cfg in configmgr.MotionController.PhysicalMotionControllers)
            {
                IMotionController ctrler = null;

                switch (cfg.Model)
                {
                    case MotionControllerModel.LUMINOS_P6A:
                        ctrler = new LuminosP6A(cfg);
                        break;

                    case MotionControllerModel.THORLABS_TDC001:
                        //TODO create the instance of thorlabs TDC001
                        break;

                    case MotionControllerModel.IRIXI_EE0017:
                        ctrler = new IrixiEE0017(cfg);
                        ((IrixiEE0017)ctrler).OnPushMessage += ((sender, message) =>
                        {

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                this.LastMessage = new CMessageItem(MessageType.Normal, string.Format("{0} {1}", sender, message));
                            });
                        });
                        break;

                    default:
                        this.LastMessage = new CMessageItem(MessageType.Error, "Unrecognized controller model {0}.", cfg.Model);
                        break;
                }

                // Add the controller to the dictionary<Guid, Controller>
                if (ctrler != null)
                {
                    this.PhysicalMotionControllerCollection.Add(ctrler.DevClass, ctrler);
                }
            }

            // bind the physical axis to logical motion controller
            ConfigLogicalAligner aligner = null;
            
            aligner = this.LogicalAlignerlayout.VGrooveAligner;
            BindPhysicalAxis(ref aligner, aligner.AxisX);
            BindPhysicalAxis(ref aligner, aligner.AxisY);
            BindPhysicalAxis(ref aligner, aligner.AxisZ);
            BindPhysicalAxis(ref aligner, aligner.AxisRoll);
            BindPhysicalAxis(ref aligner, aligner.AxisYaw);
            BindPhysicalAxis(ref aligner, aligner.AxisPitch);

            aligner = this.LogicalAlignerlayout.LensAligner;
            BindPhysicalAxis(ref aligner, aligner.AxisX);
            BindPhysicalAxis(ref aligner, aligner.AxisY);
            BindPhysicalAxis(ref aligner, aligner.AxisZ);
            BindPhysicalAxis(ref aligner, aligner.AxisRoll);
            BindPhysicalAxis(ref aligner, aligner.AxisYaw);
            BindPhysicalAxis(ref aligner, aligner.AxisPitch);

            aligner = this.LogicalAlignerlayout.COSAligner;
            BindPhysicalAxis(ref aligner, aligner.AxisX);
            BindPhysicalAxis(ref aligner, aligner.AxisY);
            BindPhysicalAxis(ref aligner, aligner.AxisZ);
            BindPhysicalAxis(ref aligner, aligner.AxisRoll);
            BindPhysicalAxis(ref aligner, aligner.AxisYaw);
            BindPhysicalAxis(ref aligner, aligner.AxisPitch);

            aligner = this.LogicalAlignerlayout.TopCamera;
            BindPhysicalAxis(ref aligner, aligner.AxisX);
            BindPhysicalAxis(ref aligner, aligner.AxisY);
            BindPhysicalAxis(ref aligner, aligner.AxisZ);

            aligner = this.LogicalAlignerlayout.AngleCamera;
            BindPhysicalAxis(ref aligner, aligner.AxisX);
            BindPhysicalAxis(ref aligner, aligner.AxisY);
            BindPhysicalAxis(ref aligner, aligner.AxisZ);

            aligner = this.LogicalAlignerlayout.FrontCamera;
            BindPhysicalAxis(ref aligner, aligner.AxisX);
            BindPhysicalAxis(ref aligner, aligner.AxisY);
            BindPhysicalAxis(ref aligner, aligner.AxisZ);
        }

        #endregion

        #region Events

        #endregion


        #region Methods
        /// <summary>
        /// Bind the physical axis to the logical aligner
        /// </summary>
        /// <param name="ParentAligner">which logical aligner belongs to</param>
        /// <param name="LogicalAxis"></param>
        /// <returns></returns>
        private bool BindPhysicalAxis(ref ConfigLogicalAligner ParentAligner, ConfigLogicalAxis LogicalAxis)
        {
            bool ret = false;

            LogicalAxis.Parent = ParentAligner; // store the parent aligner to the logical axis

            if (this.PhysicalMotionControllerCollection.ContainsKey(LogicalAxis.DeviceClass)) // check if the motion controller with the specified devclass exists?
            {
                // find the axis in the specified controller by the axis name
                LogicalAxis.PhysicalAxisInst = this.PhysicalMotionControllerCollection[LogicalAxis.DeviceClass].FindAxisByName(LogicalAxis.AxisName);
                
                if (LogicalAxis.PhysicalAxisInst == null) // if the physical axis was not found
                {
                    // Create a fake physical axis instance to tell UI this axis is disabled
                    LogicalAxis.PhysicalAxisInst = new AxisBase(-1, null, null);
                    
                    this.LastMessage = new CMessageItem(MessageType.Error, "Bind physical axis error, unable to find the axis {0}", LogicalAxis);

                    ret = false;
                }
                else
                {
                    this.LogicalAxisCollection.Add(LogicalAxis);
                    ret = true;
                }
            }
            else
            {
                // Create a fake physical axis instance to tell UI this axis is disabled
                LogicalAxis.PhysicalAxisInst = new AxisBase(-1, null, null);

                this.LastMessage = new CMessageItem(MessageType.Error, "Bind physical axis error, unable to find the controller with DevClass of *{0}*", LogicalAxis.DeviceClass);
                ret = false;
            }

            return ret;
        }

        private void SetSystemState(SystemState State)
        {
            lock(_sys_state_lock)
            {
                this.State = State;
            }
        }

        private SystemState GetSystemState()
        {
            lock (_sys_state_lock)
            {
                return this.State;
            }
        }

        public async void Init()
        {
            
            List<Task<bool>> _tasks = new List<Task<bool>>();
            List<IMotionController> _controllers = new List<IMotionController>();

            // start all controller's init() functions simultaneously
            foreach (var item in this.PhysicalMotionControllerCollection.Values)
            {
                var task = item.Init();
                task.Start();
                _tasks.Add(task);
                _controllers.Add(item);
                this.LastMessage = new CMessageItem(MessageType.Normal, "{0} Initializing ...", item);
            }

            this.LastMessage = new CMessageItem(MessageType.Normal, "Waiting ...");

            // Wait until all init tasks were done
            bool[] ret = await Task.WhenAll(_tasks);

            // Output information according the init result
            for (int i = 0; i < ret.Length; i++)
            {
                if(ret[i])
                    this.LastMessage = new CMessageItem(MessageType.Good, "{0} Initialization is completed.", _controllers[i]);
                else
                    this.LastMessage = new CMessageItem(MessageType.Error, "{0} Initialization is failed, {1}", _controllers[i], _controllers[i].LastError);
            }

            SetSystemState(SystemState.IDLE);
            
        }

        /// <summary>
        /// Move the specified axis with specified args
        /// </summary>
        /// <param name="Axis"></param>
        /// <param name="Args"></param>
        public async void MoveLogicalAxis(ConfigLogicalAxis Axis, MoveArgs Args)
        {
            if(GetSystemState() != SystemState.BUSY)
            {
                SetSystemState(SystemState.BUSY);

                this.LastMessage = new CMessageItem(MessageType.Normal, "{0} Move with argument {1} ...", Axis, Args);

                var t = Axis.PhysicalAxisInst.Move(Args.Mode, Args.Speed, Args.Distance);
                t.Start();
                bool ret = await t;
                if (ret == false)
                {
                    this.LastMessage = new CMessageItem(MessageType.Error, "{0} Unable to move, {1}", Axis, Axis.PhysicalAxisInst.LastError);

                    Messenger.Default.Send<NotificationMessage<string>>(new NotificationMessage<string>(this.LastMessage.Message, "Error"));
                }
                else
                {
                    this.LastMessage = new CMessageItem(MessageType.Normal, "{0} Move is completed, the final position is {1}", Axis, Axis.PhysicalAxisInst.AbsPosition);
                }

                SetSystemState(SystemState.IDLE);
            }
            else
            {
                this.LastMessage = new CMessageItem(MessageType.Error, "System is busy");
            }
        }

        /// <summary>
        /// Move a set of axes with the specified args
        /// </summary>
        /// <param name="Axis"></param>
        /// <param name="Args"></param>
        public async void MoveAxesSimultaneously(Tuple<IAxis, MoveArgs>[] Args)
        {
            if (GetSystemState() != SystemState.BUSY)
            {
                SetSystemState(SystemState.BUSY);

                List<Task<bool>> _move_tasks = new List<Task<bool>>();

                foreach (var item in Args)
                {
                    var axis = item.Item1;
                    var arg = item.Item2;

                    var t = axis.Move(arg.Mode, arg.Speed, arg.Distance);
                    t.Start();
                    _move_tasks.Add(t);
                }

                this.LastMessage = new CMessageItem(MessageType.Normal, "Execute simultaneous movement ...");

                bool[] ret = await Task.WhenAll(_move_tasks);

                for (int i = 0; i < _move_tasks.Count; i++)
                {
                    if(ret[i] == false)
                    {
                        this.LastMessage = new CMessageItem(MessageType.Error, "{0} Move is failed, {1}", Args[i].Item1, Args[i].Item1.LastError);
                        Messenger.Default.Send<NotificationMessage<string>>(new NotificationMessage<string>(this.LastMessage.Message, "Error"));
                    }
                }

                this.LastMessage = new CMessageItem(MessageType.Good, "Simultanenous Movement is done");

                SetSystemState(SystemState.IDLE);
            }
        }

        /// <summary>
        /// Home the specified axis
        /// </summary>
        /// <param name="Axis"></param>
        public async void Home(IAxis Axis)
        {
            if(GetSystemState() == SystemState.IDLE)
            {
                SetSystemState(SystemState.BUSY);
                bool ret = await Axis.Home();
                SetSystemState(SystemState.IDLE);
            }
        }

        /// <summary>
        /// home all axes in system
        /// </summary>
        public async void HomeAllAxes()
        {
            if (GetSystemState() == SystemState.IDLE)
            {
                int _present_order = 0;
                int _axis_homed = 0;
                int _total_axis = this.LogicalAxisCollection.Count;
                List<Task<bool>> _tasks = new List<Task<bool>>();
                List<ConfigLogicalAxis> _tmp_axis_homing = new List<ConfigLogicalAxis>();

                SetSystemState(SystemState.BUSY);

                // Loop Home() function of each axis
                do
                {
                    this.LastMessage = new CMessageItem(MessageType.Normal, "The present homing order is {0}", _present_order);

                    _tmp_axis_homing.Clear();
                    _tasks.Clear();
                    // find the axes which are to be homed in current stage
                    foreach (var axis in this.LogicalAxisCollection)
                    {
                        if (axis.HomeOrder == _present_order)
                        {
                            this.LastMessage = new CMessageItem(MessageType.Normal, "{0} Start to home ...", axis);

                            var t = axis.PhysicalAxisInst.Home();
                            t.Start();
                            _tasks.Add(t);
                            _tmp_axis_homing.Add(axis);

                        }
                    }

                    if (_tasks.Count <= 0)
                    {
                        this.LastMessage = new CMessageItem(MessageType.Warning, "There are not axes to be homed in this order");
                    }
                    else
                    {
                        this.LastMessage = new CMessageItem(MessageType.Normal, "Waiting ...");

                        // Wait asynchronoursly until all home tasks are done
                        bool[] ret = await Task.WhenAll(_tasks);

                        // Output the messages to indicate whether the specified axis were completely homed or not
                        for (int i = 0; i < ret.Length; i++)
                        {
                            if (ret[i])
                                this.LastMessage = new CMessageItem(MessageType.Good, "{0} Home is completed.", _tmp_axis_homing[i]);
                            else
                                this.LastMessage = new CMessageItem(MessageType.Error, "{0} Home is failed, {1}", _tmp_axis_homing[i], _tmp_axis_homing[i].PhysicalAxisInst.LastError);
                        }

                        // save the sum of homed axes in order to check if all axes have been homed
                        _axis_homed += _tasks.Count;
                        
                    }

                    // Move to next order
                    _present_order++;

                } while (_axis_homed < _total_axis);

                this.LastMessage = new CMessageItem(MessageType.Good, "Batch Home is completed");

                SetSystemState(SystemState.IDLE);

            }
            else
            {
                this.LastMessage = new CMessageItem(MessageType.Error, "System is busy");
            }
        }

        private void Start()
        {
            //TODO here we should judge whether the auto-program is paused or not
            // if the auto-program is not worked and an auto-program has been selected, run it;
            // otherwise, continue to run the last paused auto-program
        }

        private void Stop()
        {
            //TODO here we should judge whether the auto-program is running or not 
            // if the auto-program is running, stop it;
            // otherwise, stop all moving axis

            foreach(var controller in this.PhysicalMotionControllerCollection.Values)
            {
                controller.Stop();
            }
        }

        /// <summary>
        /// Toggle the move mode between ABS and REL
        /// </summary>
        /// <param name="Axis">The instance of physical axis</param>
        public void ToggleAxisMoveMode(IAxis Axis)
        {
            if (GetSystemState() == SystemState.IDLE)
            {
                SetSystemState(SystemState.BUSY);
                Axis.ToggleMoveMode();
                SetSystemState(SystemState.IDLE);
            }
        }
        #endregion

        #region Properties

        bool _is_inited = false;
        /// <summary>
        /// Does the system service have been initialized ?
        /// it's mainly used to set the enabled property of UI elements.
        /// </summary>
        public bool IsInitialized
        {
            private set
            {
                UpdateProperty<bool>(ref _is_inited, value);
            }
            get
            {
                return _is_inited;
            }
        }

        /// <summary>
        /// Get the system status
        /// </summary>
        public SystemState State
        {
            private set
            {
                UpdateProperty<SystemState>(ref _state, value);
            }
            get
            {
                return _state;
            }
        }
        
        /// <summary>
        /// Get the physical motion controller collection
        /// </summary>
        public Dictionary<Guid, IMotionController> PhysicalMotionControllerCollection
        {
            private set;
            get;
        }

        /// <summary>
        /// Create a collection that contains all axes defined in the config file.
        /// this list enable users to operate each axis independently without knowing which physical motion controller it belongs to
        /// </summary>
        public ObservableCollectionEx<ConfigLogicalAxis> LogicalAxisCollection
        {
            private set;
            get;
        }

        /// <summary>
        /// Get the layout of logical aligner.
        /// the UI components are binded to this property
        /// </summary>
        public ConfigLogicalLayout LogicalAlignerlayout
        {
            private set;
            get;
        }
        
        /// <summary>
        /// Set or get the last message.
        /// this message will be added into this.MessageCollection
        /// </summary>
        public CMessageItem LastMessage
        {
            private set
            {
                UpdateProperty<CMessageItem>(ref _lastmsg, value);
                MessageCollection.Add(_lastmsg);
            }
            get
            {
                return _lastmsg;
            }
        }

        /// <summary>
        /// Get the collection of messages.
        /// </summary>
        public CMessageHelper MessageCollection
        {
            get
            {
                return _msg_helper;
            }
        }
 
        #endregion

        #region ICommand

        public RelayCommand<IAxis> CommandHome
        {
            get
            {
                return new RelayCommand<IAxis>(axis =>
                {
                    Home(axis);
                });
            }
        }

        public RelayCommand CommandHomeAllAxes
        {
            get
            {
                return new RelayCommand(() =>
                {
                    HomeAllAxes();
                });
            }
        }

        public RelayCommand CommandStart
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Start();
                });
            }
        }

        public RelayCommand CommandStop
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Stop();
                });
            }
        }

        #endregion

        #region RaisePropertyChangedEvent
        ////public event PropertyChangedEventHandler PropertyChanged;

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
            ////PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
            RaisePropertyChanged(PropertyName);

        }
        #endregion

    }
}
