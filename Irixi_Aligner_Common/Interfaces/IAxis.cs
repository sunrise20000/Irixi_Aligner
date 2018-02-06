using Irixi_Aligner_Common.Classes.BaseClass;
using Irixi_Aligner_Common.Configuration;
using Irixi_Aligner_Common.Configuration.MotionController;
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
        /// Indicates that whether the axis is used to align the components
        /// </summary>
        bool IsAligner { get; }

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
        /// Get the absolute position in steps
        /// </summary>
        int AbsPosition { set; get; }

        /// <summary>
        /// Get the relative position in steps
        /// </summary>
        int RelPosition { get; }

        /// <summary>
        /// Get the maximum driven speed
        /// </summary>
        int MaxSpeed { get; }

        /// <summary>
        /// Get how many steps are used to accelerate to the max speed
        /// </summary>
        int AccelerationSteps { get; }

        /// <summary>
        /// Get or set the tag of axis
        /// </summary>
        object Tag { set; get; }

        /// <summary>
        /// Get the unithelper to convert position in step to position to real-world unit
        /// </summary>
        RealworldPositionManager UnitHelper { get; }

        /// <summary>
        /// Get the last error
        /// </summary>
        string LastError { set; get; }

        /// <summary>
        /// Get the parent motiong controller
        /// </summary>
        IMotionController Parent { get; }
        #endregion

        #region Methods
        /// <summary>
        /// Lock the axis before any operation
        /// </summary>
        /// <returns></returns>
        bool Lock();

        /// <summary>
        /// Unlock the axis after any operation
        /// </summary>
        void Unlock();

        /// <summary>
        /// Set the parameters once the class is instantiated
        /// </summary>
        /// <param name="AxisIndex"></param>
        /// <param name="Config"></param>
        /// <param name="Controller"></param>
        void SetParameters(int AxisIndex, ConfigPhysicalAxis Config, IMotionController Controller);

        /// <summary>
        /// Start a task to home the axis
        /// </summary>
        /// <returns></returns>
        bool Home();

        /// <summary>
        /// Start to move the axis by real world distance
        /// </summary>
        /// <param name="Mode">ABS/REL</param>
        /// <param name="Speed">1 ~ 100</param>
        /// <param name="Distance">The real world distance to move</param>
        /// <returns></returns>
        bool Move(MoveMode Mode, int Speed, double Distance);

        /// <summary>
        /// Start a task to move the axis and output a series of trigger pulses on the specified channel(I/O)
        /// </summary>
        /// <param name="Mode">ABS/REL</param>
        /// <param name="Speed">1 ~ 100</param>
        /// <param name="Distance">The real world distance to move</param>
        /// <param name="Interval">The distance between adjacent trigger pulse</param>
        /// <param name="Channel">The trigger channel</param>
        /// <returns></returns>
        bool MoveWithTrigger(MoveMode Mode, int Speed, double Distance, double Interval, int Channel);

        /// <summary>
        /// Start a task to move the axis and activate a series of conversion with the specified adc
        /// </summary>
        /// <param name="Mode">ABS/REL</param>
        /// <param name="Speed">1 ~ 100</param>
        /// <param name="Distance">The real world distance to move</param>
        /// <param name="Interval">The distance between adjacent ADC conversion</param>
        /// <param name="AdcIndex">The channle of ADC</param>
        /// <returns></returns>
        bool MoveWithInnerADC(MoveMode Mode, int Speed, double Distance, double Interval, int Channel);

        /// <summary>
        /// Stop the current activity immediately 
        /// </summary>
        void Stop();

        /// <summary>
        /// Toggle the move mode between ABS and REL
        /// </summary>
        void ToggleMoveMode();

        /// <summary>
        /// Set the relative position to 0
        /// </summary>
        void ClearRelPosition();

        #endregion

    }
}
