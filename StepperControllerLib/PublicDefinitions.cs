namespace IrixiStepperControllerHelper
{
    public enum EnumAxis
    {
        X = 0,
        Y,
        Z
    }

    public enum EnumCommand
    {
        /// <summary>
        /// Home the axis
        /// </summary>
        HOME = 0,

        /// <summary>
        /// Move the axis
        /// </summary>
        MOVE,

        /// <summary>
        /// Move the axis and generate trigger out pulse with the specified interval
        /// </summary>
        MOVE_TRIG,

        /// <summary>
        /// Move the axis and trigger the ADC conversion with the specified interval 
        /// </summary>
        MOVE_ADC,

        /// <summary>
        /// Stop moving immediately
        /// </summary>
        STOP,

        /// <summary>
        /// Set the state of the general output port
        /// </summary>
        GENOUT = 10
    }

    public enum MoveMode
    {
        REL = 0,
        ABS
    }

    public enum Direction
    {
        CW = 0,
        CCW
    }


    /// <summary>
    /// Input State
    /// </summary>
    public enum InputState
    {
        Triggered,
        Untriggered
    }

    /// <summary>
    /// Output State
    /// </summary>
    public enum OutputState
    {
        // The order can not be changed!
        // According the hid protocol, write 0 to controller, the output port will be switched OFF, for 1 it's ON.
        Disabled = 0,
        Enabled = 1
        
    }

    public static class PublicDefinitions
    {
        /// <summary>
        /// The lenght of HID Out Report
        /// </summary>
        public const int MAX_WRITEDATA_LEN = 64;
    }
}
