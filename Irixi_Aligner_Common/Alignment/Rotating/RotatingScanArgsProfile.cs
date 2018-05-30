using Irixi_Aligner_Common.Alignment.BaseClasses;

namespace Irixi_Aligner_Common.Alignment.Rotating
{
    public class RotatingScanArgsProfile : AlignmentArgsPresetProfileBase
    {
        public string Instrument { get; set; }
        public string Instrument2 { get; set; }
        public string AxisRotating { get; set; }
        public string AxisLinear { get; set; }
        public double LinearInterval { get; set; }
        public double LinearScanRange { get; set; }
        public double Pitch { get; set; }

        public int MoveSpeed { get; set; }

        public override void FromArgsInstance(AlignmentArgsBase arg)
        {
            var targ = arg as RotatingScanArgs;

            this.AxisRotating = targ.AxisRotating.HashString;
            this.AxisLinear = targ.AxisLinear.HashString;
            this.LinearInterval = targ.LinearInterval;
            this.LinearScanRange = targ.LinearRestriction;
            this.Pitch = targ.Pitch;

            this.Instrument = targ.Instrument.HashString;
            this.Instrument2 = targ.Instrument2.HashString;

            this.HashString = this.GetHashString();
        }

        public override void ToArgsInstance(AlignmentArgsBase arg)
        {
            var targ = arg as RotatingScanArgs;

            targ.AxisRotating = targ.Service.FindLogicalAxisByHashString(this.AxisRotating);
            targ.AxisLinear = targ.Service.FindLogicalAxisByHashString(this.AxisLinear);
            targ.LinearInterval = this.LinearInterval;
            targ.LinearRestriction = this.LinearScanRange;
            targ.Pitch = this.Pitch;
           

            targ.Instrument = targ.Service.FindInstrumentByHashString(this.Instrument);
            targ.Instrument2 = targ.Service.FindInstrumentByHashString(this.Instrument2);
        }
    }
}
