using Irixi_Aligner_Common.Configuration;
using Irixi_Aligner_Common.Message;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using Zaber;

namespace Irixi_Aligner_Common.MotionControllerEntities
{
    public class LuminosP6A : MotionControllerBase<LuminosAxis>
    {
        #region Variables
        //Positioner pos = new Positioner();
        ZaberPortFacade _zaber_port_facade = null;
        ConversationCollection _zaber_conversation_collection = null;

        //int _axes_registered = 0;

        /// <summary>
        /// Wait until all axes were registered.
        /// After a axis is found, the SDK raises the 'OnAxisCreated' event; If there are 6 axes,
        /// this event should be raised 6 times.
        /// </summary>
        //SemaphoreSlim _wait_axis_register;
        #endregion

        #region Constructor
        public LuminosP6A(ConfigPhysicalMotionController Config):base(Config)
        {
            //_wait_axis_register = new SemaphoreSlim(0);

            if (Config.Enabled)
            {

                _zaber_port_facade = new ZaberPortFacade()
                {
                    DefaultDeviceType = new DeviceType() { Commands = new List<CommandInfo>() },
                    Port = new TSeriesPort(new SerialPort(), new PacketConverter()),
                    QueryTimeout = 1000
                };

                ZaberDevice allDevices = _zaber_port_facade.GetDevice(0);

                allDevices.MessageReceived += ((s, e) =>
                {
                // if the command relative to position is received, flush the AbsPosition of axis 
                int device = e.DeviceMessage.DeviceNumber;
                    LuminosAxis _axis = FindAxisByName(device.ToString()) as LuminosAxis;

                    if (_axis != null)
                    {
                        switch (e.DeviceMessage.Command)
                        {
                            case Command.Home:
                            case Command.ManualMove:
                            case Command.ManualMoveTracking:
                            case Command.MoveAbsolute:
                            case Command.MoveToStoredPosition:
                            case Command.MoveTracking:
                            case Command.LimitActive:
                            case Command.SlipTracking:
                            case Command.UnexpectedPosition:
                            case Command.MoveRelative:
                                _axis.AbsPosition = e.DeviceMessage.Data;
                                break;

                        }
                    }

                });

                allDevices.MessageSent += ((s, e) =>
                {

                });
            }

            //pos.OnAxisCreated += Pos_OnAxisCreated;
        }

        #endregion

        #region Events of luminos sdk

        /// <summary>
        /// Fires each time an Axis object is added to the Axes collection, usually fired during the execution of the Connect method. 
        /// </summary>
        /// <param name="Unit">The index value of the new Axis that has been created and added to the Axes collection. The first Axis created will have a unit/index value of 1. </param>
        private void Pos_OnAxisCreated(byte Unit)
        {
            //PositionerLib.Axis _luminos_axis = pos.Axes[Unit];
            //LuminosAxis _axis = null;

            //_axis = FindAxisByName(_luminos_axis.AxisType.ToString()) as LuminosAxis;

            //if (_axis != null)
            //{
            //    _axis.RegisterLuminosSDKAxis(_luminos_axis);
            //    LogHelper.WriteLine(string.Format("A luminos axis was find, the type is {0}.", _luminos_axis.AxisType));
            //}
            //else
            //    LogHelper.WriteLine(string.Format("There is no definition of the luminos type {0}, please check the config file.", _luminos_axis.AxisType));

            //_axes_registered++;
            //// if all axes have been registered, complete the inititalization task
            //if (_axes_registered >= pos.Axes.Count)
            //{
            //    _wait_axis_register.Release();
            //}
        }
        
        #endregion

