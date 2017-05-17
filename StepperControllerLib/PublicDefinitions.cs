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
        HOME = 0,
        MOVE,
        MOVE_TRIG,
        MOVE_ADC,
        STOP
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
        /// The lenght of HID OUT Report to the device
        /// </summary>
        public const int MAX_WRITEDATA_LEN = 64;
    }
}
