using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace USBHIDDRIVER.USB
{
    /// <summary>
    /// Summary description
    /// </summary>
    internal class USBSharp
    {

        #region Constant Values

        internal const int DIGCF_PRESENT = 0x00000002;
        internal const int DIGCF_DEVICEINTERFACE = 0x00000010;
        internal const int DIGCF_INTERFACEDEVICE = 0x00000010;
        internal const uint GENERIC_READ = 0x80000000;
        internal const uint GENERIC_WRITE = 0x40000000;
        internal const int FILE_SHARE_READ = 0x00000001;
        internal const int FILE_SHARE_WRITE = 0x00000002;
        internal const int OPEN_EXISTING = 3;
        internal const int EV_RXFLAG = 0x0002;       // received certain character

        internal const uint FILE_FLAG_OVERLAPPED = 0x40000000;
        internal const int WAIT_TIMEOUT = 0x102;
        internal const short WAIT_OBJECT_0 = 0;

        /// <summary>
        /// specified in DCB
        /// </summary>
        internal const int INVALID_HANDLE_VALUE = -1;
        internal const int ERROR_INVALID_HANDLE = 6;
        internal const int FILE_FLAG_OVERLAPED = 0x40000000;

        #endregion

        #region Structs and DLL-Imports

        [StructLayout(LayoutKind.Sequential)]
        internal struct SECURITY_ATTRIBUTES
        {
            internal int nLength;
            internal IntPtr lpSecurityDescriptor;
            internal bool bInheritHandle;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SP_DEVINFO_DATA
        {
            internal int cbSize;
            internal Guid ClassGuid;
            internal int DevInst;
            internal IntPtr Reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SP_DEVICE_INTERFACE_DATA
        {
            internal int cbSize;
            internal Guid InterfaceClassGuid;
            internal int Flags;
            internal IntPtr Reserved;
        }

        /// <summary>
        /// Device interface detail data
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        internal struct SP_DEVICE_INTERFACE_DETAIL_DATA
        {
            internal int cbSize;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            internal string DevicePath;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 1)]
        struct NativeDeviceInterfaceDetailData
        {
            public int size;
            public char devicePath;
        }

        /// <summary>
        /// HIDD_ATTRIBUTES
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct HIDD_ATTRIBUTES
        {
            internal int Size;
            internal ushort VendorID;
            internal ushort ProductID;
            internal ushort VersionNumber;
        }

        // HIDP_CAPS
        [StructLayout(LayoutKind.Sequential)]
        internal struct HIDP_CAPS
        {
            internal short Usage;
            internal short UsagePage;
            internal short InputReportByteLength;
            internal short OutputReportByteLength;
            internal short FeatureReportByteLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
            internal short[] Reserved;
            internal short NumberLinkCollectionNodes;
            internal short NumberInputButtonCaps;
            internal short NumberInputValueCaps;
            internal short NumberInputDataIndices;
            internal short NumberOutputButtonCaps;
            internal short NumberOutputValueCaps;
            internal short NumberOutputDataIndices;
            internal short NumberFeatureButtonCaps;
            internal short NumberFeatureValueCaps;
            internal short NumberFeatureDataIndices;
        }

        /// <summary>
        /// HIDP_REPORT_TYPE 
        /// </summary>
        internal enum HIDP_REPORT_TYPE
        {
            HidP_Input,     // 0 input
            HidP_Output,    // 1 output
            HidP_Feature    // 2 feature
        }

        #endregion

        #region Structures in the union belonging to HIDP_VALUE_CAPS (see below)

        [StructLayout(LayoutKind.Sequential)]
        internal struct Range
        {
            internal ushort UsageMin;
            internal ushort UsageMax;
            internal ushort StringMin;
            internal ushort StringMax;
            internal ushort DesignatorMin;
            internal ushort DesignatorMax;
            internal ushort DataIndexMin;
            internal ushort DataIndexMax;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct NotRange
        {
            internal ushort Usage;
            internal ushort Reserved1;
            internal ushort StringIndex;
            internal ushort Reserved2;
            internal ushort DesignatorIndex;
            internal ushort Reserved3;
            internal ushort DataIndex;
            internal ushort Reserved4;
        }



        //HIDP_VALUE_CAPS
        [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi)]
        internal struct HIDP_VALUE_CAPS
        {
            //
            [FieldOffset(0)] internal ushort UsagePage;          // USHORT
            [FieldOffset(2)] internal Byte ReportID;              // UCHAR  ReportID;
            [MarshalAs(UnmanagedType.I1)]
            [FieldOffset(3)]
            internal Boolean IsAlias;         // BOOLEAN  IsAlias;
            [FieldOffset(4)] internal ushort BitField;            // USHORT  BitField;
            [FieldOffset(6)] internal ushort LinkCollection;      // USHORT  LinkCollection;
            [FieldOffset(8)] internal ushort LinkUsage;           // USAGE  LinkUsage;
            [FieldOffset(10)] internal ushort LinkUsagePage;      // USAGE  LinkUsagePage;
            [MarshalAs(UnmanagedType.I1)]
            [FieldOffset(12)]
            internal Boolean IsRange;         // BOOLEAN  IsRange;
            [MarshalAs(UnmanagedType.I1)]
            [FieldOffset(13)]
            internal Boolean IsStringRange;       // BOOLEAN  IsStringRange;
            [MarshalAs(UnmanagedType.I1)]
            [FieldOffset(14)]
            internal Boolean IsDesignatorRange;   // BOOLEAN  IsDesignatorRange;
            [MarshalAs(UnmanagedType.I1)]
            [FieldOffset(15)]
            internal Boolean IsAbsolute;      // BOOLEAN  IsAbsolute;
            [MarshalAs(UnmanagedType.I1)]
            [FieldOffset(16)]
            internal Boolean HasNull;         // BOOLEAN  HasNull;
            [FieldOffset(17)] internal Char Reserved;             // UCHAR  Reserved;
            [FieldOffset(18)] internal ushort BitSize;            // USHORT  BitSize;
            [FieldOffset(20)] internal ushort ReportCount;        // USHORT  ReportCount;
            [FieldOffset(22)] internal ushort Reserved2a;     // USHORT  Reserved2[5];		
            [FieldOffset(24)] internal ushort Reserved2b;     // USHORT  Reserved2[5];
            [FieldOffset(26)] internal ushort Reserved2c;     // USHORT  Reserved2[5];
            [FieldOffset(28)] internal ushort Reserved2d;     // USHORT  Reserved2[5];
            [FieldOffset(30)] internal ushort Reserved2e;     // USHORT  Reserved2[5];
            [FieldOffset(32)] internal ushort UnitsExp;           // ULONG  UnitsExp;
            [FieldOffset(34)] internal ushort Units;              // ULONG  Units;
            [FieldOffset(36)] internal Int16 LogicalMin;          // LONG  LogicalMin;   ;
            [FieldOffset(38)] internal Int16 LogicalMax;          // LONG  LogicalMax
            [FieldOffset(40)] internal Int16 PhysicalMin;         // LONG  PhysicalMin, 
            [FieldOffset(42)] internal Int16 PhysicalMax;         // LONG  PhysicalMax;
                                                                  // The Structs in the Union			
            [FieldOffset(44)] internal Range Range;
            [FieldOffset(44)] internal Range NotRange;
        }

        #endregion

        #region Variables
        internal IntPtr hHidFile = IntPtr.Zero;              // file handle for a Hid devices
        internal IntPtr hDevInfoSet = IntPtr.Zero;               // handle for the device infoset
        internal string DevicePathName = "";
        private Guid hidClass = new Guid();
        internal SP_DEVINFO_DATA deviceInfoData;
        internal SP_DEVICE_INTERFACE_DATA deviceInterfaceData;
        internal HIDD_ATTRIBUTES myHIDD_ATTRIBUTES;
        internal HIDP_CAPS myHIDP_CAPS;
        internal HIDP_VALUE_CAPS[] myHIDP_VALUE_CAPS;

        #endregion

        #region HID APIs

        /// <summary>
        /// Get GUID for the HID Class
        /// </summary>
        /// <param name="lpHidGuid"></param>
        [DllImport("hid.dll", SetLastError = true)]
        static extern void HidD_GetHidGuid(ref Guid lpHidGuid);

        /// <summary>
        /// Get array of structures with the HID info
        /// </summary>
        /// <param name="lpHidGuid"></param>
        /// <param name="Enumerator"></param>
        /// <param name="hwndParent"></param>
        /// <param name="Flags"></param>
        /// <returns></returns>
        [DllImport("setupapi.dll", SetLastError = true)]
        static extern IntPtr SetupDiGetClassDevs(
            ref Guid lpHidGuid,
            string Enumerator,
            int hwndParent,
            int Flags);


        [DllImport("setupapi.dll")]
        static internal extern bool SetupDiEnumDeviceInfo(
            IntPtr deviceInfoSet,
            int memberIndex,
            ref SP_DEVINFO_DATA deviceInfoData);


        /// <summary>
        /// 
        /// Get context structure for a device interface element
        /// 
        ///   SetupDiEnumDeviceInterfaces returns a context structure for a device 
        ///   interface element of a device information set. Each call returns information 
        ///   about one device interface; the function can be called repeatedly to get information
        ///   about several interfaces exposed by one or more devices.
        /// 
        /// </summary>
        /// <param name="DeviceInfoSet"></param>
        /// <param name="DeviceInfoData"></param>
        /// <param name="lpHidGuid"></param>
        /// <param name="MemberIndex"></param>
        /// <param name="lpDeviceInterfaceData"></param>
        /// <returns></returns>
        [DllImport("setupapi.dll", SetLastError = true)]
        static extern bool SetupDiEnumDeviceInterfaces(
            IntPtr DeviceInfoSet,
            IntPtr DeviceInfoData,
            ref Guid interfaceClassGuid,
            int MemberIndex,
            ref SP_DEVICE_INTERFACE_DATA lpDeviceInterfaceData);


        [DllImport("setupapi.dll", CharSet = CharSet.Auto, EntryPoint = "SetupDiGetDeviceInterfaceDetail")]
        static internal extern bool SetupDiGetDeviceInterfaceDetailBuffer(
            IntPtr deviceInfoSet,
            ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData,
            IntPtr deviceInterfaceDetailData,
            int deviceInterfaceDetailDataSize,
            ref int requiredSize,
            IntPtr deviceInfoData);



        /// <summary>
        /// 
        /// Get device Path name
        /// Works for second pass (overide), once size value is known
        /// 
        /// </summary>
        /// <param name="DeviceInfoSet"></param>
        /// <param name="lpDeviceInterfaceData"></param>
        /// <param name="myPSP_DEVICE_INTERFACE_DETAIL_DATA"></param>
        /// <param name="detailSize"></param>
        /// <param name="requiredSize"></param>
        /// <param name="bPtr"></param>
        /// <returns></returns>
        [DllImport("setupapi.dll", SetLastError = true)]
        unsafe static extern bool SetupDiGetDeviceInterfaceDetail(
            IntPtr DeviceInfoSet,
            ref SP_DEVICE_INTERFACE_DATA lpDeviceInterfaceData,
            ref NativeDeviceInterfaceDetailData lpDeviceInterfaceDetail,
            int detailSize,
            ref int requiredSize,
            IntPtr bPtr);


        [DllImport("hid.dll", SetLastError = true)]
        private static extern bool HidD_GetAttributes(
            IntPtr hidDeviceObject,                                // IN HANDLE  HidDeviceObject,
            ref HIDD_ATTRIBUTES Attributes);            // OUT PHIDD_ATTRIBUTES  Attributes


        [DllImport("hid.dll", SetLastError = true)]
        private static extern bool HidD_GetPreparsedData(
            IntPtr hidDeviceObject,                                // IN HANDLE  HidDeviceObject,
            ref IntPtr PreparsedData);             // OUT PHIDP_PREPARSED_DATA  *PreparsedData


        [DllImport("hid.dll", SetLastError = true)]
        private static extern bool HidP_GetCaps(
            IntPtr preparsedData,
            ref HIDP_CAPS capabilities);


        [DllImport("hid.dll", SetLastError = true)]
        internal static extern bool HidD_GetSerialNumberString(
            IntPtr hidDeviceObject,
            ref byte lpReportBuffer,
            int reportBufferLength);

        [DllImport("hid.dll")]
        internal static extern bool HidD_SetOutputReport(
            IntPtr hidDeviceObject,
            byte[] lpReportBuffer,
            int reportBufferLength);

        [DllImport("hid.dll")]
        internal static extern bool HidD_GetInputReport(
            IntPtr HidDeviceObject,
            ref byte lpReportBuffer,
            int ReportBufferLength);


        [DllImport("hid.dll", SetLastError = true)]
        private static extern int HidP_GetValueCaps(
            short reportType,
            [In, Out] HIDP_VALUE_CAPS[] valueCaps,
            ref short valueCapsLength,
            IntPtr preparsedData);


        [DllImport("setupapi.dll", SetLastError = true)]
        static extern int SetupDiDestroyDeviceInfoList(
            IntPtr deviceInfoSet
            );

        // 13
        [DllImport("hid.dll", SetLastError = true)]
        static extern int HidD_FreePreparsedData(
            IntPtr preparsedData
            );


        #endregion

        #region WIN32 APIs
        [DllImport("kernel32.dll")]
        static internal extern int GetLastError();

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static internal extern bool CancelIo(IntPtr hFile);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static internal extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        static public extern int WaitForSingleObject(IntPtr hHandle, int dwMilliseconds);


        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static internal extern IntPtr CreateEvent(
            ref SECURITY_ATTRIBUTES SecurityAttributes,
            int bManualReset,
            int bInitialState,
            string lpName);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static internal extern IntPtr CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            int dwShareMode,
            ref SECURITY_ATTRIBUTES lpSecurityAttributes,
            int dwCreationDisposition,
            int dwFlagsAndAttributes,
            int hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto)]
        static internal extern bool CancelIoEx(IntPtr hFile, IntPtr lpOverlapped);


        [DllImport("kernel32.dll")]
        static internal extern bool ReadFile(
            IntPtr hFile,
            IntPtr lpBuffer,
            uint nNumberOfBytesToRead,
            out uint lpNumberOfBytesRead,
            [In] ref NativeOverlapped lpOverlapped);


        [DllImport("kernel32.dll")]
        static internal extern bool WriteFile(
            IntPtr hFile,
            ref byte lpBuffer,
            int nNumberOfBytesToWrite,
            ref int lpNumberOfBytesWritten,
            [In] ref NativeOverlapped lpOverlapped);



        #endregion


        /// <summary>
        /// Get class GUID
        /// </summary>
        internal void CT_HidGuid()
        {
            HidD_GetHidGuid(ref hidClass);
        }


        /// <summary>
        /// The functions returns a handle to a device information set that contains requested device(Specified by hidClass) informatioin elements for a local computer
        /// </summary>
        /// <returns></returns>
		internal IntPtr CT_SetupDiGetClassDevs()
        {
            hDevInfoSet = SetupDiGetClassDevs(
                ref hidClass,
                null,
                0,
                USBSharp.DIGCF_INTERFACEDEVICE | USBSharp.DIGCF_PRESENT);
            return hDevInfoSet;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="memberIndex"></param>
        /// <returns></returns>
        internal bool CT_SetupDiEnumDeviceInterfaces(int memberIndex)
        {
            deviceInterfaceData = new SP_DEVICE_INTERFACE_DATA();
            deviceInterfaceData.cbSize = Marshal.SizeOf(deviceInterfaceData);

            bool result = SetupDiEnumDeviceInterfaces(
                hDevInfoSet,
                IntPtr.Zero,
                ref hidClass,
                memberIndex,
                ref deviceInterfaceData);

            return result;
        }

        /// <summary>
        /// DESCRIPTION:
        ///   results = 0 is OK with the first pass of the routine since we are
        ///   trying to get the RequiredSize parameter so in the next call we can read the entire detail
        /// </summary>
        /// <param name="RequiredSize"></param>
        /// <param name="DeviceInterfaceDetailDataSize"></param>
        /// <returns></returns>
        internal bool CT_SetupDiGetDeviceInterfaceBuffer(ref int RequiredSize, int DeviceInterfaceDetailDataSize)
        {

            bool results =
            SetupDiGetDeviceInterfaceDetailBuffer(
                hDevInfoSet,
                ref deviceInterfaceData,
                IntPtr.Zero,
                DeviceInterfaceDetailDataSize,
                ref RequiredSize,
                IntPtr.Zero);
            return results;
        }


        /// <summary>
        /// DESCRIPTION:
        /// results = 1 in the second pass of the routine is success
        /// DeviceInterfaceDetailDataSize parameter (RequiredSize) came from the first pass
        /// </summary>
        /// <param name="RequiredSize"></param>
        /// <param name="DeviceInterfaceDetailDataSize"></param>
        /// <returns></returns>
        internal bool CT_SetupDiGetDeviceInterfaceDetail(ref int RequiredSize, int DeviceInterfaceDetailDataSize)
        {
            // 
            //!                                *** IMPORTANT NOTICE OF THE FIELD cbSize ***
            //
            // A pointer to an SP_DEVICE_INTERFACE_DETAIL_DATA structure to receive information about the specified interface. 
            // This parameter is optional and can be NULL. This parameter must be NULL if DeviceInterfaceDetailSize is zero. 
            // If this parameter is specified, the caller must set DeviceInterfaceDetailData.cbSize to sizeof(SP_DEVICE_INTERFACE_DETAIL_DATA) 
            // before calling this function. The cbSize member always contains the size of the fixed part of the data structure, not a size 
            // reflecting the variable-length string at the end.
            //
            // ref to https://msdn.microsoft.com/en-us/library/windows/hardware/ff551120(v=vs.85).aspx
            // ref to http://pinvoke.net/default.aspx/setupapi.SetupDiGetDeviceInterfaceDetail
            // 

            bool ret;


            IntPtr detail = Marshal.AllocHGlobal((int)DeviceInterfaceDetailDataSize);

            uint size = (uint)DeviceInterfaceDetailDataSize;

            int cbSize = 0;

            if (IntPtr.Size == 8) // for 64 bit operating systems
                cbSize = 8;
            else
                cbSize = 4 + Marshal.SystemDefaultCharSize; // for 32 bit systems


            Marshal.WriteInt32(detail, cbSize);

            try
            {

                ret = SetupDiGetDeviceInterfaceDetailBuffer(
                    hDevInfoSet,
                    ref deviceInterfaceData,
                    detail,
                    DeviceInterfaceDetailDataSize,
                    ref RequiredSize,
                    IntPtr.Zero);

                if (ret == false)
                {
                    int err = Marshal.GetLastWin32Error();
                }

                IntPtr pDevicePathName = new IntPtr(detail.ToInt32() + 4);
                DevicePathName = Marshal.PtrToStringAuto(pDevicePathName);
            }
            catch
            {
                throw new Win32Exception();
            }
            finally
            {
                Marshal.FreeHGlobal(detail);
            }


            /*

            var interfaceDetail = new SP_DEVICE_INTERFACE_DETAIL_DATA();
            if (IntPtr.Size == 8) // for 64 bit operating systems
                interfaceDetail.cbSize = 8;
            else
                interfaceDetail.cbSize = 4 + Marshal.SystemDefaultCharSize; // for 32 bit systems

            ret =
                SetupDiGetDeviceInterfaceDetail(
                hDevInfoSet,                                   // IN HDEVINFO  DeviceInfoSet,
                ref deviceInterfaceData,             // IN PSP_DEVICE_INTERFACE_DATA  DeviceInterfaceData,
                ref interfaceDetail,     // DeviceInterfaceDetailData,  OPTIONAL
                DeviceInterfaceDetailDataSize,              // IN DWORD  DeviceInterfaceDetailDataSize,
                ref RequiredSize,                         // OUT PDWORD  RequiredSize,  OPTIONAL
                IntPtr.Zero); // 

            if (ret == false)
            {
                int err = Marshal.GetLastWin32Error();
            }

    

            DevicePathName = interfaceDetail.DevicePath;
       */

            return ret;
        }


        /// <summary>
        /// Get a handle (opens file) to the HID device
        /// returns  0 is no success - Returns 1 if success
        /// </summary>
        /// <param name="DeviceName"></param>
        /// <returns></returns>
        internal bool CT_CreateFile(string DeviceName)
        {
            var security = new SECURITY_ATTRIBUTES();
            security.lpSecurityDescriptor = IntPtr.Zero;
            security.bInheritHandle = true;
            security.nLength = Marshal.SizeOf(security);

            hHidFile = CreateFile(
                DeviceName,
                GENERIC_READ | GENERIC_WRITE,
                FILE_SHARE_READ | FILE_SHARE_WRITE,
                ref security,
                OPEN_EXISTING,
                (int)FILE_FLAG_OVERLAPPED,
                0);

            if (hHidFile == IntPtr.Zero)
            {
                return false;
            }
            else
            {
                return true;
            }

        }

        internal void CT_CloseFile()
        {
            if (Environment.OSVersion.Version.Major > 5)
            {
                CancelIoEx(hHidFile, IntPtr.Zero);
            }

            CloseHandle(hHidFile);
        }

        /// <summary>
        /// Get HID device serial number
        /// </summary>
        /// <param name="SerialNumber"></param>
        /// <returns></returns>
        internal int CT_HidD_GetHIDSerialNumber(out string SerialNumber)
        {
            byte[] data = new byte[254];
            SerialNumber = string.Empty;

            if (HidD_GetSerialNumberString(hHidFile, ref data[0], data.Length))
            {
                // Convert data bytes to unicode string
                var value = Encoding.Unicode.GetString(data);
                SerialNumber = value.Remove(value.IndexOf((char)0));

                return 1;
            }
            else
            {
                return 0;
            }
        }

        internal bool CT_HidD_GetInputReport(out byte[] Buffer)
        {
            Buffer = new byte[64];

            if (HidD_GetInputReport(hHidFile, ref Buffer[0], Buffer.Length))
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// Get a handle to the HID device
        /// </summary>
        /// <param name="hObject"></param>
        /// <returns></returns>
        internal bool CT_HidD_GetAttributes(IntPtr hObject)
        {
            // Create an instance of HIDD_ATTRIBUTES
            myHIDD_ATTRIBUTES = new HIDD_ATTRIBUTES();
            // Calculate its size
            myHIDD_ATTRIBUTES.Size = Marshal.SizeOf(myHIDD_ATTRIBUTES);

            return HidD_GetAttributes(
                    hObject,
                    ref myHIDD_ATTRIBUTES);
        }

        /// <summary>
        /// Gets a pointer to the preparsed data buffer
        /// </summary>
        /// <param name="hObject"></param>
        /// <param name="pPHIDP_PREPARSED_DATA"></param>
        /// <returns></returns>
        internal bool CT_HidD_GetPreparsedData(IntPtr hObject, ref IntPtr pPHIDP_PREPARSED_DATA)
        {
            return HidD_GetPreparsedData(hObject, ref pPHIDP_PREPARSED_DATA);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="HidDeviceObject"></param>
        /// <param name="lpReportBuffer"></param>
        /// <param name="ReportBufferLength"></param>
        /// <returns></returns>
        internal bool CT_HidD_SetOutputReport(IntPtr HidDeviceObject, ref byte[] lpReportBuffer, int ReportBufferLength)
        {
            return HidD_SetOutputReport(HidDeviceObject, lpReportBuffer, ReportBufferLength);
        }


        /// <summary>
        /// Gets the capabilities report
        /// </summary>
        /// <param name="pPreparsedData"></param>
        /// <returns></returns>
        internal bool CT_HidP_GetCaps(IntPtr pPreparsedData)
        {
            myHIDP_CAPS = new HIDP_CAPS();
            return HidP_GetCaps(
             pPreparsedData,
             ref myHIDP_CAPS);
        }


        /// <summary>
        /// Value Capabilities
        /// </summary>
        /// <param name="CalsCapsLength"></param>
        /// <param name="pPHIDP_PREPARSED_DATA"></param>
        /// <returns></returns>
        internal int CT_HidP_GetValueCaps(ref short CalsCapsLength, IntPtr pPHIDP_PREPARSED_DATA)
        {

            HIDP_REPORT_TYPE myType = 0;
            myHIDP_VALUE_CAPS = new HIDP_VALUE_CAPS[5];
            return HidP_GetValueCaps(
                (short)myType,
                myHIDP_VALUE_CAPS,
                ref CalsCapsLength,
                pPHIDP_PREPARSED_DATA);

        }

        /// <summary>
        /// Read Port
        /// </summary>
        /// <param name="InputReportByteLength"></param>
        /// <see cref="https://msdn.microsoft.com/en-us/library/windows/desktop/ms687032(v=vs.85).aspx"/>
        /// <returns></returns>
        internal byte[] CT_ReadFile(int InputReportByteLength)
        {
            var security = new SECURITY_ATTRIBUTES();
            IntPtr eventObject = CreateEvent(ref security, 0, 0, "");

            var overlapped = new NativeOverlapped()
            {
                EventHandle = eventObject
            };

            uint _byte_read = 0;
            byte[] buffer = null;

            IntPtr nonManagedBuffer = Marshal.AllocHGlobal(InputReportByteLength);

            bool ret = ReadFile(hHidFile, nonManagedBuffer, (uint)InputReportByteLength, out _byte_read, ref overlapped);


            /// the err should be 0x3E5
            /// which indicates the overlapped I/O operation is in progress
            /// <see cref="https://msdn.microsoft.com/en-us/library/ms681388(v=vs.85).aspx"/>
            int err = GetLastError();

            int evt = WaitForSingleObject(overlapped.EventHandle, 500); // wait for 500ms

            switch (evt)
            {
                case 0x0:  // the state of the specified object is signaled
                    buffer = new byte[InputReportByteLength];
                    Marshal.Copy(nonManagedBuffer, buffer, 0, buffer.Length);
                    break;

                case 0x102:  // The time-out interval elapsed, and the object's state is nonsignaled
                default:  // unsolved errors
                    buffer = null;
                    break;
            }

            Marshal.FreeHGlobal(nonManagedBuffer);

            return buffer;



            //if (ReadFile(hHidFile, nonManagedBuffer, (uint)buffer.Length, out BytesRead, ref overlapped))
            //{
            //    Marshal.Copy(nonManagedBuffer, buffer, 0, buffer.Length);
            //    Marshal.FreeHGlobal(nonManagedBuffer);
            //    return buffer;
            //}
            //else
            //{
            //    Marshal.FreeHGlobal(nonManagedBuffer);
            //    return null;
            //}

        }

        /// <summary>
        /// Write data to HID device
        /// </summary>
        /// <param name="hFile">HID device handle</param>
        /// <param name="lpBuffer">Buffer to write</param>
        /// <param name="nNumberOfBytesToWrite">How many bytes to write</param>
        /// <param name="lpNumberOfBytesWritten">How many bytes are written</param>
        /// <param name="lpOverlapped"></param>
        /// <returns></returns>
        internal bool CT_WriteFile(IntPtr hFile, ref byte lpBuffer, int nNumberOfBytesToWrite, ref int lpNumberOfBytesWritten, NativeOverlapped lpOverlapped)
        {
            return WriteFile(hFile, ref lpBuffer, nNumberOfBytesToWrite, ref lpNumberOfBytesWritten, ref lpOverlapped);
        }

        /// <summary>
        /// DestroyDeviceInfoList
        /// </summary>
        /// <returns></returns>
        internal int CT_SetupDiDestroyDeviceInfoList()
        {
            return SetupDiDestroyDeviceInfoList(hDevInfoSet);

        }

        /// <summary>
        /// FreePreparsedData
        /// </summary>
        /// <param name="pPHIDP_PREPARSED_DATA"></param>
        /// <returns></returns>
        internal int CT_HidD_FreePreparsedData(IntPtr pPHIDP_PREPARSED_DATA)
        {
            return SetupDiDestroyDeviceInfoList(pPHIDP_PREPARSED_DATA);
        }

        internal HidApiDeclarations HidApiDeclarations
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
            }
        }
    }
}
