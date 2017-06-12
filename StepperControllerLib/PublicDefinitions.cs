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

    public enum EnumGeneralOutputState
    {
        OFF = 0,
        ON
    }

    public enum EnumMoveMode
    {
        REL = 0,
        ABS
    }

    public enum EnumDirection
    {
        CW = 0,
        CCW
    }
    
    public static class PublicDefinitions
    {
        /// <summary>
        /// The lenght of HID Out Report
        /// </summary>
        public const int MAX_WRITEDATA_LEN = 64;
    }
}