        #region Methods
        public override Task<bool> Init()
        {
            return new Task<bool>(() => 
            {
                // implement the common process
                using (var t = base.Init())
                {
                    t.Start();
                    t.Wait();
                    if (t.Result == false)
                        return false;
                }

                //pos.Port = _config.Port;
                bool _init_ret = true;
                
                try
                {
                    //pos.Connect();

                    _zaber_port_facade.Open(_config.Port);
                    Thread.Sleep(1000);

                    if (_zaber_port_facade.Conversations.Count > 1)
                    {

                        // The axes of the luminos p6a have been found
                        foreach (var conversation in _zaber_port_facade.Conversations)
                        {

                            if (conversation is ConversationCollection) // this conversation with device number 0 is used to control all axis
                            {
                                _zaber_conversation_collection = conversation as ConversationCollection;
                            }
                            else
                            {
                                if (FindAxisByName(conversation.Device.DeviceNumber.ToString()) is LuminosAxis _axis)
                                {

                                    _axis.RegisterZaberConversation(conversation);
                                    LogHelper.WriteLine(string.Format("One luminos axis was find, the id is {0}.", conversation.Device.DeviceNumber));
                                }
                                else
                                {
                                    LogHelper.WriteLine(string.Format("**ERROR** The device number {0} reported by luminos sdk is not defined in the config file.", conversation.Device.DeviceNumber));
                                    _init_ret = false;
                                }
                            }
                        }
                    }
                    else // no axis was found by the sdk
                    {
                        LogHelper.WriteLine(string.Format("**ERROR** No axis was found by the zaber sdk."));
                        _init_ret = false;
                    }


                    if (_init_ret == false)
                        this.LastError = "we encountered some problem while initializing the device, please see the log file for detail information.";
                    else
                        this.IsInitialized = true;

                    return _init_ret;

                    //// wait until all luminos axes are returned by SDK
                    //if (_wait_axis_register.Wait(new TimeSpan(0, 0, 30)))
                    //{
                    //    this.IsInitialized = true;
                    //    return true;
                    //}
                    //else
                    //{
                    //    this.LastError = "Initialization timeout, the SDK did not return expected amount of axes";
                    //    return false;
                    //}
                }
                catch (Exception ex)
                {
                    this.LastError = string.Format("{0}", ex.Message);
                    return false;
                }
            });

        }

        public override Task<bool> Home(Interfaces.IAxis Axis)
        {
            return new Task<bool>(() =>
            {
                // implement the common process
                using (var t = base.Home(Axis))
                {
                    t.Start();
                    t.Wait();
                    if (t.Result == false)
                        return false;
                }


                LuminosAxis _axis = Axis as LuminosAxis;
                bool ret = false;

                //PositionerLib.ResultEnum luminos_ret;

                // lock the axis
                if (_axis.Lock())
                {
                    this.IncreaseRunningAxes();

                    try
                    {
                        _axis.IsHomed = false;
                        DeviceMessage zaber_ret = _axis.ZaberConversation.Request(Command.Home);
                        
                        if (zaber_ret.HasFault == false)
                        {
                            _axis.IsHomed = true;

                            ret = true;
                        }
                        else
                        {
                            _axis.LastError = string.Format("sdk reported error code {0}", zaber_ret.FlagText);

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

        public override Task<bool> HomeAll()
        {
            throw new NotImplementedException();
        }

        public override Task<bool> Move(Interfaces.IAxis Axis, MoveMode Mode, int Speed, int Distance)
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

                LuminosAxis _axis = Axis as LuminosAxis;

                bool ret = false;
                int target_pos = 0;

                if(_axis.IsHomed == false)
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
                            DeviceMessage _zaber_msg = _axis.ZaberConversation.Request(Command.MoveAbsolute, target_pos);
                            //luminos_ret = _axis.LuminosAxisInstance.SetPosition(target_pos, true, out object lastpos);

                            if (_zaber_msg.HasFault == false)
                            {
                                ret = true;
                            }
                            else
                            {
                                _axis.LastError = string.Format("sdk reported error code {0}", "null");

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

        public override void Stop()
        {
            //pos.Axes.StopAll();
            _zaber_conversation_collection.Request(Command.Stop);
        }

        public override void Dispose()
        {
            try
            {
                _zaber_port_facade.Close();
            }
            catch
            {

            }
        }

        #endregion
    }
}
