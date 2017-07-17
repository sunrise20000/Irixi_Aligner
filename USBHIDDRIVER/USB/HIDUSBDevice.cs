using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace USBHIDDRIVER.USB
{
    /// <summary>
    ///
    /// </summary>
    public class HIDUSBDevice: IDisposable
    {
        public event EventHandler<byte[]> OnDataReceived;
        public event EventHandler OnDeviceLost;

        

        bool disposed = false;
       
        public int byteCount = 0;       //Recieved Bytes
        //recieve Buffer (Each report is one Element)
        //this one was replaced by the receive Buffer in the interface
        //public static ArrayList receiveBuffer = new ArrayList();
        
        //USB Object
        private USBSharp myUSB = new USBSharp();

        //thread for read operations
        CancellationTokenSource cts;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="HIDUSBDevice"/> class.
        /// And tries to establish a connection to the device.
        /// </summary>
        /// <param name="VID"></param>
        /// <param name="PID"></param>
        /// <param name="SerialNumber"></param>
        public HIDUSBDevice(string VID, string PID = "", string SerialNumber = "")
        {
            this.VID = VID;
            this.PID = PID;
            this.SerialNumber = SerialNumber;

            cts = new CancellationTokenSource();
        }

        #endregion

        #region Properties
        public string PID { private set; get; }
        public string VID { private set; get; }
        public string SerialNumber { private set; get; }
        public string DevicePath { private set; get; }
        public bool IsConnected { private set; get; }
        #endregion

        public bool ConnectDevice()
        { 
            //searchDevice
            SearchDevice();

            //return connection state
            return this.IsConnected;
        }

        bool SearchDevice()
        {
            this.DevicePath = string.Empty;
            
            myUSB.CT_HidGuid(); // Get the GUID of the HID device Class

            myUSB.CT_SetupDiGetClassDevs(); // Get the device information set


            bool? result = null;
            bool resultb = false;
            int device_count = 0;
            int size = 0;
            int requiredSize = 0;

            // Reset the IsConnected to false
            this.IsConnected = false;

            //search the device until you have found it or no more devices in list
            while (!result.HasValue || result.Value == true)
            {
                //
                //if (result == false)
                //    break;

                //open the device
                result = myUSB.CT_SetupDiEnumDeviceInterfaces(device_count);

                //get size of device path
                resultb = myUSB.CT_SetupDiGetDeviceInterfaceBuffer(ref requiredSize, 0);

                size = requiredSize;
                
                //get device path
                resultb = myUSB.CT_SetupDiGetDeviceInterfaceDetail(ref requiredSize, size);

                if (resultb == false)
                {
                    int err = Marshal.GetLastWin32Error();
                }
                
                this.DevicePath = myUSB.DevicePathName;

                //is this the device i want?
                string deviceID = this.VID + "&" + this.PID;

                if (this.DevicePath.ToLower().IndexOf(deviceID.ToLower()) > 0)
                {
                    //create HID Device Handel
                    resultb = myUSB.CT_CreateFile(this.DevicePath);

                    // Check the serial Number
                    myUSB.CT_HidD_GetHIDSerialNumber(out string device_sn);

                    if (this.SerialNumber == null || this.SerialNumber == string.Empty || device_sn == this.SerialNumber)
                    {
                        IntPtr myPtrToPreparsedData = default(IntPtr);

                        if (myUSB.CT_HidD_GetPreparsedData(myUSB.hHidFile, ref myPtrToPreparsedData))
                        {

                            bool code = myUSB.CT_HidP_GetCaps(myPtrToPreparsedData);
                            int reportLength = myUSB.myHIDP_CAPS.InputReportByteLength;

                            //we have found our device so stop searching
                            this.IsConnected = true;
                            this.SerialNumber = device_sn;
                        }

                        break;
                    }
                    else
                    {
                        myUSB.CT_CloseFile();
                    }
                }

                device_count++;
            }

            myUSB.CT_SetupDiDestroyDeviceInfoList();
            
            //return state
            return this.IsConnected;
        }

        /// <summary>
        /// returns the number of devices with specified vendorID and productID 
        /// </summary>
        /// <returns>returns the number of devices with specified vendorID and productID</returns>
        public int GetDeviceCount()
        {
            this.DevicePath = string.Empty;

            myUSB.CT_HidGuid();
            myUSB.CT_SetupDiGetClassDevs();

            bool result = false;
            bool resultb = false;
            int device_count = 0;
            int size = 0;
            int requiredSize = 0;
            int numberOfDevices = 0;
            //search the device until you have found it or no more devices in list
            while (result)
            {
                //open the device
                result = myUSB.CT_SetupDiEnumDeviceInterfaces(device_count);
                //get size of device path
                resultb = myUSB.CT_SetupDiGetDeviceInterfaceBuffer(ref requiredSize, 0);
                size = requiredSize;
                //get device path
                resultb = myUSB.CT_SetupDiGetDeviceInterfaceDetail(ref requiredSize, size);

                //is this the device i want?
                string deviceID = this.VID + "&" + this.PID;
                if (myUSB.DevicePathName.IndexOf(deviceID) > 0)
                {
                   numberOfDevices++;
                }
                device_count++;
            }
            return numberOfDevices;
        }

        /// <summary>
        /// Writes the data.
        /// </summary>
        /// <param name="bDataToWrite">The b data to write.</param>
        /// <returns></returns>
        public bool WriteData(byte[] bDataToWrite)
        {
            bool success = false;
            if (this.IsConnected)
            {
                try
                {
                    //get output report length
                    int myPtrToPreparsedData = -1;
                   // myUSB.CT_HidD_GetPreparsedData(myUSB.HidHandle, ref myPtrToPreparsedData);
                   // int code = myUSB.CT_HidP_GetCaps(myPtrToPreparsedData);

                    int outputReportByteLength = myUSB.myHIDP_CAPS.OutputReportByteLength;

                    int bytesSend = 1;
                    //if bWriteData is bigger then one report diveide into sevral reports
                    while (bytesSend < bDataToWrite.Length)
                    {

                        // Set the size of the Output report buffer.
                       // byte[] OutputReportBuffer = new byte[myUSB.myHIDP_CAPS.OutputReportByteLength - 1 + 1];
                        byte[] OutputReportBuffer = new byte[outputReportByteLength];

                        // set the report id
                        OutputReportBuffer[0] = bDataToWrite[0];

                        // Store the report data following the report ID.
                        for (int i = 1; i < OutputReportBuffer.Length; i++)
                        {
                            if (bytesSend < bDataToWrite.Length)
                            {
                                OutputReportBuffer[i] = bDataToWrite[bytesSend];
                                bytesSend++;
                            }
                            else
                            {
                                OutputReportBuffer[i] = 0;
                            }
                        }

                        OutputReport myOutputReport = new OutputReport();
                        success = myOutputReport.Write(OutputReportBuffer, myUSB.hHidFile);
                    }
                }
                catch (AccessViolationException ex)
                {
                    success = false;
                }
            }
            else 
            {
                success = false;
            }
            return success;
        }

        /// <summary>
        /// Read HID report from hid device
        /// </summary>
        /// <returns></returns>
        public byte[] ReadData()
        {
            byte[] retval = myUSB.CT_ReadFile(myUSB.myHIDP_CAPS.InputReportByteLength);

            return retval;
        }
       

        /// <summary>
        ///  ThreadMethod for reading Data
        /// </summary>
        Task DoReadHIDReport(CancellationToken ct)
        {
            return Task.Run(() =>
            {
                int receivedNull = 0;
                //byte[] recv_buf = null;

                while (true)
                {
                    var myPtrToPreparsedData = default(IntPtr);
                    if (myUSB.CT_HidD_GetPreparsedData(myUSB.hHidFile, ref myPtrToPreparsedData))
                    {
                        bool code = myUSB.CT_HidP_GetCaps(myPtrToPreparsedData);
                        int reportLength = myUSB.myHIDP_CAPS.InputReportByteLength;

                        while (true)
                        {
                            //read until thread is stopped
                            /*
                             * <ref>https://docs.microsoft.com/zh-cn/windows-hardware/drivers/hid/obtaining-hid-reports-by-user-mode-applications#using_readfile</ref>
                            if(myUSB.CT_HidD_GetInputReport(out recv_buf))
                            {
                                byteCount += recv_buf.Length;
                                OnDataReceived?.Invoke(this, recv_buf);

                                ct.ThrowIfCancellationRequested();
                            }
                            else
                            {
                                if (receivedNull > 100)
                                {
                                    throw new Exception("Unable to read data from the HID device, it could be disconnected.");
                                }
                                receivedNull++;
                            }
                            */
                            ct.ThrowIfCancellationRequested();

                            byte[] myRead = myUSB.CT_ReadFile(myUSB.myHIDP_CAPS.InputReportByteLength);
                            if (myRead != null)
                            {
                                //ByteCount + bytes received
                                byteCount += myRead.Length;

                                OnDataReceived?.Invoke(this, myRead);
                            }
                            else
                            {
                                throw new Exception("Unable to read data from the HID device, it could be disconnected.");
                            }
                        }
                    }
                }
            });
        }


        /// <summary>
        /// controls the read thread
        /// </summary>
        public async void StartReadHIDReport()
        {
            var t = DoReadHIDReport(cts.Token);

            try
            {
                
                await t;
            }
            catch (Exception ex)
            {
                // Do nothing if error
            }
            finally
            {
                this.DisconnectDevice();

                OnDeviceLost?.Invoke(this, new EventArgs());
            }
        }

        /// <summary>
        /// Aborts the read thread.
        /// </summary>
        public void AbortDataRead()
        {
            cts.Cancel();
        }

        
        /// <summary>
        /// Disconnects the device.
        /// </summary>
        public void DisconnectDevice()
        {
            myUSB.CT_CloseFile();
            myUSB.CT_SetupDiDestroyDeviceInfoList();

            this.IsConnected = false;
        }


        /// <summary>
        /// Gets the device count.
        /// </summary>
        /// <returns></returns>
        public List<string> GetDevices()
        {
            List<string> devices = new List<string>();

            this.DevicePath = string.Empty;

            myUSB.CT_HidGuid();
            myUSB.CT_SetupDiGetClassDevs();

            bool? result = null;
            bool resultb = false;
            int device_count = 0;
            int size = 0;
            int requiredSize = 0;
            int numberOfDevices = 0;
            //search the device until you have found it or no more devices in list

            while (result.HasValue == false || result.Value == true)
            {
                //open the device
                result = myUSB.CT_SetupDiEnumDeviceInterfaces(device_count);
                //get size of device path
                resultb = myUSB.CT_SetupDiGetDeviceInterfaceBuffer(ref requiredSize, 0);
                size = requiredSize;
                //get device path
                resultb = myUSB.CT_SetupDiGetDeviceInterfaceDetail(ref requiredSize, size);

                //is this the device i want?
                string deviceID = this.VID;
                if (myUSB.DevicePathName.IndexOf(deviceID) > 0)
                {

                    //create HID Device Handel
                    resultb = myUSB.CT_CreateFile(myUSB.DevicePathName);

                    // Check the serial Number
                    myUSB.CT_HidD_GetHIDSerialNumber(out string device_sn);

                    myUSB.CT_CloseFile();

                    if(device_sn != "")
                        devices.Add(device_sn);

                    numberOfDevices++;
                }

                device_count++;

            } 

            myUSB.CT_SetupDiDestroyDeviceInfoList();

            return devices;
        }

        public string GetDevicePath()
        {
            return this.DevicePath;
        }

        internal abstract class HostReport
        {

            // For reports the host sends to the device.

            // Each report class defines a ProtectedWrite method for writing a type of report.

            protected abstract bool ProtectedWrite(IntPtr deviceHandle, byte[] reportBuffer);


            internal bool Write(byte[] reportBuffer, IntPtr deviceHandle)
            {

                bool Success = false;

                // Purpose    : Calls the overridden ProtectedWrite routine.
                //            : This method enables other classes to override ProtectedWrite
                //            : while limiting access as Friend.
                //            : (Directly declaring Write as Friend MustOverride causes the
                //            : compiler(warning) "Other languages may permit Friend
                //            : Overridable members to be overridden.")
                // Accepts    : reportBuffer - contains the report ID and report data.
                //            : deviceHandle - handle to the device.             '
                // Returns    : True on success. False on failure.

                try
                {
                    Success = ProtectedWrite(deviceHandle, reportBuffer);
                }
                catch (Exception ex)
                {
                    
                }

                return Success;
            }
        }

        internal class OutputReport : HostReport
        {

            // For Output reports the host sends to the device.
            // Uses interrupt or control transfers depending on the device and OS.

            protected override bool ProtectedWrite(IntPtr hidHandle, byte[] outputReportBuffer)
            {

                // Purpose    : writes an Output report to the device.
                // Accepts    : HIDHandle - a handle to the device.
                //              OutputReportBuffer - contains the report ID and report to send.
                // Returns    : True on success. False on failure.

                int NumberOfBytesWritten = 0;
                bool Success = false;

                try
                {
                    // The host will use an interrupt transfer if the the HID has an interrupt OUT
                    // endpoint (requires USB 1.1 or later) AND the OS is NOT Windows 98 Gold (original version).
                    // Otherwise the the host will use a control transfer.
                    // The application doesn't have to know or care which type of transfer is used.

                    // ***
                    // API function: WriteFile
                    // Purpose: writes an Output report to the device.
                    // Accepts:
                    // A handle returned by CreateFile
                    // The output report byte length returned by HidP_GetCaps.
                    // An integer to hold the number of bytes written.
                    // Returns: True on success, False on failure.
                    // ***
                    var overlapped = new NativeOverlapped();

                    Success = USBSharp.WriteFile(hidHandle, ref outputReportBuffer[0], outputReportBuffer.Length, ref NumberOfBytesWritten, ref overlapped);
                    if (Success == false)
                    {
                        int err = USBSharp.GetLastError();
                    }

                }
                catch (Exception ex)
                {
                }

                return Success;
            }

        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposeManagedResources)
        {
            if (!this.disposed)
            {
                if (disposeManagedResources)
                {
                    //only clear up managed stuff here
                }

                //clear up unmanaged stuff here
                if (myUSB.hHidFile != IntPtr.Zero)
                {
                    myUSB.CT_CloseFile();
                }

                if (myUSB.hDevInfoSet != IntPtr.Zero)
                {
                    myUSB.CT_SetupDiDestroyDeviceInfoList();
                }

                this.disposed = true;
            }
        }

        #endregion

      
    }
}
