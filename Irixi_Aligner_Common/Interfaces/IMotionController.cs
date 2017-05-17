using Irixi_Aligner_Common.Configuration;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Irixi_Aligner_Common.Interfaces
{

    public interface IMotionController
    {
        event EventHandler<string> OnErrorOccurred;
        event EventHandler<object> OnHomeCompleted;

        #region Properties

        /// <summary>
        /// Get the device class which makes this controller exclusively in the system.
        /// the controller could be located by the device class.
        /// </summary>
        Guid DevClass { get; }

        /// <summary>
        /// Get the model of this controller.
        /// </summary>
        MotionControllerModel Model { get; }

        /// <summary>
        /// Get the communication port of the controller.
        /// this might be serial port name, usb hid device serial number, etc.
        /// </summary>
        string Port { get;}

        /// <summary>
        ///  Get the name of the controller
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Get how many axes are moving.
        /// 0 indicates the motion controller is idle
        /// </summary>
        int RunningAxesSum { get; }
       
        /// <summary>
        /// Get whehter the controller is available or not
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Get whether the controller has been initialized
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Get the last error message
        /// </summary>
        string LastError { get; }

        /// <summary>
        /// Get or set the collection of axes
        /// </summary>
        ObservableCollection<IAxis> AxisCollection { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Initialize the controller
        /// </summary>
        /// <returns></returns>
        Task<bool> Init();

        /// <summary>
        /// <see cref="IAxis.Home"/> for more infomation
        /// </summary>
        /// <param name="Axis"></param>
        Task<bool> Home(IAxis Axis);

        /// <summary>
        ///  Home all axes
        /// </summary>
        Task<bool> HomeAll();

        /// <summary>
        /// <see cref="IAxis.Move(MoveMode, int, int)"/> for more infomation
        /// </summary>
        /// <param name="Axis">The instance of axis class inherited IAxis</param>
        /// <param name="Mode"></param>
        /// <param name="Speed"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        Task<bool> Move(IAxis Axis, MoveMode Mode, int Speed, int Distance);

        /// <summary>
        /// <see cref="IAxis.MoveWithTrigger(MoveMode, int, int, int, int)"/> for more infomation
        /// </summary>
        /// <param name="Axis">The instance of axis class inherited IAxis</param>
        /// <param name="Mode"></param>
        /// <param name="Speed"></param>
        /// <param name="Distance"></param>
        /// <param name="Interval"></param>
        /// <param name="Channel"></param>
        /// <returns></returns>
        Task<bool> MoveWithTrigger(IAxis Axis, MoveMode Mode, int Speed, int Distance, int Interval, int Channel);

        /// <summary>
        /// <see cref="IAxis.MoveWithInnerADC(MoveMode, int, int, int, int)"/> for more infomation
        /// </summary>
        /// <param name="Axis">The instance of axis class inherited IAxis</param>
        /// <param name="Mode"></param>
        /// <param name="Speed"></param>
        /// <param name="Distance"></param>
        /// <param name="Interval"></param>
        /// <param name="AdcIndex"></param>
        /// <returns></returns>
        Task<bool> MoveWithInnerADC(IAxis Axis, MoveMode Mode, int Speed, int Distance, int Interval, int Channel);

        /// <summary>
        /// <see cref="IAxis.Stop"/> for more infomation
        /// </summary>
        void Stop();

        /// <summary>
        /// Increase the property of RunningAxesSum
        /// </summary>
        void IncreaseRunningAxes();

        /// <summary>
        /// Decrease the property of RunningAxesSum
        /// </summary>
        void DecreaseRunningAxes();

        /// <summary>
        /// Find the axis with the specified name
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        IAxis FindAxisByName(string Name);

        #endregion
    }
}
