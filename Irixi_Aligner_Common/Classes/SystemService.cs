using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Irixi_Aligner_Common.Alignment.AlignmentXD;
using Irixi_Aligner_Common.Alignment.BaseClasses;
using Irixi_Aligner_Common.Alignment.CentralAlign;
using Irixi_Aligner_Common.Alignment.Rotating;
using Irixi_Aligner_Common.Alignment.SnakeRouteScan;
using Irixi_Aligner_Common.Alignment.SpiralScan;
using Irixi_Aligner_Common.Classes.BaseClass;
using Irixi_Aligner_Common.Configuration.Common;
using Irixi_Aligner_Common.Equipments.Base;
using Irixi_Aligner_Common.Equipments.BaseClass;
using Irixi_Aligner_Common.Equipments.Equipments;
using Irixi_Aligner_Common.Equipments.Instruments;
using Irixi_Aligner_Common.Interfaces;
using Irixi_Aligner_Common.Message;
using Irixi_Aligner_Common.MotionControllers.Base;
using Irixi_Aligner_Common.MotionControllers.Irixi;
using Irixi_Aligner_Common.MotionControllers.Luminos;
using IrixiStepperControllerHelper;
using System.Threading;
using System.Collections.Specialized;
using System.Windows.Forms;
using System.IO;

namespace Irixi_Aligner_Common.Classes
{
    sealed public class SystemService : ViewModelBase, IDisposable
    {
        #region Variables
        
        MessageItem lastMessage = null;
        SystemState sysState = SystemState.IDLE;

        bool isInitialized = false;
        readonly DateTime initStartTime;

        readonly EquipmentCollection<LogicalAxis> listLogicalAxis;
        readonly EquipmentCollection<LogicalAxis> listLogicalAxisInAligner;
        readonly EquipmentCollection<LogicalMotionComponent> listLogicalMotionComponent;
        readonly EquipmentCollection<LogicalMotionComponent> listLogicalAligner;
        readonly EquipmentCollection<InstrumentBase> listAvaliableInstrument;
        readonly EquipmentCollection<InstrumentBase> listDefinedInstrument;
    
        ScriptState _scriptState = ScriptState.IDLE;
        MessageItem _lastmsg = null;
        MessageHelper _msg_helper = new MessageHelper();
        private AutoResetEvent PauseResetEvent = new AutoResetEvent(true);

        /// <summary>
        /// lock while set or get this.State
        /// </summary>
        readonly object lockSystemStatus = new object();
        readonly object lockSystemOpStatus = new object();
        /// <summary>
        /// Output put initialization messages
        /// </summary>
        public event EventHandler<string> InitProgressChanged;

        public Dictionary<string, Dictionary<string, Dictionary<string, string>>> Enums { set; get; }
        private Task taskMonitor = null;
        private CancellationTokenSource cts = new CancellationTokenSource();
        #endregion

        #region Constructor

