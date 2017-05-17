using Irixi_Aligner_Common.Configuration;
using System.Threading.Tasks;

namespace Irixi_Aligner_Common.Interfaces
{


    public interface IAxis
    {
        #region Properties
        /// <summary>
        /// Get or set the readable name of axis
        /// </summary>
        string AxisName { get; }

        /// <summary>
        /// Get the axis index in its mother controller
        /// </summary>
        int AxisIndex { get; }

        /// <summary>
        /// Get whether the axis is busy or not.
        /// if is busy, the operation to the axis is denied.
        /// </summary>
        bool IsBusy { get; }

        /// <summary>
        /// Get or set whether the axis could be operated manually or not
        /// </summary>
        bool IsManualEnabled { set; get; }

        /// <summary>
        /// Get or set whether the axis works on ABS mode
        /// </summary>
        bool IsAbsMode { set; get; }

        /// <summary>
        /// Get whether the axis is homed or not
        /// </summary>
        bool IsHomed { get; }

        /// <summary>
        /// Get or set whether the axis could be used or not
        /// </summary>
        bool IsEnabled { set; get; }

        /// <summary>
        /// Get the initial position after homed
        /// </summary>
        int InitPosition { get; }

        /// <summary>
        /// Get the absolute position
        /// </summary>
        int AbsPosition { set; get; }

        /// <summary>
        /// Get the relative position
        /// </summary>
        int RelPosition { get; }

        /// <summary>
        /// Get the maximum stroke the axis supports
        /// </summary>
        int MaxStroke { get; }

        /// <summary>
        /// Get or set the CW limitaion
        /// </summary>
        int CWL { set; get; }

        /// <summary>
        /// Get or set the CCW limitaion
        /// </summary>
        int CCWL { set; get; }

        /// <summary>
        /// Get or set the tag of axis
        /// </summary>
        object Tag { set; get; }

        /// <summary>
        /// Get distance per step, unit in nm
        /// </summary>
        int Dps { get; }

        /// <summary>
        /// Get the last error
        /// </summary>
        string LastError { set; get; }

        /// <summary>
        /// Get the parent motiong controller
        /// </summary>
        IMotionController ParentController { get; }
        #endregion


        #region Methods
        bool Lock();

        void Unlock();

        void SetParameters(int AxisIndex, ConfigPhysicalAxis Config, IMotionController Controller);

        Task<bool> Home();

        Task<bool> Move(MoveMode Mode, int Speed, int Distance);

        void Stop();

        void ToggleMoveMode();

        #endregion

    }
}
