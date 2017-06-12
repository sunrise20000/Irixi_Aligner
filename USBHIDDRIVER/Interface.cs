
using System;
using System.ComponentModel;
using System.Threading;

namespace USBHIDDRIVER
{
    /// <summary>
    /// Interface for the HID USB Driver.
    /// </summary>
    public class USBInterface
    {
        #region Variables
        private string usbVID;
        private string usbPID;
        private string deviceSN;
        private bool isConnected;

        private USB.HIDUSBDevice usbdevice;

        /// <summary>
        /// Buffer for incomming data.
        /// </summary>
        public BindingList<byte[]> usbBuffer;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="USBInterface"/> class.
        /// </summary>
        /// <param name="VID">The vendor id of the USB device (e.g. vid_06ba)</param>
        /// <param name="PID">The product id of the USB device (e.g. pid_ffff)</param>
        public  USBInterface(string VID, string PID = "", string SerialNumber = "")
        {
            _constructor(VID, PID, SerialNumber);
        }

        public USBInterface(int VID, int PID = 0, string SerialNumber = "")
        {

            string vid = string.Format("vid_{0:x4}", VID);
            string pid = PID == 0 ? "" : string.Format("pid_{0:x4}", PID);
            _constructor(vid, pid, SerialNumber);
        }

        private void _constructor(string vid, string pid, string serialnumber)
        {
            this.usbVID = vid;
            this.usbPID = pid;
            this.deviceSN = serialnumber;

            this.usbdevice = new USB.HIDUSBDevice(this.usbVID, this.usbPID, this.deviceSN);
        }
        #endregion

        /// <summary>
        /// Establishes a connection to the USB device. 
        /// You can only establish a connection to a device if you have used the construct with vendor AND product id. 
        /// Otherwise it will connect to a device which has the same vendor id is specified, 
        /// this means if more than one device with these vendor id is plugged in, 
        /// you can't be determine to which one you will connect. 
        /// </summary>
        /// <returns>false if an error occures</returns>
        public bool Connect()
        {
            return this.usbdevice.ConnectDevice();
        }

        /// <summary>
        /// Disconnects the device
        /// </summary>
        public void Disconnect()
        {
            if (isConnected)
            {
                this.usbdevice.DisconnectDevice();
            }
        }

        /// <summary>
        /// Returns a list of devices with the vendor id (or vendor and product id) 
        /// specified in the constructor.
        /// This function is needed if you want to know how many (and which) devices with the specified
        /// vendor id are plugged in.
        /// </summary>
        /// <returns>String list with device paths</returns>
        public String[] GetDeviceList()
        {
            return (String[])this.usbdevice.GetDevices().ToArray();
        }

        /// <summary>
        /// Writes the specified bytes to the USB device.
        /// If the array length exceeds 64, the array while be divided into several 
        /// arrays with each containing 64 bytes.
        /// The 0-63 byte of the array is sent first, then the 64-127 byte and so on.
        /// </summary>
        /// <param name="bytes">The bytes to send.</param>
        /// <returns>Returns true if all bytes have been written successfully</returns>
        public bool Write(Byte[] bytes)
        {
                int byteCount = bytes.Length;
                int bytePos = 0;
               
                bool success = true;

                //build hid reports with 64 bytes
                while (bytePos <= byteCount-1)
                {
                    if (bytePos > 0)
                    {
                        Thread.Sleep(5);
                    }
                    Byte[] transfByte = new byte[64];
                    for (int u = 0; u < 64; u++)
                    {
                        if (bytePos < byteCount)
                        {
                            transfByte[u] = bytes[bytePos];
                            bytePos++;
                        }
                        else 
                        {
                            transfByte[u] = 0;
                        }
                    }
                    //send the report
                    if (!this.usbdevice.WriteData(transfByte))
                    {
                        success = false;
                    }
                    Thread.Sleep(5);
                }
                return success;
        }
        
        /// <summary>
        /// Read hid report
        /// </summary>
        /// <returns></returns>
        public byte[] Read()
        {
            return this.usbdevice.ReadData();
        }

        /// <summary>
        /// Starts reading. 
        /// If you execute this command a thread is started which listens to the USB device and waits for data.
        /// </summary>
        public void StartRead()
        {
            this.usbdevice.StartReadHIDReport();
        }

        /// <summary>
        /// Stops the read thread.
        /// By executing this command the read data thread is stopped and now data will be received.
        /// </summary>
        public void StopRead()
        {
            this.usbdevice.AbortDataRead();
        }

        /// <summary>
        /// Enables the usb buffer event.
        /// Whenever a dataset is added to the buffer (and so received from the usb device)
        /// the event handler method will be called.
        /// </summary>
        /// <param name="Callback">The event handler method.</param>
        public void EnableUsbBufferEvent(EventHandler<byte[]> CallbackFunc)
        {
            this.usbdevice.OnDataReceived += CallbackFunc;
            
        }
        
        public void EnableUsbDisconnectEvent(EventHandler CallbackFunc)
        {
            this.usbdevice.OnDeviceLost += CallbackFunc;
        }
    }
}