        public SystemService()
        {


            initStartTime = DateTime.Now;

            Messenger.Default.Register<string>(this, "ScriptCompileError", msg => System.Windows.Application.Current.Dispatcher.Invoke(() => LastMessage = new MessageItem(MessageType.Error, msg)));
            Messenger.Default.Register<string>(this, "ScriptRunTimeError", msg => System.Windows.Application.Current.Dispatcher.Invoke(() => LastMessage = new MessageItem(MessageType.Error, msg)));
            Messenger.Default.Register<string>(this, "ScriptCompileOk", msg => System.Windows.Application.Current.Dispatcher.Invoke(() => LastMessage = new MessageItem(MessageType.Good, msg)));
            Messenger.Default.Register<string>(this, "ScriptFinish", m => { ScriptState = ScriptState.IDLE; });

            //ThreadPool.SetMinThreads(50, 50);

            // read version from AssemblyInfo.cs
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            SysVersion = version.ToString();

            // force to enable the log, otherwise the initial message could not be recored
            LogHelper.LogEnabled = true;

            StringBuilder sb = new StringBuilder();
            sb.Append("\r\n");
            sb.Append("> =================================================================\r\n");
            sb.Append("> =                 4x25G/10x10G Alignment System                 =\r\n");
            sb.Append("> =                Copyright (C) 2017 - 2018 Irixi                =\r\n");
            sb.Append("> =================================================================\r\n");
            LogHelper.WriteLine(sb.ToString());
            this.LastMessage = new MessageItem(MessageType.Normal, "System startup ...");
            this.LastMessage = new MessageItem(MessageType.Normal, "Application Version {0}", version);


            // read the configuration from the file named SystemCfg.json
            // the file is located in \Configuration
            ConfigManager configMgr = SimpleIoc.Default.GetInstance<ConfigManager>();

            // whether output the log
            LogHelper.LogEnabled = configMgr.ConfSystemSetting.LogEnabled;

            // initialize the properties
            BusyComponents = new List<IServiceSystem>();

            PhysicalMotionControllerCollection = new Dictionary<Guid, IMotionController>();
            //LogicalAxisCollection = new EquipmentCollection<LogicalAxis>();
            //LogicalAxisInAlignerCollection = new EquipmentCollection<LogicalAxis>();
            //LogicalMotionComponentCollection = new EquipmentCollection<LogicalMotionComponent>();
            //LogicalAlignerCollection = new EquipmentCollection<LogicalMotionComponent>();
            //MeasurementInstrumentCollection = new EquipmentCollection<InstrumentBase>();
            //BindingOperations.EnableCollectionSynchronization(MeasurementInstrumentCollection, lockSystemStatus);

            listLogicalAxis = new EquipmentCollection<LogicalAxis>();
            listLogicalAxisInAligner = new EquipmentCollection<LogicalAxis>();
            listLogicalMotionComponent = new EquipmentCollection<LogicalMotionComponent>();
            listLogicalAligner = new EquipmentCollection<LogicalMotionComponent>();
            listAvaliableInstrument = new EquipmentCollection<InstrumentBase>();
            listDefinedInstrument = new EquipmentCollection<InstrumentBase>();

            State = SystemState.BUSY;

            SpiralScanArgs = new SpiralScanArgs(this);
            SnakeRouteScanArgs = new SnakeRouteScanArgs(this);
            AlignmentXDArgs = new AlignmentXDArgs(this);
            RotatingScanArgs = new RotatingScanArgs(this);
            CentralAlignArgs = new CentralAlignArgs(this);
            
            /*
             * enumerate all physical motion controllers defined in the config file,
             * and create the instance of the motion controller class.
             */
            foreach (var conf in configMgr.ConfSystemSetting.PhysicalMotionControllers)
            {
                IMotionController motion_controller = null;

                switch (conf.Model)
                {
                    case MotionControllerType.LUMINOS_P6A:
                        motion_controller = new LuminosP6A(conf);
                        break;

                    case MotionControllerType.THORLABS_TDC001:
                        //TODO create the instance of thorlabs TDC001
                        break;

                    case MotionControllerType.IRIXI_EE0017:
                        motion_controller = new IrixiEE0017(conf);

                        ((IrixiEE0017)motion_controller).OnMessageReported += ((sender, message) =>
                        {
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                this.LastMessage = new MessageItem(MessageType.Normal, string.Format("{0} {1}", sender, message));
                            });
                        });
                        break;

                    default:
                        this.LastMessage = new MessageItem(MessageType.Error, "Unrecognized controller model {0}.", conf.Model);
                        break;
                }

                motion_controller.OnMoveBegin += PhysicalMotionController_OnMoveBegin;
                motion_controller.OnMoveEnd += PhysicalMotionController_OnMoveEnd;

                // Add the controller to the dictionary<Guid, Controller>
                if (motion_controller != null)
                {
                    this.PhysicalMotionControllerCollection.Add(motion_controller.DeviceClass, motion_controller);
                }
            }

            // create the instance of the Logical Motion Components
            foreach (var cfgLMC in configMgr.ConfSystemSetting.LogicalMotionComponents)
            {
                LogicalMotionComponent lmc = new LogicalMotionComponent(cfgLMC.Caption, cfgLMC.Icon, cfgLMC.IsAligner);
               
                foreach (var cfgLogicalAxis in cfgLMC.LogicalAxisArray)
                {
                    // new logical axis object will be added to the Logical Motion Component
                    LogicalAxis axis = new LogicalAxis(cfgLogicalAxis, cfgLMC.Caption);

                    axis.OnHomeRequsted += LogicalAxis_OnHomeRequsted;
                    axis.OnMoveRequsted += LogicalAxis_OnMoveRequsted;
                    axis.OnStopRequsted += LogicalAxis_OnStopRequsted;

                    // bind the physical axis instance to logical axis
                    BindPhysicalAxis(axis);

                    lmc.Add(axis);
                    listLogicalAxis.Add(axis);
                    
                    // if the logical motion controller is aligner, save the 
                    if (lmc.IsAligner)
                        listLogicalAxisInAligner.Add(axis);
                }

                listLogicalMotionComponent.Add(lmc);

                if (lmc.IsAligner)
                    listLogicalAligner.Add(lmc);
            }

