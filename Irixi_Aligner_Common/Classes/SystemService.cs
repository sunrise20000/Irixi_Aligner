using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Irixi_Aligner_Common.Classes.BaseClass;
using Irixi_Aligner_Common.Configuration;
using Irixi_Aligner_Common.Equipments;
using Irixi_Aligner_Common.Interfaces;
using Irixi_Aligner_Common.Message;
using Irixi_Aligner_Common.MotionControllerEntities;
using Irixi_Aligner_Common.MotionControllerEntities.BaseClass;
using IrixiStepperControllerHelper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Irixi_Aligner_Common.Classes
{
    public class SystemService : ViewModelBase, IDisposable
    {
        #region Variables
        
        SystemState _state = SystemState.IDLE;

        MessageItem _lastmsg = null;
        MessageHelper _msg_helper = new MessageHelper();
        
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
            ThreadPool.SetMinThreads(50, 50);

            // read the configuration from the file named SystemCfg.json
            // the file is located in \Configuration
            ConfigManager configmgr = SimpleIoc.Default.GetInstance<ConfigManager>();

            // whether output the log
            LogHelper.LogEnabled= configmgr.MotionControllerConfig.LogEnabled;

            // read version from AssemblyInfo.cs
            Version version = Assembly.GetExecutingAssembly().GetName().Version;

            // initialize the properties
            this.PhysicalMotionControllerCollection = new Dictionary<Guid, IMotionController>();
            this.LogicalAxisCollection = new ObservableCollectionEx<LogicalAxis>();
            this.LogicalMotionComponentCollection = new ObservableCollectionEx<LogicalMotionComponent>();
            this.State = SystemState.BUSY;


            StringBuilder sb = new StringBuilder();
            sb.Append("\r\n");
            sb.Append("> =================================================================\r\n");
            sb.Append("> =                 4x25G/10x10G Alignment System                 =\r\n");
            sb.Append("> =                    Copyright (C) 2017 Irixi                   =\r\n");
            sb.Append("> =================================================================\r\n");
            LogHelper.WriteLine(sb.ToString());

            this.LastMessage = new MessageItem(MessageType.Normal, "System startup ...");

            this.LastMessage = new MessageItem(MessageType.Normal, "Assembly Version {0}", version);

            // enumerate all physical motion controllers defined in the config file
            foreach (var config in configmgr.MotionControllerConfig.PhysicalMotionControllers)
            {
                IMotionController _mc = null;

                switch (config.Model)
                {
                    case ControllerType.LUMINOS_P6A:
                        _mc = new LuminosP6A(config);
                        break;

                    case ControllerType.THORLABS_TDC001:
                        //TODO create the instance of thorlabs TDC001
                        break;

                    case ControllerType.IRIXI_EE0017:
                        _mc = new IrixiEE0017(config);
                        ((IrixiEE0017)_mc).OnMessageReported += ((sender, message) =>
                        {

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                this.LastMessage = new MessageItem(MessageType.Normal, string.Format("{0} {1}", sender, message));
                            });
                        });
                        break;

                    default:
                        this.LastMessage = new MessageItem(MessageType.Error, "Unrecognized controller model {0}.", config.Model);
                        break;
                }

                // Add the controller to the dictionary<Guid, Controller>
                if (_mc != null)
                {
                    this.PhysicalMotionControllerCollection.Add(_mc.DevClass, _mc);
                }
            }

            // generate the Logical Motion Component
            foreach( var cfg_motion_comp in configmgr.MotionControllerConfig.LogicalMotionComponentArray)
            {
                LogicalMotionComponent comp = new LogicalMotionComponent(cfg_motion_comp.Caption, cfg_motion_comp.Icon);

                int axis_id = 0;
                foreach(var cfg_axis in cfg_motion_comp.LogicalAxisArray)
                {
                    // new logical axis object will be added to the Logical Motion Component
                    LogicalAxis axis = new LogicalAxis(this, cfg_axis, cfg_motion_comp.Caption, axis_id);

                    axis.OnHomeRequsted += LogicalAxis_OnHomeRequsted;
                    axis.OnMoveRequsted += LogicalAxis_OnMoveRequsted;
                    axis.OnStopRequsted += LigicalAxis_OnStopRequsted;

                    // bind the physical axis instance to logical axis object
                    BindPhysicalAxis(axis);

                    comp.LogicalAxisCollection.Add(axis);
                    this.LogicalAxisCollection.Add(axis);

                    axis_id++;
                }

                this.LogicalMotionComponentCollection.Add(comp);
            }

            // Initialize the cylinder controller
            // Currently, the Irixi Motion Controller is used to controller the cylinder
            try
            {

                IrixiEE0017 ctrl = PhysicalMotionControllerCollection[configmgr.MotionControllerConfig.Cylinder.HardwareClass] as IrixiEE0017;
                CylinderController = new CylinderController(
                ctrl,
                configmgr.MotionControllerConfig.Cylinder.PedalInput,
                configmgr.MotionControllerConfig.Cylinder.FiberClampOutput,
                configmgr.MotionControllerConfig.Cylinder.LensVacuumOutput,
                configmgr.MotionControllerConfig.Cylinder.PLCVacuumOutput,
                configmgr.MotionControllerConfig.Cylinder.PODVacuumOutput
                )
                {
                    IsEnabled = configmgr.MotionControllerConfig.Cylinder.Enabled
                };
            }
            catch (Exception e)
            {
                this.LastMessage = new MessageItem(MessageType.Error, "Unable to construct the cylinder controller class, {0}", e.Message);
            }

            Debug.WriteLine("{0} SystemService constructed!", DateTime.Now);

        }
        
        #endregion

        #region Events
        private void LogicalAxis_OnHomeRequsted(object sender, EventArgs args)
        {
            var s = sender as LogicalAxis;
            Home(s.PhysicalAxisInst);
        }

        private void LogicalAxis_OnMoveRequsted(object sender, MoveArgs args)
        {
            var s = sender as LogicalAxis;
            MoveLogicalAxis(s, args);
        }

        private void LigicalAxis_OnStopRequsted(object sender, EventArgs args)
        {
            var s = sender as LogicalAxis;
            s.PhysicalAxisInst.Stop();
        }

        #endregion

        #region Methods
        /// <summary>
        /// Bind the physical axis to the logical aligner
        /// </summary>
        /// <param name="ParentAligner">which logical aligner belongs to</param>
        /// <param name="Axis"></param>
        /// <returns></returns>
        private bool BindPhysicalAxis(LogicalAxis Axis)
        {
            bool ret = false;

            // find the physical motion controller by the device class
            if (this.PhysicalMotionControllerCollection.ContainsKey(Axis.Config.DeviceClass)) 
            {
                // find the axis in the specified controller by the axis name
                // and bind the physical axis to the logical axis
                Axis.PhysicalAxisInst = this.PhysicalMotionControllerCollection[Axis.Config.DeviceClass].FindAxisByName(Axis.Config.AxisName);
                
                if (Axis.PhysicalAxisInst == null) // if the physical axis was not found
                {
                    // Create a fake physical axis instance to tell UI this axis is disabled
                    Axis.PhysicalAxisInst = new AxisBase(-1, null, null);
                    
                    this.LastMessage = new MessageItem(MessageType.Error, "{0} bind physical axis error, unable to find the axis", Axis);

                    ret = false;
                }
                else 
                {
                    ret = true;
                }
            }
            else // the controller with the specified DevClass does not exist
            {
                // Create a fake physical axis instance to tell UI this axis is disabled
                Axis.PhysicalAxisInst = new AxisBase(-1, null, null);

                this.LastMessage = new MessageItem(MessageType.Error, "{0} bind physical axis error, unable to find the controller DevClass *{1}*", Axis, Axis.Config.DeviceClass);
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

        /// <summary>
        /// Initialize all devices in the system
        /// </summary>
        public async void Init()
        {

            bool[] ret;
            List<Task<bool>> _tasks = new List<Task<bool>>();
            List<IBaseEquipment> _equipments = new List<IBaseEquipment>();

            Debug.WriteLine("{0} SystemService initializing ...", DateTime.Now);

            SetSystemState(SystemState.BUSY);

            #region Initialize motion controllers

            // initialize all motion controllers simultaneously
            foreach (var _mc in this.PhysicalMotionControllerCollection.Values)
            {
                if (_mc.IsEnabled)
                {
                    var task = _mc.Init();
                    task.Start();
                    _tasks.Add(task);
                    _equipments.Add(_mc);
                    this.LastMessage = new MessageItem(MessageType.Normal, "{0} Initializing ...", _mc);

                    // update UI immediately
                    await Task.Delay(50);
                }
            }

            while(_tasks.Count > 0)
            {
                // Wait until all init tasks were done
                Task<bool> t = await Task.WhenAny(_tasks);

                int id = _tasks.IndexOf(t);


                if (t.Result)
                    this.LastMessage = new MessageItem(MessageType.Good, "{0} Initialization is completed.", _equipments[id]);
                else
                    this.LastMessage = new MessageItem(MessageType.Error, "{0} Initialization is failed, {1}", _equipments[id], _equipments[id].LastError);

                _tasks.RemoveAt(id);
                _equipments.RemoveAt(id);
            }

           

            #endregion

            #region Clear the conllections in order to restart the initilization process

            _tasks.Clear();
            _equipments.Clear();

            #endregion

            /*
             * The following process is initializing the equipments that are based on 
             * the motion controller.
             * 
             * For example, in practice, the cylinder controller is attached to the Irixi motion controllers,
             * if the corresponding motion controller is not available, the cylinder controller is not available. 
             */

            #region Initialize the other equipments
            
            // initialize the cylinder controller
            var _t = this.CylinderController.Init();
            _t.Start();
            _tasks.Add(_t);
            _equipments.Add(this.CylinderController);


            this.LastMessage = new MessageItem(MessageType.Normal, "{0} Initializing ...", this.CylinderController);

            ret = await Task.WhenAll(_tasks);

            // Output information according the init result
            for (int i = 0; i < ret.Length; i++)
            {
                if (ret[i])
                    this.LastMessage = new MessageItem(MessageType.Good, "{0} Initialization is completed.", _equipments[i]);
                else
                    this.LastMessage = new MessageItem(MessageType.Error, "{0} Initialization is failed, {1}", _equipments[i], _equipments[i].LastError);
            }

            #endregion

            SetSystemState(SystemState.IDLE);

            Debug.WriteLine("{0} SystemService initialized!", DateTime.Now);

        }

        /// <summary>
        /// Move the specified axis with specified args
        /// </summary>
        /// <param name="Axis"></param>
        /// <param name="Args"></param>
        public async void MoveLogicalAxis(LogicalAxis Axis, MoveArgs Args)
        {
            if(GetSystemState() != SystemState.BUSY)
            {
                SetSystemState(SystemState.BUSY);

                this.LastMessage = new MessageItem(MessageType.Normal, "{0} Move with argument {1} ...", Axis, Args);

                var t = Axis.PhysicalAxisInst.Move(Args.Mode, Args.Speed, Args.Distance);
                t.Start();
                bool ret = await t;
                if (ret == false)
                {
                    this.LastMessage = new MessageItem(MessageType.Error, "{0} Unable to move, {1}", Axis, Axis.PhysicalAxisInst.LastError);

                    Messenger.Default.Send<NotificationMessage<string>>(new NotificationMessage<string>(this.LastMessage.Message, "Error"));
                }
                else
                {
                    this.LastMessage = new MessageItem(MessageType.Normal, "{0} Move is completed, the final position is {1}", Axis, Axis.PhysicalAxisInst.AbsPosition);
                }

                SetSystemState(SystemState.IDLE);
            }
            else
            {
                this.LastMessage = new MessageItem(MessageType.Warning, "System is busy");
            }
        }

        /// <summary>
        /// Move a set of logical axes with the specified order
        /// </summary>
        /// <param name="AxesGroup"></param>
        /// <remarks>
        /// An args is consisted of 3 elements: Move Order, Logical Axis, How to Move
        /// </remarks>
        public async void MassMoveLogicalAxis(Tuple<int, LogicalAxis, MoveArgs>[] AxesGroup)
        {
            if (GetSystemState() != SystemState.BUSY)
            {
                SetSystemState(SystemState.BUSY);

                // how many axes to be moved
                int _total_to_move = AxesGroup.Length;

                // how many axes have been moved
                int _moved_cnt = 0;

                int _present_order = 0;
                

                // generate a list which contains the movement tasks
                // this is used by the Task.WhenAll() function
                List<Task<bool>> _move_tasks = new List<Task<bool>>();
                List<LogicalAxis> _axis_moving = new List<LogicalAxis>();

                this.LastMessage = new MessageItem(MessageType.Normal, "Executing mass move ...");

                do
                {
                    // clear the previous tasks
                    _move_tasks.Clear();
                    _axis_moving.Clear();

                    // find the axes which belong to current order
                    foreach (var item in AxesGroup)
                    {
                        var _order = item.Item1;
                        var _axis = item.Item2;
                        var _arg = item.Item3;

                        if (_order == _present_order)
                        {
                            var t = _axis.PhysicalAxisInst.Move(_arg.Mode, _arg.Speed, _arg.Distance);
                            t.Start();
                            _move_tasks.Add(t);
                            _axis_moving.Add(_axis);
                        }
                    }

                    // if no axes to be moved, move to the next loop
                    if (_move_tasks.Count > 0)
                    {
                        while (_move_tasks.Count > 0)
                        {
                            // wait until all the axes are moved
                            Task<bool> t = await Task.WhenAny(_move_tasks);
                            int id = _move_tasks.IndexOf(t);

                            if(t.Result)
                                this.LastMessage = new MessageItem(MessageType.Good, "{0} Move is completed.", _axis_moving[id]);
                            else
                                this.LastMessage = new MessageItem(MessageType.Error, "{0} Move is failed, {1}", _axis_moving[id], _axis_moving[id].PhysicalAxisInst.LastError);

                            _move_tasks.RemoveAt(id);
                            _move_tasks.RemoveAt(id);

                            // save the sum of homed axes in order to check if all axes have been homed
                            _moved_cnt++;
                        }
                    }
                    
                    // set the order of next loop
                    _present_order++;

                } while (_moved_cnt < _total_to_move); // loop until all axes were moved

                

                this.LastMessage = new MessageItem(MessageType.Good, "Simultanenous Movement is completed");

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
        public async void MassHome()
        {
            if (GetSystemState() == SystemState.IDLE)
            {
                int _present_order = 0;
                int _homed_cnt = 0;
                int _total_axis = this.LogicalAxisCollection.Count;
                List<Task<bool>> _tasks = new List<Task<bool>>();
                List<LogicalAxis> _axis_homing = new List<LogicalAxis>();

                SetSystemState(SystemState.BUSY);

                // update UI immediately
                await Task.Delay(50);

                // Loop Home() function of each axis
                do
                {
                    //this.LastMessage = new MessageItem(MessageType.Normal, "The present homing order is {0}", _present_order);

                    _axis_homing.Clear();
                    _tasks.Clear();
                    // find the axes which are to be homed in current stage
                    foreach (var axis in this.LogicalAxisCollection)
                    {
                        if (axis.Config.HomeOrder == _present_order)
                        {
                            this.LastMessage = new MessageItem(MessageType.Normal, "{0} Start to home ...", axis);

                            var t = axis.PhysicalAxisInst.Home();
                            t.Start();
                            _tasks.Add(t);
                            _axis_homing.Add(axis);

                            // update UI immediately
                            await Task.Delay(10);

                        }
                    }

                    if (_tasks.Count <= 0)
                    {
                        //this.LastMessage = new MessageItem(MessageType.Warning, "There are not axes to be homed in this order");
                    }
                    else
                    {
                        while (_tasks.Count > 0)
                        {
                            Task<bool> t = await Task.WhenAny(_tasks);
                            int id = _tasks.IndexOf(t);

                            if (t.Result)
                                this.LastMessage = new MessageItem(MessageType.Good, "{0} Home is completed.", _axis_homing[id]);
                            else
                                this.LastMessage = new MessageItem(MessageType.Error, "{0} Home is failed, {1}", _axis_homing[id], _axis_homing[id].PhysicalAxisInst.LastError);

                            _tasks.RemoveAt(id);
                            _axis_homing.RemoveAt(id);

                            // save the sum of homed axes in order to check if all axes have been homed
                            _homed_cnt ++;
                        }
                    }

                    // Move to next order
                    _present_order++;

                } while (_homed_cnt < _total_axis);

                this.LastMessage = new MessageItem(MessageType.Good, "Mass Home is completed");

                SetSystemState(SystemState.IDLE);

            }
            else
            {
                this.LastMessage = new MessageItem(MessageType.Warning, "System is busy");
            }
        }

        #region Cylinder Control
        #region Fiber Clamp Control
        public void FiberClampON()
        {
            if (GetSystemState() == SystemState.IDLE)
            {
                SetSystemState(SystemState.BUSY);
                this.CylinderController.SetFiberClampState(OutputState.Enabled);
                LogHelper.WriteLine("Fiber Clamp is opened", LogHelper.LogType.NORMAL);
                SetSystemState(SystemState.IDLE);
            }
        }

        public void FiberClampOFF()
        {
            if (GetSystemState() == SystemState.IDLE)
            {
                SetSystemState(SystemState.BUSY);
                this.CylinderController.SetFiberClampState(OutputState.Disabled);

                LogHelper.WriteLine("Fiber Clamp is closed", LogHelper.LogType.NORMAL);
                SetSystemState(SystemState.IDLE);
            }
        }

        public void ToggleFiberClampState()
        {
            if (GetSystemState() == SystemState.IDLE)
            {
                SetSystemState(SystemState.BUSY);
                if (this.CylinderController.FiberClampState == OutputState.Disabled)
                {
                    this.CylinderController.SetFiberClampState(OutputState.Enabled);
                    LogHelper.WriteLine("Fiber Clamp is opened", LogHelper.LogType.NORMAL);
                }
                else
                {
                    this.CylinderController.SetFiberClampState(OutputState.Disabled);
                    LogHelper.WriteLine("Fiber Clamp is closed", LogHelper.LogType.NORMAL);
                }
                SetSystemState(SystemState.IDLE);
            }
        }
        #endregion

        #region Lens Vacuum Control
        public void LensVacuumON()
        {
            if (GetSystemState() == SystemState.IDLE)
            {
                SetSystemState(SystemState.BUSY);
                this.CylinderController.SetLensVacuumState(OutputState.Enabled);
                LogHelper.WriteLine("Lens Vacuum is opened", LogHelper.LogType.NORMAL);
                SetSystemState(SystemState.IDLE);
            }
        }

        public void LensVacuumOFF()
        {
            if (GetSystemState() == SystemState.IDLE)
            {
                SetSystemState(SystemState.BUSY);
                this.CylinderController.SetLensVacuumState(OutputState.Disabled);
                LogHelper.WriteLine("Lens Vacuum is closed", LogHelper.LogType.NORMAL);
                SetSystemState(SystemState.IDLE);
            }
        }

        public void ToggleLensVacuumState()
        {
            if (GetSystemState() == SystemState.IDLE)
            {
                SetSystemState(SystemState.BUSY);
                if (this.CylinderController.LensVacuumState == OutputState.Disabled)
                {
                    this.CylinderController.SetLensVacuumState(OutputState.Enabled);
                    LogHelper.WriteLine("Lens Vacuum is opened", LogHelper.LogType.NORMAL);
                }
                else
                {
                    this.CylinderController.SetLensVacuumState(OutputState.Disabled);
                    LogHelper.WriteLine("Lens Vacuum is closed", LogHelper.LogType.NORMAL);
                }
                SetSystemState(SystemState.IDLE);
            }
        }
        #endregion

        #region PLC Vacuum Control
        public void PlcVacuumON()
        {
            if (GetSystemState() == SystemState.IDLE)
            {
                SetSystemState(SystemState.BUSY);
                this.CylinderController.SetPlcVacuumState(OutputState.Enabled);
                LogHelper.WriteLine("PLC Vacuum is opened", LogHelper.LogType.NORMAL);
                SetSystemState(SystemState.IDLE);
            }
        }

        public void PlcVacuumOFF()
        {
            if (GetSystemState() == SystemState.IDLE)
            {
                SetSystemState(SystemState.BUSY);
                this.CylinderController.SetPlcVacuumState(OutputState.Disabled);
                LogHelper.WriteLine("PLC Vacuum is closed", LogHelper.LogType.NORMAL);
                SetSystemState(SystemState.IDLE);
            }
        }

        public void TogglePlcVacuumState()
        {
            if (GetSystemState() == SystemState.IDLE)
            {
                SetSystemState(SystemState.BUSY);
                if (this.CylinderController.PlcVacuumState == OutputState.Disabled)
                {
                    this.CylinderController.SetPlcVacuumState(OutputState.Enabled);
                    LogHelper.WriteLine("PLC Vacuum is opened", LogHelper.LogType.NORMAL);
                }
                else
                {
                    this.CylinderController.SetPlcVacuumState(OutputState.Disabled);
                    LogHelper.WriteLine("PLC Vacuum is closed", LogHelper.LogType.NORMAL);
                }
                SetSystemState(SystemState.IDLE);
            }
        }
        #endregion

        #region POD Vacuum Control
        public void PodVacuumON()
        {
            if (GetSystemState() == SystemState.IDLE)
            {
                SetSystemState(SystemState.BUSY);
                this.CylinderController.SetPodVacuumState(OutputState.Enabled);
                LogHelper.WriteLine("POD Vacuum is opened", LogHelper.LogType.NORMAL);
                SetSystemState(SystemState.IDLE);
            }
        }

        public void PodVacuumOFF()
        {
            if (GetSystemState() == SystemState.IDLE)
            {
                SetSystemState(SystemState.BUSY);
                this.CylinderController.SetPodVacuumState(OutputState.Disabled);
                LogHelper.WriteLine("POD Vacuum is opened", LogHelper.LogType.NORMAL);
                SetSystemState(SystemState.IDLE);
            }
        }

        public void TogglePodVacuumState()
        {
            if (GetSystemState() == SystemState.IDLE)
            {
                SetSystemState(SystemState.BUSY);
                if (this.CylinderController.PodVacuumState == OutputState.Disabled)
                {
                    this.CylinderController.SetPodVacuumState(OutputState.Enabled);
                    LogHelper.WriteLine("POD Vacuum is opened", LogHelper.LogType.NORMAL);
                }
                else
                {
                    this.CylinderController.SetPodVacuumState(OutputState.Disabled);
                    LogHelper.WriteLine("POD Vacuum is opened", LogHelper.LogType.NORMAL);
                }
                SetSystemState(SystemState.IDLE);
            }
        }
        #endregion
        #endregion

        /// <summary>
        /// Start running user program
        /// </summary>
        private void Start()
        {
            //TODO here we should judge whether the auto-program is paused or not
            // if the auto-program is not worked and an auto-program has been selected, run it;
            // otherwise, continue to run the last paused auto-program
        }

        /// <summary>
        /// Stop the moving axes or stop running the user program
        /// </summary>
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

        public void Dispose()
        {
            foreach (var ctrl in this.PhysicalMotionControllerCollection)
            {
                ctrl.Value.Dispose();
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
        /// Get the instance of the Cylinder Controller Class
        /// </summary>
        public CylinderController CylinderController
        {
            private set;
            get;
        }
        
        /// <summary>
        /// Get the instance collection of the Motion Controller Class
        /// </summary>
        public Dictionary<Guid, IMotionController> PhysicalMotionControllerCollection
        {
            private set;
            get;
        }

        /// <summary>
        /// Create a collection that contains all logical axes defined in the config file.
        /// this list enable users to operate each axis independently without knowing which physical motion controller it belongs to
        /// </summary>
        public ObservableCollectionEx<LogicalAxis> LogicalAxisCollection
        {
            private set;
            get;
        }

        /// <summary>
        /// Get the layout of logical aligner.
        /// the UI components are binded to this property
        /// </summary>
        public ObservableCollectionEx<LogicalMotionComponent> LogicalMotionComponentCollection
        {
            private set;
            get;
        }
        
        /// <summary>
        /// Set or get the last message.
        /// this message will be added into this.MessageCollection
        /// </summary>
        public MessageItem LastMessage
        {
            private set
            {
                UpdateProperty<MessageItem>(ref _lastmsg, value);
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
        public MessageHelper MessageCollection
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
                    MassHome();
                });
            }
        }

        public RelayCommand CommandToggleFiberClampState
        {
            get
            {
                return new RelayCommand(() =>
              {
                  ToggleFiberClampState();
              });
            }
        }

        public RelayCommand CommandToggleLensVacuumState
        {
            get
            {
                return new RelayCommand(() =>
                {
                    ToggleLensVacuumState();
                });
            }
        }

        public RelayCommand CommandTogglePlcVacuumState
        {
            get
            {
                return new RelayCommand(() =>
                {
                    TogglePlcVacuumState();
                });
            }
        }

        public RelayCommand CommandTogglePodVacuumState
        {
            get
            {
                return new RelayCommand(() =>
                {
                    TogglePodVacuumState();
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
