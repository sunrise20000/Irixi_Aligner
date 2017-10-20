using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Irixi_Aligner_Common.Alignment.AlignmentXD;
using Irixi_Aligner_Common.Alignment.Base;
using Irixi_Aligner_Common.Alignment.SpiralScan;
using Irixi_Aligner_Common.Classes.BaseClass;
using Irixi_Aligner_Common.Configuration.Common;
using Irixi_Aligner_Common.Equipments.Base;
using Irixi_Aligner_Common.Equipments.Equipments;
using Irixi_Aligner_Common.Equipments.Instruments;
using Irixi_Aligner_Common.Interfaces;
using Irixi_Aligner_Common.Message;
using Irixi_Aligner_Common.MotionControllers.Base;
using Irixi_Aligner_Common.MotionControllers.Irixi;
using Irixi_Aligner_Common.MotionControllers.Luminos;
using IrixiStepperControllerHelper;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Irixi_Aligner_Common.Classes
{
    sealed public class SystemService : ViewModelBase, IDisposable
    {
        #region Variables

        SystemState _state = SystemState.IDLE;
        MessageItem _lastmsg = null;
        MessageHelper _msg_helper = new MessageHelper();
        bool isInitialized = false;

        /// <summary>
        /// lock while set or get this.State
        /// </summary>
        readonly object lockSystemStatus = new object();

        #endregion

        #region Constructor

        public SystemService()
        {
            ThreadPool.SetMinThreads(50, 50);

            // read version from AssemblyInfo.cs
            Version version = Assembly.GetExecutingAssembly().GetName().Version;

            // force to enable the log, otherwise the initial message could not be recored
            LogHelper.LogEnabled = true;

            StringBuilder sb = new StringBuilder();
            sb.Append("\r\n");
            sb.Append("> =================================================================\r\n");
            sb.Append("> =                 4x25G/10x10G Alignment System                 =\r\n");
            sb.Append("> =                    Copyright (C) 2017 Irixi                   =\r\n");
            sb.Append("> =================================================================\r\n");
            LogHelper.WriteLine(sb.ToString());

            this.LastMessage = new MessageItem(MessageType.Normal, "System startup ...");

            this.LastMessage = new MessageItem(MessageType.Normal, "Application Version {0}", version);


            // read the configuration from the file named SystemCfg.json
            // the file is located in \Configuration
            ConfigManager conf_manager = SimpleIoc.Default.GetInstance<ConfigManager>();

            // whether output the log
            LogHelper.LogEnabled = conf_manager.ConfSystemSetting.LogEnabled;

            // initialize the properties
            BusyComponents = new List<IServiceSystem>();

            PhysicalMotionControllerCollection = new Dictionary<Guid, IMotionController>();
            LogicalAxisCollection = new ObservableCollection<LogicalAxis>();
            LogicalMotionComponentCollection = new ObservableCollection<LogicalMotionComponent>();
            MeasurementInstrumentCollection = new ObservableCollection<InstrumentBase>();
            ActiveInstrumentCollection = new ObservableCollection<InstrumentBase>();
            State = SystemState.BUSY;

            SpiralScanArgs = new SpiralScanArgs();
            AlignmentXDArgs = new AlignmentXDArgs();


            /*
             * enumerate all physical motion controllers defined in the config file,
             * and create the instance of the motion controller class.
             */
            foreach (var conf in conf_manager.ConfSystemSetting.PhysicalMotionControllers)
            {
                IMotionController motion_controller = null;

                switch (conf.Model)
                {
                    case MotionControllerModel.LUMINOS_P6A:
                        motion_controller = new LuminosP6A(conf);
                        motion_controller.OnMoveBegin += PhysicalMotionController_OnMoveBegin;
                        motion_controller.OnMoveEnd += PhysicalMotionController_OnMoveEnd;
                        break;

                    case MotionControllerModel.THORLABS_TDC001:
                        //TODO create the instance of thorlabs TDC001
                        break;

                    case MotionControllerModel.IRIXI_EE0017:
                        motion_controller = new IrixiEE0017(conf);
                        motion_controller.OnMoveBegin += PhysicalMotionController_OnMoveBegin;
                        motion_controller.OnMoveEnd += PhysicalMotionController_OnMoveEnd;
                        ((IrixiEE0017)motion_controller).OnMessageReported += ((sender, message) =>
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                this.LastMessage = new MessageItem(MessageType.Normal, string.Format("{0} {1}", sender, message));
                            });
                        });
                        break;

                    default:
                        this.LastMessage = new MessageItem(MessageType.Error, "Unrecognized controller model {0}.", conf.Model);
                        break;
                }

                // Add the controller to the dictionary<Guid, Controller>
                if (motion_controller != null)
                {
                    this.PhysicalMotionControllerCollection.Add(motion_controller.DeviceClass, motion_controller);
                }
            }

            // create the instance of the Logical Motion Components
            foreach (var cfg_motion_comp in conf_manager.ConfSystemSetting.LogicalMotionComponents)
            {
                LogicalMotionComponent comp = new LogicalMotionComponent(cfg_motion_comp.Caption, cfg_motion_comp.Icon);

                int axis_id = 0;
                foreach (var cfg_axis in cfg_motion_comp.LogicalAxisArray)
                {
                    // new logical axis object will be added to the Logical Motion Component
                    LogicalAxis axis = new LogicalAxis(this, cfg_axis, cfg_motion_comp.Caption, axis_id);

                    axis.OnHomeRequsted += LogicalAxis_OnHomeRequsted;
                    axis.OnMoveRequsted += LogicalAxis_OnMoveRequsted;
                    axis.OnStopRequsted += LogicalAxis_OnStopRequsted;

                    // bind the physical axis instance to logical axis object
                    BindPhysicalAxis(axis);

                    comp.LogicalAxisCollection.Add(axis);
                    this.LogicalAxisCollection.Add(axis);

                    axis_id++;
                }

                this.LogicalMotionComponentCollection.Add(comp);
            }

            // create the instance of the cylinder
            try
            {
                IrixiEE0017 ctrl = PhysicalMotionControllerCollection[Guid.Parse(conf_manager.ConfSystemSetting.Cylinder.Port)] as IrixiEE0017;
                CylinderController = new CylinderController(conf_manager.ConfSystemSetting.Cylinder, ctrl);
            }
            catch (Exception e)
            {
                this.LastMessage = new MessageItem(MessageType.Error, "Unable to initialize the cylinder controller, {0}", e.Message);
            }

            // create instance of the keithley 2400
            foreach (var cfg in conf_manager.ConfSystemSetting.Keithley2400s)
            {
                this.MeasurementInstrumentCollection.Add(new Keithley2400(cfg));
            }

            // create instance of the newport 2832C
            foreach (var cfg in conf_manager.ConfSystemSetting.Newport2832Cs)
            {
                this.MeasurementInstrumentCollection.Add(new Newport2832C(cfg));
            }
        }

        #endregion

        #region Events

        void LogicalAxis_OnHomeRequsted(object sender, EventArgs args)
        {
            var s = sender as LogicalAxis;
            Home(s.PhysicalAxisInst);
        }

        void LogicalAxis_OnMoveRequsted(object sender, MoveByDistanceArgs args)
        {
            var s = sender as LogicalAxis;
            MoveLogicalAxis(s, args);
        }

        void LogicalAxis_OnStopRequsted(object sender, EventArgs args)
        {
            var s = sender as LogicalAxis;
            s.PhysicalAxisInst.Stop();
        }

        void PhysicalMotionController_OnMoveBegin(object sender, EventArgs args)
        {
            var obj = (IServiceSystem)sender;
            RegisterBusyComponent(obj);
        }

        void PhysicalMotionController_OnMoveEnd(object sender, EventArgs args)
        {
            var obj = (IServiceSystem)sender;
            DeregisterBusyComponent(obj);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Get or set the list of the busy devices/processes, this list is used to stop the busy devices or processes such as alignment process, user-process, etc.
        /// </summary>
        List<IServiceSystem> BusyComponents { get; }

        /// <summary>
        /// Does the system service have been initialized ?
        /// it's mainly used to set the enabled property of UI elements.
        /// </summary>
        public bool IsInitialized
        {
            private set
            {
                isInitialized = value;
                RaisePropertyChanged();
            }
            get
            {
                return isInitialized;
            }
        }

        /// <summary>
        /// Get the system status
        /// </summary>
        public SystemState State
        {
            private set
            {
                _state = value;
                RaisePropertyChanged();
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
        public ObservableCollection<LogicalAxis> LogicalAxisCollection
        {
            private set;
            get;
        }

        /// <summary>
        /// Get the collection of the logical motion components, this property should be used to generate the motion control panel for each aligner
        /// </summary>
        public ObservableCollection<LogicalMotionComponent> LogicalMotionComponentCollection { get; }

        /// <summary>
        /// Get the collection of instruments that defined in the configuration file
        /// </summary>
        public ObservableCollection<InstrumentBase> MeasurementInstrumentCollection { get; }

        /// <summary>
        /// Get the collection of the active instruments which are initialized successfully, the property should be used to represent the valid instruments in the alignment control panel
        /// </summary>
        public ObservableCollection<InstrumentBase> ActiveInstrumentCollection { get; }

        /// <summary>
        /// Set or get the last message.
        /// this message will be added into this.MessageCollection
        /// </summary>
        public MessageItem LastMessage
        {
            private set
            {
                _lastmsg = value;
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

        /// <summary>
        /// Get argument of Spiral Scan, the properties in the class are binded to the UI
        /// </summary>
        public SpiralScanArgs SpiralScanArgs
        {
            get;
        }

        /// <summary>
        /// Get argument of Alignement-XD, the properties in the class are binded to the UI
        /// </summary>
        public AlignmentXDArgs AlignmentXDArgs
        {
            get;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Add the busy object to the list to stop it
        /// </summary>
        /// <param name="Obj"></param>
        void RegisterBusyComponent(IServiceSystem Obj)
        {
            if (!BusyComponents.Contains(Obj))
                BusyComponents.Add(Obj);
        }

        /// <summary>
        /// Remove the busy object from the list which has been stopped
        /// </summary>
        /// <param name="Obj"></param>
        void DeregisterBusyComponent(IServiceSystem Obj)
        {
            if (BusyComponents.Contains(Obj))
                BusyComponents.Remove(Obj);
        }

        /// <summary>
        /// Bind the physical axis to the logical aligner
        /// </summary>
        /// <param name="ParentAligner">which logical aligner belongs to</param>
        /// <param name="Axis"></param>
        /// <returns></returns>
        bool BindPhysicalAxis(LogicalAxis Axis)
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

        void SetSystemState(SystemState State)
        {
            lock (lockSystemStatus)
            {
                this.State = State;
            }
        }

        SystemState GetSystemState()
        {
            lock (lockSystemStatus)
            {
                return this.State;
            }
        }

        /// <summary>
        /// Start running user program
        /// </summary>
        void Start()
        {
            //TODO here we should judge whether the auto-program is paused or not
            // if the auto-program is not worked and an auto-program has been selected, run it;
            // otherwise, continue to run the last paused auto-program
        }

        /// <summary>
        /// Stop the moving axes or stop running the user program
        /// </summary>
        void Stop()
        {
            List<Task> tasks = new List<Task>();

            foreach (var item in BusyComponents)
            {
                tasks.Add(Task.Run(() =>
                {
                    item.Stop();
                }));
            }

            try
            {
                Task.WaitAll(tasks.ToArray());
            }
            catch (AggregateException ex)
            {
                this.LastMessage = new MessageItem(MessageType.Error, "Error(s) occurred while stopping objects: ");
                foreach (var item in ex.InnerExceptions)
                {
                    this.LastMessage = new MessageItem(MessageType.Error, string.Format("*{0}* >>> {1}", item.Source, ex.Message));
                }
            }
        }

        /// <summary>
        /// Start a specified alignment process asynchronously, all alignment process will be started by this common function
        /// </summary>
        /// <param name="AlignHandler"></param>
        async void StartAlignmentProc(AlignmentBase AlignHandler)
        {
            if (GetSystemState() == SystemState.IDLE)
            {
                SetSystemState(SystemState.BUSY);

                LastMessage = new MessageItem(MessageType.Normal, string.Format("Start running {0}...", AlignHandler));

                // to calculate time costs
                DateTime alignStarts = DateTime.Now;

                try
                {
                    // add alignement class to busy components list
                    BusyComponents.Add(AlignHandler);

                    // pause the auto-fetching process of instrument
                    AlignHandler.Args.PauseInstruments();

                    // run actual alignment process
                    await Task.Run(() =>
                    {
                        AlignHandler.Start();
                    });
                }
                catch (Exception ex)
                {
                    this.LastMessage = new MessageItem(MessageType.Error, string.Format("{0} Error, {1}", AlignHandler, ex.Message));
                    PostErrorMessageToFrontEnd(this.LastMessage.Message);
                }
                finally
                {
                    try
                    {
                        AlignHandler.Args.ResumeInstruments();
                    }
                    catch (Exception ex)
                    {
                        LastMessage = new MessageItem(MessageType.Error, string.Format("Unable to resume auto-fetching process of {0}, {1}", AlignHandler.Args.Instrument, ex.Message));
                        BusyComponents.Remove(AlignHandler);
                    }
                }

                LastMessage = new MessageItem(MessageType.Normal, string.Format("{0} complete, costs {1}s", AlignHandler, (DateTime.Now - alignStarts).TotalSeconds));

                SetSystemState(SystemState.IDLE);
            }
        }


        /// <summary>
        /// Post the error message by Mvvmlight.Messenger class
        /// </summary>
        /// <param name="Message"></param>
        void PostErrorMessageToFrontEnd(string Message)
        {
            Messenger.Default.Send<NotificationMessage<string>>(new NotificationMessage<string>(
                        this,
                        Message,
                        "ERROR"));
        }

        #endregion

        #region Public Methods (The functions are also APIs of the user's programm)

        /// <summary>
        /// Initialize all devices in the system
        /// </summary>
        public async void Init()
        {
            bool[] ret;
            List<Task<bool>> _tasks = new List<Task<bool>>();
            List<IEquipmentBase> _equipments = new List<IEquipmentBase>();

            DateTime initStarts = DateTime.Now;

            SetSystemState(SystemState.BUSY);

            #region Initialize motion controllers

            // initialize all motion controllers simultaneously
            foreach (var _mc in this.PhysicalMotionControllerCollection.Values)
            {
                if (_mc.IsEnabled)
                {
                    _equipments.Add(_mc);
                    _tasks.Add(Task.Run<bool>(() => { return _mc.Init(); }));

                    this.LastMessage = new MessageItem(MessageType.Normal, "{0} Initializing ...", _mc);

                    // update UI immediately
                    await Task.Delay(50);
                }
            }

            while (_tasks.Count > 0)
            {
                // Wait until all init tasks were done
                Task<bool> t = await Task.WhenAny(_tasks);

                int id = _tasks.IndexOf(t);

                if (t.Result)
                {
                    this.LastMessage = new MessageItem(MessageType.Good, "{0} Initialization is completed.", _equipments[id]);
                }
                else
                    this.LastMessage = new MessageItem(MessageType.Error, "{0} Initialization is failed, {1}", _equipments[id], _equipments[id].LastError);

                _tasks.RemoveAt(id);
                _equipments.RemoveAt(id);
            }

            #endregion

            #region Clear the task conllections to inititalize the other equipments

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
            if (CylinderController.IsEnabled)
            {
                _tasks.Add(Task.Factory.StartNew(this.CylinderController.Init));
                _equipments.Add(this.CylinderController);

                this.LastMessage = new MessageItem(MessageType.Normal, "{0} Initializing ...", this.CylinderController);
            }

            // initizlize the keithley 2400
            foreach (var instr in this.MeasurementInstrumentCollection)
            {
                if (instr.IsEnabled)
                {
                    _tasks.Add(Task.Factory.StartNew(instr.Init));
                    _equipments.Add(instr);

                    this.LastMessage = new MessageItem(MessageType.Normal, "{0} Initializing ...", instr);
                }
            }

            while (_tasks.Count > 0)
            {
                Task<bool> t = await Task.WhenAny(_tasks);
                
                int ended_id = _tasks.IndexOf(t);


                if (t.Result)
                {
                    this.LastMessage = new MessageItem(MessageType.Good, "{0} Initialization is completed.", _equipments[ended_id]);

                    // add the instruments which are initialized successfully to the acitve collection
                    if (_equipments[ended_id] is InstrumentBase)
                        ActiveInstrumentCollection.Add((InstrumentBase)_equipments[ended_id]);
                }
                else
                    this.LastMessage = new MessageItem(MessageType.Error, "{0} Initialization is failed, {1}", _equipments[ended_id], _equipments[ended_id].LastError);

                _tasks.RemoveAt(ended_id);
                _equipments.RemoveAt(ended_id);
            }
            #endregion

            SetSystemState(SystemState.IDLE);

            this.LastMessage = new MessageItem(MessageType.Normal,
                string.Format("System Initialization is finished, costs {0:F2}s", (DateTime.Now - initStarts).TotalSeconds));

        }

        /// <summary>
        /// Move the specified axis with specified args
        /// </summary>
        /// <param name="Axis"></param>
        /// <param name="Args"></param>
        public async void MoveLogicalAxis(LogicalAxis Axis, MoveByDistanceArgs Args)
        {
            if (GetSystemState() != SystemState.BUSY)
            {
                SetSystemState(SystemState.BUSY);

                this.LastMessage = new MessageItem(MessageType.Normal, "{0} Move with argument {1}{2} ...",
                    Axis,
                    Args,
                    Axis.PhysicalAxisInst.UnitHelper.Unit);

                var t = new Task<bool>(() =>
                {
                    return Axis.PhysicalAxisInst.Move(Args.Mode, Args.Speed, Args.Distance);
                });
                t.Start();
                bool ret = await t;
                if (ret == false)
                {
                    this.LastMessage = new MessageItem(MessageType.Error, "{0} Unable to move, {1}", Axis, Axis.PhysicalAxisInst.LastError);

                    PostErrorMessageToFrontEnd(this.LastMessage.Message);
                }
                else
                {
                    this.LastMessage = new MessageItem(MessageType.Normal, "{0} Move is completed, the final position is {1}/{2}{3}",
                        new object[]
                        {
                            Axis,
                            Axis.PhysicalAxisInst.AbsPosition,
                            Axis.PhysicalAxisInst.UnitHelper.AbsPosition,
                            Axis.PhysicalAxisInst.UnitHelper.Unit
                        });
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
        public async void MassMoveLogicalAxis(Tuple<int, LogicalAxis, MoveByDistanceArgs>[] AxesGroup)
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
                            var t = new Task<bool>(() =>
                            {
                                return _axis.PhysicalAxisInst.Move(_arg.Mode, _arg.Speed, _arg.Distance);
                            });
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

                            if (t.Result)
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
            if (GetSystemState() == SystemState.IDLE)
            {
                SetSystemState(SystemState.BUSY);
                bool ret = await Task.Run<bool>(() => Axis.Home());
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

                            var t = new Task<bool>(() =>
                            {
                                return axis.PhysicalAxisInst.Home();
                            });
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
                            _homed_cnt++;
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

        public void DoAlignmentXD(AlignmentXDArgs Args)
        {
            StartAlignmentProc(new AlignmentXD(Args));
        }

        public void DoBlindSearch(SpiralScanArgs Args)
        {
            StartAlignmentProc(new SpiralScan(Args));
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

        public void Dispose()
        {
            // dispose motion controllers
            foreach (var ctrl in this.PhysicalMotionControllerCollection)
            {
                ctrl.Value.Dispose();
            }

            // dispose keithley2400s
            foreach (var k2400 in this.MeasurementInstrumentCollection)
            {
                k2400.Dispose();
            }
        }

        #endregion
        
        #region ICommands

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

        public RelayCommand<AlignmentXDArgs> CommandDoAlignmentXD
        {
            get
            {
                return new RelayCommand<AlignmentXDArgs>(args =>
                {
                    DoAlignmentXD(args);
                });
            }
        }

        public RelayCommand<SpiralScanArgs> CommandDoBlindSearch
        {
            get
            {
                return new RelayCommand<SpiralScanArgs>(args =>
                {
                    DoBlindSearch(args);
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

    }
}
