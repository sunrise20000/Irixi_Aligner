﻿using Irixi_Aligner_Common.Configuration.MotionController;
using Irixi_Aligner_Common.Interfaces;
using Irixi_Aligner_Common.Message;
using Irixi_Aligner_Common.MotionControllers.Base;
using IrixiStepperControllerHelper;
using System;

namespace Irixi_Aligner_Common.MotionControllers.Irixi
{
    public class IrixiEE0017 : MotionControllerBase<IrixiAxis>
    {
        #region Variables
        /// <summary>
        /// Report the messages when the state of the controller changed such as device connected/disconnected
        /// </summary>
        public event EventHandler<string> OnMessageReported;

        /// <summary>
        /// Raise the event while the input state has changed
        /// </summary>
        public event EventHandler<InputEventArgs> OnInputStateChanged;


        /// <summary>
        /// Raise the event while the hid report has been received
        /// </summary>
        public event EventHandler<DeviceStateReport> OnHIDReportReceived;

        /// <summary>
        /// The instance of the IrixiMotionController class
        /// </summary>
        IrixiMotionController _controller;

        #endregion

        #region Constructors

        public IrixiEE0017(ConfigPhysicalMotionController Config) : base(Config)
        {
            _controller = new IrixiMotionController(Config.Port);
            _controller.OnConnectionStatusChanged += _controller_OnConnectionProgressChanged;
            _controller.OnReportUpdated += _controller_OnReportUpdated;
            _controller.OnInputIOStatusChanged += ((s, e) =>
            {
                OnInputStateChanged?.Invoke(this, e);
            });
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Raise the event while the connection state is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _controller_OnConnectionProgressChanged(object sender, ConnectionEventArgs e)
        {
            switch (e.Event)
            {
                case ConnectionEventArgs.EventType.ConnectionSuccess:
                    OnMessageReported?.Invoke(this, "Connected!");
                    break;

                case ConnectionEventArgs.EventType.TotalAxesReturned:
                    OnMessageReported?.Invoke(this, string.Format("The total of axes is {0}", e.Content));
                    this.IsInitialized = true;
                    break;

                case ConnectionEventArgs.EventType.ConnectionLost:
                    OnMessageReported?.Invoke(this, "Connection was lost, retry ...");
                    this.IsInitialized = false;

                    // Start to connect the controller repeatly
                    // Init();
                    break;

            }
        }

        /// <summary>
        /// Raise the event while the hid report is received
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

            OnHIDReportReceived?.Invoke(this, e);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Initialize the motion controller
        /// </summary>
        /// <returns></returns>
        protected override bool CustomInitProcess()
        {
            _controller.OpenDevice();
            if (_controller.IsConnected)
            {
                if (_controller.ReadFWInfo())
                {
                    LogHelper.WriteLine("{0}, firmware version {1}", this, _controller.FirmwareInfo);

                    // pass the configurations to the instance of irixi motion controller class
                    for (int i = 0; i < this.AxisCollection.Count; i++)
                    {
                        var axis = this.AxisCollection[i.ToString()] as IrixiAxis;
                        var axis_of_sdk = _controller.AxisCollection[i];

                        axis_of_sdk.SoftCCWLS = 0;
                        axis_of_sdk.SoftCWLS = axis.UnitHelper.MaxSteps;
                        axis_of_sdk.MaxSteps = axis.UnitHelper.MaxSteps;
                        axis_of_sdk.MaxSpeed = axis.MaxSpeed;
                        axis_of_sdk.AccelerationSteps = axis.AccelerationSteps;

                        // reverse the drive directoin
                        if (axis.ReverseDriveDirecton)
                            _controller.ReverseMoveDirection(i, true);
                    }

                    return true;
                }
                else
                {

                    LogHelper.WriteLine("{0}, unable to read the firmware info, {1}", this, _controller.LastError);
                    return false;
                }


            }
            else
            {
                this.LastError = _controller.LastError;
                return false;
            }
        }

        protected override bool CustomHomeProcess(IAxis Axis)
        {
                bool ret = false;
                IrixiAxis _axis = Axis as IrixiAxis;

                // lock the axis
                if (_axis.Lock())
                {
                    try
                    {
                        // start homing
                        ret = _controller.Home(_axis.AxisIndex);

                        if (ret)
                        {
                            // set the rel position to 0 after homing
                            _axis.ClearRelPosition();

                            ret = true;
                        }
                        else
                        {
                            _axis.LastError = string.Format("sdk reported error, {0}", _controller.LastError);
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
            }

        /// <summary>
        /// Move the specified axis with specified parameters
        /// </summary>
        /// <param name="Axis">The instance of the IrixiAxis class</param>
        /// <param name="Mode">Abs or Rel</param>
        /// <param name="Speed">The velocity to move</param>
        /// <param name="Distance">The distance to move</param>
        /// <returns></returns>
        protected override bool CustomMoveProcess(IAxis Axis, MoveMode Mode, int Speed, int Distance)
        {
            bool ret = false;
            IrixiAxis _axis = Axis as IrixiAxis;
            
            int target_pos = 0;

            if (_axis.IsHomed == false)
            {
                _axis.LastError = "the axis is not homed";
                return false;
            }

            // lock the axis
            if (_axis.Lock())
            {
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
                        ret = _controller.Move(_axis.AxisIndex, Speed, target_pos, IrixiStepperControllerHelper.MoveMode.ABS);

                        if (!ret)
                        {
                            if (_controller.LastError.EndsWith("31"))
                            {
                                // ignore the error 31 which indicates that uesr interrupted the movement
                                ret = true;
                            }
                            else
                            {
                                _axis.LastError = string.Format("sdk reported error code {0}", _controller.LastError);
                                ret = false;
                            }
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
                    // release the axis
                    _axis.Unlock();
                }

            }
            return ret;
        }

        public override void Stop()
        {
            if (this.IsInitialized)
                _controller.Stop(-1);
        }

        /// <summary>
        /// Toggle the specified output
        /// </summary>
        /// <param name="Channel"></param>
        public void ToggleOutput(int Channel)
        {
            _controller.ToggleGeneralOutput(Channel);
        }

        /// <summary>
        /// Set the state of the specified output
        /// </summary>
        /// <param name="Channel"></param>
        /// <param name="State"></param>
        public void SetOutput(int Channel, OutputState State)
        {
            _controller.SetGeneralOutput(Channel, State);
        }

        public override void Dispose()
        {
            _controller.CloseDevice();
            _controller = null;
        }
        #endregion
    }
}
