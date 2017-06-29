using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using IrixiStepperControllerHelper;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace StepperControllerDebuger.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase, IDisposable
    {

        IrixiStepperControllerHelper.IrixiMotionController _controller;

        string _conn_prog_msg = "";

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {


            if (IsInDesignMode)
            {
                //Code runs in Blend-- > create design time data.
            }
            else
            {
                
                _controller = new IrixiStepperControllerHelper.IrixiMotionController(GlobalVariables.HidSN); // For debug, the default SN of the controller is used.

                //
                // While scanning the controller, report the state to user window
                //
                _controller.OnConnectionStatusChanged += new EventHandler<IrixiStepperControllerHelper.ConnectionEventArgs>((sender, args) =>
                {
                    switch(args.Event)
                    {
                        case ConnectionEventArgs.EventType.ConnectionRetried: // how many times tried to connect to the device is reported
                            this.ConnectionProgressMessage = string.Format("Searching device...tried {0} times", (int)args.Content);
                            break;

                        case ConnectionEventArgs.EventType.ConnectionLost:
                            StartOpenDevice();
                            break;
                    }
                    
                });

                _controller.OnInputChanged += new EventHandler<InputEventArgs>((s, e) =>
                {
                    if(e.Channel == 0 && e.State == InputState.Triggered)
                    {
                        _controller.SetGeneralOutput(0, OutputState.Enabled);
                    }
                });
                

                //
                // Once the report is received, update the UI components.
                //
                _controller.OnReportUpdated += new System.EventHandler<IrixiStepperControllerHelper.DeviceStateReport>((sender, report) =>
                {
                    //
                    // Nothing to do in this demo
                    // 
                    //
                });

                //_controller.OpenDeviceAsync();
                StartOpenDevice();
            }
        }

        async void StartOpenDevice()
        {
            bool success = await _controller.OpenDeviceAsync();
            this.ConnectionProgressMessage = "Connected";
        }

        #region Properties
        /// <summary>
        /// Get the message about the connection progress
        /// </summary>
        public string ConnectionProgressMessage
        {
            private set
            {
                _conn_prog_msg = value;
                RaisePropertyChanged("ConnectionProgressMessage");
            }
            get
            {
                return _conn_prog_msg;
            }
        }

        /// <summary>
        /// Get the instance of the stepper controller class
        /// </summary>
        public IrixiStepperControllerHelper.IrixiMotionController StepperController
        {
            get
            {
                return _controller;
            }
        }

        #endregion

        #region Commands

        #region Explain of null Command argument
        //!         While the ¡®commnd argument' incoming is null
        // ref to the command struct generator class, while the format of the input values are illegal,
        // for example, the distance value is not a integer, a null command object will be returned in 
        // the converter.
        //
        // As a matter of the fact, the input values should be validated in XAML level.
        //TODO: Validate the input values in xaml
        //
        #endregion

        /// <summary>
        /// Home all axis
        /// </summary>
        public RelayCommand<int> CommandHome
        {
            get
            {
                return new RelayCommand<int>(axisid =>
                {
                    if (axisid == -1)
                        StartHomeAsync();
                    else
                        StartHomeAsync(axisid);
                });
            }
        }

        /// <summary>
        /// Home all axes
        /// </summary>
        async void StartHomeAsync()
        {
            Task<bool>[] tasks = new Task<bool>[_controller.TotalAxes];
            bool[] retvals = new bool[_controller.TotalAxes];

            for (int i = 0; i < _controller.TotalAxes; i ++)
            {
                tasks[i] = _controller.HomeAsync(i);
                //await Task.Delay(10);
            }

            retvals = await Task.WhenAll<bool>(tasks);

        }

        /// <summary>
        /// Home the specified axis
        /// </summary>
        /// <param name="AxisIndex"></param>
        async void StartHomeAsync(int AxisIndex)
        {
            bool success = await _controller.HomeAsync(AxisIndex);
            if (success)
            {
                success = await _controller.MoveAsync(
                    AxisIndex,
                    100,
                    _controller.AxisCollection[AxisIndex].PosAfterHome,
                    IrixiStepperControllerHelper.MoveMode.ABS);

                if (!success)
                {
                    Messenger.Default.Send<NotificationMessage<string>>(
                        new NotificationMessage<string>(string.Format("Unable to move to the initial position, {0}", _controller.LastError),
                                                        "Error"));
                }
            }
            else
            {
                Messenger.Default.Send<NotificationMessage<string>>(
                            new NotificationMessage<string>(
                                string.Format("Unable to home, {0}", _controller.LastError),
                                "Error"));
            }
        }

        /// <summary>
        /// Move the specified axis
        /// </summary>
        public RelayCommand<CommandStruct> CommandMove
        {
            get
            {
                return new RelayCommand<IrixiStepperControllerHelper.CommandStruct>(args =>
                {
                    // about the null, ref to the explain above
                    if (args == null)
                    {
                        Messenger.Default.Send<NotificationMessage<string>>(
                                new NotificationMessage<string>(
                                    string.Format("The move parameter could not be null."),
                                    "Error"));
                    }
                    else if (args.Command == IrixiStepperControllerHelper.EnumCommand.MOVE)
                    {
                        StartMoveAsync(args);

                    }
                    else
                    {
                        // Got the wrong command
                        Messenger.Default.Send<NotificationMessage<string>>(new NotificationMessage<string>("The command flag is not Move.", "Error"));
                    }
                });
            }
        }

        async void StartMoveAsync(CommandStruct args)
        {
            bool success = await _controller.MoveAsync(args.AxisIndex, args.DriveVelocity, args.TotalSteps, args.Mode);
            if(!success)
            {
                /*
                Messenger.Default.Send<NotificationMessage<string>>(
                                                new NotificationMessage<string>(
                                                    string.Format("Unable to move, {0}", _controller.LastError),
                                                    "Error"));

    */
            }
        }

        /// <summary>
        /// Stop the specified axis
        /// </summary>
        public RelayCommand<int> CommandStop
        {
            get
            {
                return new RelayCommand<int>(axisid =>
                {

                    bool success = _controller.Stop(axisid);
                    if (!success)
                    {
                        Messenger.Default.Send<NotificationMessage<string>>(
                            new NotificationMessage<string>(
                                string.Format("Unable to stop, {0}", _controller.LastError),
                                "Error"));
                    }

                });
            }
        }

        public RelayCommand<Tuple<int, OutputState>> CommandSetGeneralOutput
        {
            get
            {
                return new RelayCommand<Tuple<int, OutputState>>(arg =>
                {
                    _controller.SetGeneralOutput(arg.Item1, arg.Item2);
                });
            }
        }

        #endregion


        public void Dispose()
        {
            _controller.Dispose();
        }
    }
}