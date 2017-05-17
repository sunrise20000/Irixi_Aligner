using Irixi_Aligner_Common.Classes;
using Irixi_Aligner_Common.Configuration;
using Irixi_Aligner_Common.Interfaces;
using System;
using Thorlabs.MotionControl.DeviceManagerCLI;
using Thorlabs.MotionControl.GenericMotorCLI.AdvancedMotor;
using Thorlabs.MotionControl.GenericMotorCLI.Settings;
using Thorlabs.MotionControl.TCube.DCServoCLI;

namespace Irixi_Aligner_Common
{
    public class ThorlabsTDC001 : MotionControllerBase
    {

        #region Variables
        TCubeDCServo device = null;
        #endregion

        #region Constructor

        public ThorlabsTDC001(ConfigPhysicalMotionController Config)
            : base(Config)
        {
            this.AxisX = new ThorlabsAxis();
            this.AxisCollection.Add(this.AxisX);
        }

        #endregion


        public override void Home(IAxis Axis)
        {
            throw new NotImplementedException();
        }

        public override void HomeAll()
        {
            // start the device polling         
            device.StartPolling(250);
            // call GetMotorConfiguration on the device to initialize the DeviceUnitConverter object required for real world unit parameters     
            MotorConfiguration motorSettings = device.GetMotorConfiguration(this.Config.Name);
            DCMotorSettings currentDeviceSettings = device.MotorDeviceSettings as DCMotorSettings;
            // display info about device  
            DeviceInfo deviceInfo = device.GetDeviceInfo();

            try
            {
                device.Home(60000);
            }
            catch(Exception ex)
            {
                this.LastError = string.Format("Failed to home device {0}\r\n{1}", this, ex.Message);
            }
            finally
            {
                device.StopPolling();
            }
        }

        public override void Init()
        {
            LogHelper.InsertLog(string.Format("Initializing the stage [{0}]", this));

            /* Create the device */
            device = TCubeDCServo.CreateTCubeDCServo(this.Config.Name);
            if (device.Equals(null))
            {
                this.LastError = string.Format("{0} is not a TCubeDCServo.", this.Config.Name);
                return false;
            }
            else
            {
                /* Connect to the device */
                try
                {
                    device.Connect(this.Config.Name);
                }
                catch (Exception ex)
                {
                    this.LastError = string.Format("Unable to connect to the device {0}\r\n{1}", this.Config.Name, ex.Message);
                    return false;
                }

                /* wait for the device settings to initialize */
                if (!device.IsSettingsInitialized())
                {
                    try
                    {
                        device.WaitForSettingsInitialized(5000);
                    }
                    catch (Exception ex)
                    {
                        this.LastError = string.Format("Settings failed to initialize\r\n{0}", ex.Message);
                        return false;
                    }
                }

                this.IsInitialized = true;
                return true;
            }
        }

        public override bool Move(MotorJogArgs Args)
        {
            throw new NotImplementedException();
        }

        public override void Stop()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return string.Format("Thorlabs TDC001: {0}", this.Name);
        }
    }
}