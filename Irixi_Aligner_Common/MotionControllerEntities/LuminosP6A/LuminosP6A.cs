using Irixi_Aligner_Common.Configuration;
using Irixi_Aligner_Common.Message;
using PositionerLib;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Irixi_Aligner_Common.MotionControllerEntities
{
    public class LuminosP6A : MotionControllerBase<LuminosAxis>
    {
        #region Variables
        Positioner pos = new Positioner();
        int _axes_registered = 0;

        /// <summary>
        /// Wait until all axes were registered.
        /// After a axis is found, the SDK raises the 'OnAxisCreated' event; If there are 6 axes,
        /// this event should be raised 6 times.
        /// </summary>
        SemaphoreSlim _wait_axis_register;
        #endregion

        #region Constructor
        public LuminosP6A(ConfigPhysicalMotionController Config)
            :base(Config)
        {
            _wait_axis_register = new SemaphoreSlim(0);

            pos.OnAxisCreated += Pos_OnAxisCreated;
        }

        #endregion

        #region Events of luminos sdk

        /// <summary>
        /// Fires each time an Axis object is added to the Axes collection, usually fired during the execution of the Connect method. 
        /// </summary>
        /// <param name="Unit">The index value of the new Axis that has been created and added to the Axes collection. The first Axis created will have a unit/index value of 1. </param>
        private void Pos_OnAxisCreated(byte Unit)
        {
            PositionerLib.Axis _luminos_axis = pos.Axes[Unit];
            LuminosAxis _axis = null;

            _axis = FindAxisByName(_luminos_axis.AxisType.ToString()) as LuminosAxis;

            if (_axis != null)
            {
                _axis.RegisterLuminosSDKAxis(_luminos_axis);
                LogHelper.WriteLine(string.Format("A luminos axis was find, the type is {0}.", _luminos_axis.AxisType));
            }
            else
                LogHelper.WriteLine(string.Format("There is no definition of the luminos type {0}, please check the config file.", _luminos_axis.AxisType));

            _axes_registered++;
            // if all axes have been registered, complete the inititalization task
            if (_axes_registered >= pos.Axes.Count)
            {
                _wait_axis_register.Release();
            }
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

                pos.Port = _config.Port;
                try
                {
                    pos.Connect();

                    // wait until all luminos axes are returned by SDK
                    if (_wait_axis_register.Wait(new TimeSpan(0, 0, 30)))
                    {
                        this.IsInitialized = true;
                        return true;
                    }
                    else
                    {
                        this.LastError = "Initialization timeout, the SDK did not return expected amount of axes";
                        return false;
                    }
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

                PositionerLib.ResultEnum luminos_ret;

                // lock the axis
                if (_axis.Lock())
                {
                    this.IncreaseRunningAxes();

                    try
                    {
                        _axis.IsHomed = false;
                        luminos_ret = _axis.LuminosAxisInstance.Home();
                        
                        if (luminos_ret == PositionerLib.ResultEnum.R_OK)
                        {
                            _axis.IsHomed = true;

                            ret = true;
                        }
                        else
                        {
                            _axis.LastError = string.Format("sdk reported error code {0}", luminos_ret);

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
                PositionerLib.ResultEnum luminos_ret = PositionerLib.ResultEnum.R_ERROR;

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
                            luminos_ret = _axis.LuminosAxisInstance.SetPosition(target_pos, true, out object lastpos);

                            if (luminos_ret == PositionerLib.ResultEnum.R_OK)
                            {
                                ret = true;
                            }
                            else
                            {
                                _axis.LastError = string.Format("sdk reported error code {0}", luminos_ret);

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
            pos.Axes.StopAll();
        }
        
        #endregion
    }
}