            // create the instance of the cylinder controller
            IrixiEE0017 controller = null;
            try
            {
                controller = PhysicalMotionControllerCollection[Guid.Parse(configMgr.ConfSystemSetting.Cylinder.Port)] as IrixiEE0017;
            }
            catch
            {
                this.LastMessage = new MessageItem(MessageType.Error, "Unable to find the attached motion controller as the cylinder controller");
            }

            CylinderController = new CylinderController(configMgr.ConfSystemSetting.Cylinder, controller);

            // create instance of the keithley 2400
            foreach (var cfg in configMgr.ConfSystemSetting.Keithley2400s)
            {

                if(cfg.Enabled)
                    listDefinedInstrument.Add(new Keithley2400(cfg));

            }

            // create instance of the newport 2832C
            foreach (var cfg in configMgr.ConfSystemSetting.Newport2832Cs)
            {
                if (cfg.Enabled)
                    listDefinedInstrument.Add(new Newport2832C(cfg));
            }
            //
        }
        ~SystemService()
        {
            Messenger.Default.Unregister<string>(this, "ScriptCompileError");
            Messenger.Default.Unregister<string>(this, "ScriptRunTimeError");
            Messenger.Default.Unregister<string>(this, "ScriptCompileOk");
            Messenger.Default.Unregister<string>(this, "ScriptFinish");
            foreach (var it in MeasurementInstrumentCollection) //Stop fetch
                ;// it.StopAutoFetching();
        }
        #endregion

        #region Events

        void LogicalAxis_OnHomeRequsted(object sender, EventArgs args)
        {
            var s = sender as LogicalAxis;
            Home(s.PhysicalAxisInst);
        }

        void LogicalAxis_OnMoveRequsted(object sender, AxisMoveArgs args)
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
            var imbusy = (IServiceSystem)sender;
            RegisterBusyComponent(imbusy);
        }

        void PhysicalMotionController_OnMoveEnd(object sender, EventArgs args)
        {
            var imfree = (IServiceSystem)sender;
            UnregisterBusyComponent(imfree);
        }

        #endregion

        #region Properties

        public string SysVersion { get; private set; }

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
                sysState = value;
                RaisePropertyChanged();
            }
            get
            {
                return sysState;
            }
        }
        public ScriptState ScriptState
        {
            set
            {
                _scriptState = value;
                RaisePropertyChanged();
            }
            get
            {
                return _scriptState;
            }
        }
        /// <summary>
        /// Get the instance of the Cylinder Controller Class
        /// </summary>
        public CylinderController CylinderController
        {
            get;
        }

        /// <summary>
        /// Get the instance collection of the Motion Controller Class
        /// </summary>
        public Dictionary<Guid, IMotionController> PhysicalMotionControllerCollection
        {
            get;
        }

        /// <summary>
        /// Get the list of the overall logical axes defined in the system setting file.
        /// this list enable users to operate each axis independently without knowing which physical motion controller it belongs to
        /// </summary>
        public ICollectionView LogicalAxisCollection
        {
            get
            {
                return CollectionViewSource.GetDefaultView(listLogicalAxis);
            }
        }
        /// <summary>
        /// Get all enum infomations,it'll be used in binding
        /// </summary>
        public Dictionary<string, List<KeyValuePair<string, List<KeyValuePair<string, int>>>>> EnumInfos
        {
            get { return GetAllEnumsInfo(); }
        }

        /// <summary>
        /// Get the collection contains the logical axes those belong to the logical aligner
        /// </summary>
        public ICollectionView LogicalAxisInAlignerCollection
        {
            get
            {
                return CollectionViewSource.GetDefaultView(listLogicalAxisInAligner);
            }
        }

        /// <summary>
        /// Get the collection of the logical motion components, this property should be used to generate the motion control panel for each aligner
        /// </summary>
        public ICollectionView LogicalMotionComponentCollection
        {
            get
            {
                return CollectionViewSource.GetDefaultView(listLogicalMotionComponent);
            }
        }

        /// <summary>
        /// Get the logical motion components which is marked as logical aligner
        /// </summary>
        public ICollectionView LogicalAlignerCollection
        {
            get
            {
                return CollectionViewSource.GetDefaultView(listLogicalAligner);
            }
        }

        /// <summary>
        /// Get the view of the collection of the instruments defined in the config file.
        /// </summary>
        public ICollectionView CollectionViewDefinedInstruments
        {
            get
            {
                return CollectionViewSource.GetDefaultView(listDefinedInstrument);
            }
        }

        /// <summary>
        /// Get the view of the collection of the instruments of those initialized successfully.
        /// </summary>
        public ICollectionView MeasurementInstrumentCollection
        {
            get
            {
                return CollectionViewSource.GetDefaultView(listAvaliableInstrument);
            }
        }

        /// <summary>
        /// Set or get the last message.
        /// this message will be added into this.MessageCollection
        /// </summary>
        public MessageItem LastMessage
        {
            set
            {
                lastMessage = value;
                MessageCollection.Add(lastMessage);
            }
            get
            {
                return lastMessage;
            }
        }

        /// <summary>
        /// Get the collection of messages.
        /// </summary>
        public MessageHelper MessageCollection { get; } = new MessageHelper();

        #endregion

        #region Alignment Function Args

        /// <summary>
        /// Get argument of Spiral Scan, the properties in the class are bound to the UI
        /// </summary>
        public SpiralScanArgs SpiralScanArgs
        {
            get;
        }

        /// <summary>
        /// Get argument of Snake Route Scan, the properties in the class are bound to the UI
        /// </summary>
        public SnakeRouteScanArgs SnakeRouteScanArgs
        {
            get;
        }

        /// <summary>
        /// Get argument of Alignement-nD, the properties in the class are bound to the UI
        /// </summary>
        public AlignmentXDArgs AlignmentXDArgs
        {
            get;
        }


        /// <summary>
        /// Get argument of rotating alignment, the properties in the class are bound to the UI
        /// </summary>
        public RotatingScanArgs RotatingScanArgs
        {
            get;
        }

        /// <summary>
        /// Get argument of central alignment, the properties in the class are bound to the UI
        /// </summary>
        public CentralAlignArgs CentralAlignArgs
        {
            get;
        }

        #endregion

        #region Private Methods

        public void SetSystemState(SystemState State)
        {
            lock (lockSystemStatus)
            {
                this.State = State;
            }
        }
        private SystemState GetSystemState()
        {
            lock (lockSystemStatus)
            {
                return this.State;
            }
        }

        private Dictionary<string, List<KeyValuePair<string, List<KeyValuePair<string, int>>>>> GetAllEnumsInfo()
        {
            var dic = ScriptHelpMgr.Instance.GetAllEnuminfo();
            var axisList = new List<KeyValuePair<string, List<KeyValuePair<string, int>>>>();
            var instrumentList = new List<KeyValuePair<string, List<KeyValuePair<string, int>>>>();
            //Axis
            foreach (LogicalAxis axis in LogicalAxisCollection) //Axisname list
            {
                var kp = new KeyValuePair<string, List<KeyValuePair<string, int>>>(axis.ToString().Replace(" ", "").Replace("*", "").Replace("@", "_").ToUpper(), null);
                axisList.Add(kp);
            }
            dic.Add("AXIS", axisList);

            //Instrument
            foreach (var instrument in MeasurementInstrumentCollection)
            {
                
                var kp = new KeyValuePair<string, List<KeyValuePair<string, int>>>(instrument.ToString().Replace(" ", "_").Replace("*","").Replace("@","_").ToUpper(), null);
                instrumentList.Add(kp);
            }
            dic.Add("INST", instrumentList);
            return dic;
        }
        /// <summary>
        /// Indicates which components should be stoped if #STOP button clicked
        /// </summary>
        /// <param name="BusyGuy"></param>
        private void RegisterBusyComponent(IServiceSystem BusyGuy)
        {
            if (!BusyComponents.Contains(BusyGuy))
                BusyComponents.Add(BusyGuy);
        }

        /// <summary>
        /// Remove the busy component
        /// </summary>
        /// <param name="BusyGuy"></param>
        private void UnregisterBusyComponent(IServiceSystem BusyGuy)
        {
            if (BusyComponents.Contains(BusyGuy))
                BusyComponents.Remove(BusyGuy);
        }

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
                Axis.PhysicalAxisInst = this.PhysicalMotionControllerCollection[Axis.Config.DeviceClass].GetAxisByName(Axis.Config.AxisName);

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

        /// <summary>
        /// Start running user program
        /// </summary>
        private void Start()
        {
            //TODO here we should judge whether the auto-program is paused or not
            // if the auto-program is not worked and an auto-program has been selected, run it;
            // otherwise, continue to run the last paused auto-program
            Messenger.Default.Send("", "ScriptStart");
        }

        /// <summary>
        /// Stop the moving axes or stop running the user program
        /// </summary>

        private void Stop()
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

        private async void StartAlignmentProc(AlignmentBase AlignHandler)
        {
            if (GetSystemState() == SystemState.IDLE)
            {
                SetSystemState(SystemState.BUSY);
                LastMessage = new MessageItem(MessageType.Normal, string.Format("Start running {0}...", AlignHandler));

                // to calculate time costs
                DateTime alignStarts = DateTime.Now;

                // add alignement class to busy components list
                BusyComponents.Add(AlignHandler);

                try
                {
                    if (AlignHandler.Args == null)
                    {
                        this.LastMessage = new MessageItem(
                            MessageType.Error, 
                            $"{AlignHandler} Error, the argument can not be null.");

                        PostErrorMessage(this.LastMessage.Message);

                    }
                    else
                    {
                        // validate the parameters
                        AlignHandler.Args.Validate();

                        // pause the auto-fetching process of instrument
                        AlignHandler.Args.PauseInstruments();

                        // run actual alignment process
                        await Task.Run(() =>
                        {
                            AlignHandler.Start();
                        });
                    }
                }
                catch (Exception ex)
                {
                    this.LastMessage = new MessageItem(MessageType.Error, string.Format("{0} Error, {1}", AlignHandler, ex.Message));
                    PostErrorMessage(this.LastMessage.Message);   //????
                }
                finally
                {
                    try
                    {
                        AlignHandler.Args.ResumeInstruments();
                    }
                    catch (Exception ex)
                    {
                        //LastMessage = new MessageItem(MessageType.Error, string.Format("Unable to resume auto-fetching process of {0}, {1}", AlignHandler.Args.Instrument, ex.Message));
                    }
                }

                LastMessage = new MessageItem(MessageType.Normal, string.Format("{0} complete, costs {1}s", AlignHandler, (DateTime.Now - alignStarts).TotalSeconds));

                BusyComponents.Remove(AlignHandler);
                SetSystemState(SystemState.IDLE);
            }
        }

        /// <summary>
        /// Post the error message to the UI
        /// </summary>
        /// <param name="Message"></param>
        private void PostErrorMessage(string Message)
        {
            Messenger.Default.Send<NotificationMessage<string>>(new NotificationMessage<string>(
                        this,
                        Message,
                        "ERROR"));
        }
     

        /// <summary>
        /// Output init messages
        /// </summary>
        /// <param name="Message"></param>
        private void PostInitMessage(string Message)
        {
            InitProgressChanged?.Invoke(this, Message);
        }


 
        #endregion

        #region Public Methods

        /// <summary>
        /// Initialize all devices in the system
        /// </summary>
        public async void Init()
        {
            List<Task<bool>> _tasks = new List<Task<bool>>();
            List<IEquipment> _equipments = new List<IEquipment>();
            
            SetSystemState(SystemState.BUSY);

            #region Initialize motion controllers

            PostInitMessage("Initializing motion controllers ...");

            // initialize all motion controllers simultaneously
            foreach (var controller in this.PhysicalMotionControllerCollection.Values)
            {
                if (controller.IsEnabled)
                {
                    _equipments.Add(controller);
                    _tasks.Add(Task.Run(() => { return controller.Init(); }));

                    this.LastMessage = new MessageItem(MessageType.Normal, "{0} Initializing ...", controller);

                    // prevent UI from halt
                    await Task.Delay(10);
                }
            }

            // wait until all axes are initialized
            // once a task is done, display the message on the screen
            while (_tasks.Count > 0)
            {
                Task<bool> t = await Task.WhenAny(_tasks);
                int id = _tasks.IndexOf(t);
                var controller = _equipments[id];

                if(t.IsFaulted)
                {
                    if (t.Exception != null)
                    {
                        foreach (var ex in t.Exception.Flatten().InnerExceptions)
                        {
                            this.LastMessage = new MessageItem(MessageType.Error, $"{controller} Initialization is failed, {ex.Message}");
                        }
                    }
                    else
                    {
                        this.LastMessage = new MessageItem(MessageType.Error, $"{controller} Initialization is failed, unable to get the inner exceptions.");
                    }
                }
                else if (t.Result)
                    this.LastMessage = new MessageItem(MessageType.Good, "{0} Initialization is completed.", controller);
                else
                    this.LastMessage = new MessageItem(MessageType.Error, "{0} Initialization is failed, {1}", controller, controller.LastError);

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

            PostInitMessage("Initializing cylinder controller ...");

            // initialize the cylinder controller
            _tasks.Add(Task.Factory.StartNew(this.CylinderController.Init));
            _equipments.Add(this.CylinderController);
            this.LastMessage = new MessageItem(MessageType.Normal, "{0} Initializing ...", this.CylinderController);


            PostInitMessage("Initializing measurement instruments ...");

            // initizlize the measurement instruments defined in the config file
            // the instruments initialized successfully will be added to the collection #ActiveInstrumentCollection
            foreach (var device in listDefinedInstrument)
            {
                //_tasks.Add(Task.Factory.StartNew(instr.Init));
                _tasks.Add(Task.Factory.StartNew(device.Init));
                _equipments.Add(device);
                this.LastMessage = new MessageItem(MessageType.Normal, "{0} Initializing ...", device);
            }


            while (_tasks.Count > 0)
            {
                int idFinishedTask = 0;
                Task<bool> t = await Task.WhenAny(_tasks);
                idFinishedTask = _tasks.IndexOf(t);
                var equipment = _equipments[idFinishedTask];


                if (t.IsFaulted)
                {
                    if(t.Exception != null)
                    {
                        foreach(var ex in t.Exception.Flatten().InnerExceptions)
                        {
                            this.LastMessage = new MessageItem(MessageType.Error, $"{equipment} Initialization is failed, {ex.Message}");
                        }
                    }
                    else
                    {
                        this.LastMessage = new MessageItem(MessageType.Error, $"{equipment} Initialization is failed, unable to get the inner exceptions.");
                    }
                }
                else if(t.Result)
                { 
                    this.LastMessage = new MessageItem(MessageType.Good, "{0} Initialization is completed.", equipment);

                    // add the instruments which are initialized successfully to the acitve collection
                    if (_equipments[idFinishedTask] is InstrumentBase)
                    {
                        listAvaliableInstrument.Add((InstrumentBase)_equipments[idFinishedTask]);

                    }
                }
                else
                    this.LastMessage = new MessageItem(MessageType.Error, $"{equipment} Initialization is failed, {equipment.LastError}");


                _tasks.RemoveAt(idFinishedTask);
                _equipments.RemoveAt(idFinishedTask);


            }

            //ActiveInstrumentCollection.DisableNotifications();

            #endregion

            SetSystemState(SystemState.IDLE);

            this.LastMessage = new MessageItem(MessageType.Normal,
                string.Format("The initialization has finished, costs {0:F2}s", (DateTime.Now - initStartTime).TotalSeconds));

        }
        /// <summary>
        /// Just for monitor IO, Axis position and Instrument data etc...
        /// 
        /// </summary>
        public void StartMonitor()
        {
            long i = 0;
            taskMonitor = new Task(() =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    //Read IO

                    //Read AxisPos

                    //Read Instument Data


                    Console.WriteLine(i++);
                    Thread.Sleep(20);
                }
            }, cts.Token);

            //taskMonitor.Start();
        }
        /// <summary>
        /// 
        /// </summary>
        public void StopMonitor()
        {
            cts.Cancel();
            Stop();
            cts = null;
            taskMonitor = null;


        }
        /// <summary>
        /// Move the specified axis with specified args
        /// </summary>
        /// <param name="Axis"></param>
        /// <param name="Args"></param>
        public void Pause()
        {
            PauseResetEvent.Reset();
            ScriptState = ScriptState.PAUSE;
            PauseResetEvent.WaitOne();
        }
        public void Resume()
        {
            PauseResetEvent.Set();
        }
        public async void MoveLogicalAxis(LogicalAxis Axis, AxisMoveArgs Args)
        {
            if (GetSystemState() == SystemState.IDLE)
            {
                SetSystemState(SystemState.BUSY);

                this.LastMessage = new MessageItem(MessageType.Normal, "{0} Move with argument {1} ...", Axis, Args);

                var ret = await Task.Run(() =>
                {
                    return Axis.PhysicalAxisInst.Move(Args.Mode, Args.Speed, Args.Distance);
                });

                if (ret == false)
                {
                    this.LastMessage = new MessageItem(MessageType.Error, "{0} Unable to move, {1}", Axis, Axis.PhysicalAxisInst.LastError);
                    PostErrorMessage(this.LastMessage.Message);
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
        public async void MassMoveLogicalAxis(MassMoveArgs Args)
        {
            /*
             *  Operation Sequence
             *  1. find logical motion component
             *  2. check logical axis
             *  3. get move sequence
             *  4. move
             */

            if (GetSystemState() != SystemState.BUSY)
            {
                SetSystemState(SystemState.BUSY);

                #region Find the LMC by the hash string


                LogicalMotionComponent lmc;
                try
                {
                    lmc = listLogicalMotionComponent.FindItemByHashString(Args.LogicalMotionComponent);
                }
                catch(Exception ex)
                { 
                    this.LastMessage = new MessageItem(MessageType.Error, $"Unable to run the mass-move process, {ex}");
                    return;
                }

                #endregion

                #region Validate arguments and arrange the logical to ready to move

                List<LogicalAxis> axesToMove = new List<LogicalAxis>();

                if (Args.Count != lmc.Count)
                {
                    this.LastMessage = new MessageItem(MessageType.Error, $"The axis count is different between argument and LMC.");
                    return;
                }
                else
                {
                    foreach (var perArg in Args)
                    {
                        LogicalAxis la;
                        try
                        {
                            la = lmc.FindAxisByHashString(perArg.LogicalAxisHashString);

                            if (perArg.IsMoveable)
                            {
                                // Only the moveable axis will be added
                                la.MoveArgsTemp = perArg.Clone() as AxisMoveArgs;
                                axesToMove.Add(la);
                            }
                        }
                        catch(Exception ex)
                        {
                            this.LastMessage = new MessageItem(MessageType.Error, $"Unable to run the mass-move process, {ex}");
                            return;
                        }
                    }
                }
                #endregion

                #region Get move order
                var moveOrder = Args.GetDistinctMoveOrder();
                if (moveOrder == null)
                {
                    this.LastMessage = new MessageItem(MessageType.Error, $"Unable to get the list of the move order");
                    return;
                }

                #endregion


                // generate a list which contains the movement tasks
                // this is used by the Task.WhenAll() function
                List<Task<bool>> movingTasks = new List<Task<bool>>();
                List<LogicalAxis> movingAxes = new List<LogicalAxis>();

                this.LastMessage = new MessageItem(MessageType.Normal, "Mass-Move is running ...");

                foreach (var order in moveOrder)
                {
                    // clear the previous tasks
                    movingTasks.Clear();
                    movingAxes.Clear();

                    var ready2move = axesToMove.Where(a => a.MoveArgsTemp.MoveOrder == order).Select(b => b).ToArray();
                    if (ready2move.Any())
                    {
                        foreach (var la in ready2move)
                        {
                            this.LastMessage = new MessageItem(MessageType.Normal, $"Start to move axis {la} ...");

                            var t = new Task<bool>(() =>
                            {
                                return la.PhysicalAxisInst.Move(
                                    la.MoveArgsTemp.Mode,
                                    la.MoveArgsTemp.Speed,
                                    la.MoveArgsTemp.Distance);
                            });
                            t.Start();
                            movingTasks.Add(t);
                            movingAxes.Add(la);
                        }

                        // if no axes to be moved, move to the next loop
                        if (movingTasks.Count > 0)
                        {
                            while (movingTasks.Count > 0)
                            {
                                // wait until all the axes are moved
                                Task<bool> t = await Task.WhenAny(movingTasks);
                                int id = movingTasks.IndexOf(t);

                                if (t.Result)
                                    this.LastMessage = new MessageItem(MessageType.Normal, $"{movingAxes[id]} has moved to the position.");
                                else
                                    this.LastMessage = new MessageItem(MessageType.Error, $"{movingAxes[id]} was moved incorrectly, {movingAxes[id].PhysicalAxisInst.LastError}");

                                movingTasks.RemoveAt(id);
                                movingAxes.RemoveAt(id);
                            }
                        }
                    }
                }

            }
            this.LastMessage = new MessageItem(MessageType.Good, "Mass-Move Done.");
            SetSystemState(SystemState.IDLE);
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
                int _total_axis = listLogicalAxis.Count;
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
                    foreach (var axis in listLogicalAxis)
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
                            await Task.Delay(200);

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
        public void ToggleAxisMoveMode(IAxis Axis)  //切换运动方式
        {
            if (GetSystemState() == SystemState.IDLE)
            {
                SetSystemState(SystemState.BUSY);
                Axis.ToggleMoveMode();
                SetSystemState(SystemState.IDLE);
            }
        }

        /// <summary>
        /// Find the instrument by its hash string 
        /// </summary>
        /// <param name="hashstring"></param>
        /// <returns></returns>
        public IInstrument FindInstrumentByHashString(string hashstring)
        {
            return listAvaliableInstrument.FindItemByHashString(hashstring);
        }

        /// <summary>
        /// Find the logical motion component by its hash string
        /// </summary>
        /// <param name="hashstring"></param>
        /// <returns></returns>
        public LogicalMotionComponent FindLogicalMotionComponentByHashString(string hashstring)
        {
            return listLogicalMotionComponent.FindItemByHashString(hashstring);
        }

        /// <summary>
        /// Find the logical axis by its hash string
        /// </summary>
        /// <param name="hashstring"></param>
        /// <returns></returns>
        public LogicalAxis FindLogicalAxisByHashString(string hashstring)

        {
            return listLogicalAxis.FindItemByHashString(hashstring);
        }

        public void DoAlignmentXD(AlignmentXDArgs Args)
        {
            StartAlignmentProc(new AlignmentXD(Args));
        }

        public void DoBlindSearch(SpiralScanArgs Args)
        {
            StartAlignmentProc(new SpiralScan(Args));
        }

        public void DoSnakeRouteScan(SnakeRouteScanArgs Args)
        {
            StartAlignmentProc(new SnakeRouteScan(Args));
        }

        public void DoRotatingScan(RotatingScanArgs Args)
        {
            StartAlignmentProc(new RotatingScan(Args));
        }

        public void DoCentralAlign(CentralAlignArgs Args)
        {
            StartAlignmentProc(new CentralAlign(Args));
        }

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

        public void ToggleFiberClampState() //气缸
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

        public void TogglePlcVacuumState()  //切换PLC真空状态
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

        public void Dispose()
        {
            // dispose motion controllers
            foreach (var ctrl in this.PhysicalMotionControllerCollection)
            {
                ctrl.Value.Dispose();
            }

            // dispose keithley2400s
            foreach (var k2400 in listAvaliableInstrument)
            {
                k2400.Dispose();
            }
        }
        public void SetLastMessage(MessageType msgType, string strMsgInfo)
        {
            this.LastMessage = new MessageItem(msgType, strMsgInfo);
        }

        public void StopAll()
        {
            Stop();
            foreach (var instr in this.MeasurementInstrumentCollection)
                ;//instr.StopAutoFetching();
            

        }
        public void PopWindow(string windowName)
        {
            Messenger.Default.Send<NotificationMessage<string>>(new NotificationMessage<string>(
                        this,
                        windowName,
                        "POSTWINDOW"));
        }
        #endregion

        #region Commands

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
                    DialogService.DialogService ds = new DialogService.DialogService();
                    ds.OpenHomeConfirmDialog(null, ret =>
                    {
                        if (ret == true)
                        {
                            MassHome();
                        }
                    });


                });
            }
        }

        public RelayCommand<MassMoveArgs> CommandMassMove
        {
            get
            {
                return new RelayCommand<MassMoveArgs>(args =>
                {
                    MassMoveLogicalAxis(args);
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

        public RelayCommand<SnakeRouteScanArgs> CommandDoSnakeRouteScan
        {
            get
            {
                return new RelayCommand<SnakeRouteScanArgs>(args =>
                {
                    DoSnakeRouteScan(args);
                });
            }
        }

        public RelayCommand<RotatingScanArgs> CommandDoRotatingScan
        {
            get
            {
                return new RelayCommand<RotatingScanArgs>(args =>
                {
                    DoRotatingScan(args);
                });
            }
        }

        public RelayCommand<CentralAlignArgs> CommandDoCentralAlign
        {
            get
            {
                return new RelayCommand<CentralAlignArgs>(args =>
                {
                    DoCentralAlign(args);
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
