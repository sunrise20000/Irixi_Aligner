using System;

namespace Irixi_Aligner_Common.Classes.BaseClass
{
    public class MoveByStepsArgs : EventArgs
    {
        public int Speed { get; set; }
        public int Steps { get; set; }
        public MoveMode Mode { get; set; }

        public override string ToString()
        {
            return string.Format("{0}/{1}/{2}", Mode, Speed, Steps);
        }
    }

    public class MoveByDistanceArgs :EventArgs
    {
        public int Speed { get; set; }
        public double Distance { get; set; }
        public MoveMode Mode { get; set; }

        public override string ToString()
        {
            return string.Format("{0}/{1}/{2}", Mode, Speed, Distance);
        }
    }
}
