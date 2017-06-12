using Irixi_Aligner_Common.Configuration;
using Irixi_Aligner_Common.Interfaces;
using IrixiStepperControllerHelper;
using System;
using System.Threading.Tasks;

namespace Irixi_Aligner_Common.MotionControllerEntities
{
    public class IrixiEE0017 : MotionControllerBase<IrixiAxis>
    {
        #region Variables
        /// <summary>
        /// Raise when some messages are generated and pass the messages to the subscriber
        /// </summary>
        public EventHandler<string> OnNewMessage;

        /// <summary>
        /// The instance of the IrixiMotionController class
        /// </summary>
        IrixiMotionController _controller;

        #endregion

        #region Constructors

        public IrixiEE0017(ConfigPhysicalMotionController Config)
            : base(Config)
        {
            _controller = new IrixiMotionController(Config.Port);
            _controller.OnConnectionStatusChanged += _controller_OnConnectionProgressChanged;
            _controller.OnReportUpdated += _controller_OnReportUpdated;
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// The event will be raised after the status of the controller is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _controller_OnConnectionProgressChanged(object sender, ConnectionEventArgs e)
        {
            switch (e.Event)
            {
                case ConnectionEventArgs.EventType.ConnectionSuccess:
                    OnNewMessage?.Invoke(this, "Connected!");
                    break;

                case ConnectionEventArgs.EventType.TotalAxesReturned:
                    OnNewMessage?.Invoke(this, string.Format("The total of axes is {0}", e.Content));
                    this.IsInitialized = true;
                    break;

                case ConnectionEventArgs.EventType.ConnectionLost:
                    OnNewMessage?.Invoke(this, "Connection was lost, retry ...");
                    this.IsInitialized = false;

                    // Start to reconnect the controller
                    Init().Start();
                    break;

            }
        }

        /// <summary>
        /// The event will be raised after a HID report is received
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">The HID Report</param>
        private void _controller_OnReportUpdated(object sender, DeviceStateReport e)
        {
            foreach (var state in e.AxisStateCollection)
            {
                IrixiAxis _axis = FindAxisByName(state.AxisIndex.ToString()) as IrixiAxis;
                _axis.AbsPosition = state.AbsPosition;
                _axis.IsHomed = state.IsHomed;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Initialize the motion controller
        /// </summary>
        /// <returns></returns>
        public override Task<bool> Init()
        {
            // implement the common process
            using (var t = base.Init())
            {
                t.Start();
                t.Wait();
                if (t.Result == false)
                    return new Task<bool>(() =>
                    {
                        return false;
                    });
            }

            // The OpenDeviceAsync process will be always running until the device was found
            return new Task<bool>(() =>
            {
                _controller.OpenDeviceAsync();
                this.LastError = "NOT a real error! The initialization is running in the background thread ...";
                // return immediately
                return false;
            });
            
        }

        /// <summary>
        /// Home the specified axis
        /// </summary>
        /// <param name="Axis">The instance of the IrixiAxis class</param>
        /// <returns></returns>
        public override Task<bool> Home(IAxis Axis)
        {
            return new Task<bool>(() =>
            {
                bool ret = false;

                // implement the common process
                using (var t = base.Home(Axis))
                {
                    t.Start();
                    t.Wait();
                    if (t.Result == false)
                        return false;
                }

                IrixiAxis _axis = Axis as IrixiAxis;

                // lock the axis
                if (_axis.Lock())
                {
                    this.IncreaseRunningAxes();

                    try
                    {
                        // start homing
                        ret = _controller.HomeAsync(_axis.AxisIndex).Result;

                        if (ret)
                        {
                            // set the rel position to 0 after homing
                            _axis.ClearRelPosition();

                            ret = true;
                        }
                        else
                        {
                            _axis.LastError = string.Format("sdk reported error {0}", _controller.LastError);

                            ret = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        _axis.LastError = string.Format("{0}", ex.Message);
                        ret = false;
                    }
                    finally
                    {
                        this.DecreaseRunningAxes();

                        // release the axis
                        _axis.Unlock();
                    }
                }
                else
                {
                    _axis.LastError = "axis is busy";
                    ret = false;
                }

                return ret;
            });
        }

        /// <summary>
        /// Move the specified axis with specified parameters
        /// </summary>
        /// <param name="Axis">The instance of the IrixiAxis class</param>
        /// <param name="Mode">Abs or Rel</param>
        /// <param name="Speed">The velocity to move</param>
        /// <param name="Distance">The distance to move</param>
        /// <returns></returns>
        public override Task<bool> Move(IAxis Axis, MoveMode Mode, int Speed, int Distance)
        {
            return new Task<bool>(() =>
            {
                // implement the common process
                using (var t = base.Move(Axis, Mode, Speed, Distance))
                {
                    t.Start();
                    t.Wait();
                    if (t.Result == false)
                        return false;
                }

                IrixiAxis _axis = Axis as IrixiAxis;

                bool ret = false;
                int target_pos = 0;

                if (_axis.IsHomed == false)
                {
                    _axis.LastError = "the axis is not homed";
                    return false;
                }

                // lock the axis
                if (_axis.Lock())
                {
                    this.IncreaseRunningAxes();

                    try
                    {

                        // Set the move speed
                        if (Mode == MoveMode.ABS)
                        {
                            target_pos = Math.Abs(Distance);
                        }
                        else
                        {
                            target_pos = _axis.AbsPosition + Distance;
                        }

                        // Move the the target position
                        if (_axis.CheckSoftLimitation(target_pos))
                        {
                            ret = _controller.MoveAsync(_axis.AxisIndex, Speed, target_pos, EnumMoveMode.ABS).Result;

                            if(!ret)
                            {
                                _axis.LastError = string.Format("sdk reported error code {0}", _controller.LastError);

                                ret = false;
                            }
                        }
                        else
                        {
                            _axis.LastError = "target position exceeds the limitation.";

                            ret = false;
                        }

                    }
                    catch (Exception ex)
                    {
                        _axis.LastError = ex.Message;
                        ret = false;
                    }

                    finally
                    {
                        this.DecreaseRunningAxes();

                        // release the axis
                        _axis.Unlock();
                    }

                }
                return ret;

            });
        }
        #endregion
    }
}
