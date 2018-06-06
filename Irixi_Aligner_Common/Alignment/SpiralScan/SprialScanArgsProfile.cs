using Irixi_Aligner_Common.Alignment.BaseClasses;
using Irixi_Aligner_Common.Alignment.SpiralScan;
using Irixi_Aligner_Common.MotionControllers.Base;

namespace Irixi_Aligner_Common.Alignment.SnakeRouteScan
{
    public class SprialScanArgsProfile : AlignmentArgsPresetProfileBase
    {
        public string Instrument { get; set; }
        public string Axis
        {
            get;
            set;
        }
        public string Axis2
        {
            get;
            set;
        }
        public double ScanRestriction
        {
            get;
            set;
        }
        public double ScanInterval
        {
            get;
            set;
        }
        public int MoveSpeed { get; set; }

        public override void FromArgsInstance(AlignmentArgsBase arg)
        {
            var targ = arg as SpiralScanArgs;

            this.Axis = targ.Axis.HashString;
            this.Axis2 = targ.Axis2.HashString;
            this.ScanRestriction = targ.ScanRestriction;
            this.ScanInterval = targ.ScanInterval;
            this.MoveSpeed = targ.MoveSpeed;
            this.Instrument = targ.Instrument.HashString;

            this.HashString = this.GetHashString();
        }

        public override void ToArgsInstance(AlignmentArgsBase arg)
        {
            var targ = arg as SpiralScanArgs;

            targ.Axis = targ.Service.FindLogicalAxisByHashString(this.Axis);
            targ.Axis2 = targ.Service.FindLogicalAxisByHashString(this.Axis2);
            targ.ScanRestriction = this.ScanRestriction;
            targ.ScanInterval = this.ScanInterval; ;
            targ.MoveSpeed = this.MoveSpeed;         
            targ.Instrument = targ.Service.FindInstrumentByHashString(this.Instrument);

        }
    }
}
