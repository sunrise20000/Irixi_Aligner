using Irixi_Aligner_Common.Alignment.BaseClasses;

namespace Irixi_Aligner_Common.Alignment.CentralAlign
{
    public class CentralAlignArgsProfile : AlignmentArgsPresetProfileBase
    {
        public string HorizontalAxis { get; set; }
        public double HorizontalInterval { get; set; }
        public double HorizontalRange { get; set; }

        public string VerticalAxis { get; set; }
        public double VerticalInterval { get; set; }
        public double VerticalRange { get; set; }

        public string Instrument { get; set; }
        public string Instrument2 { get; set; }

        public int MoveSpeed { get; set; }

        public override void FromArgsInstance(AlignmentArgsBase arg)
        {
            var targ = arg as CentralAlignArgs;

            this.HorizontalAxis = targ.Axis.HashString;
            this.HorizontalInterval = targ.AxisRestriction;
            this.HorizontalRange = targ.ScanIntervalHorizontal;
            this.VerticalAxis = targ.Axis2.HashString;
            this.VerticalInterval = targ.Axis2Restriction;
            this.VerticalRange = targ.ScanIntervalVertical;

            this.Instrument = targ.Instrument.HashString;
            this.Instrument2 = targ.Instrument2.HashString;

            this.MoveSpeed = targ.MoveSpeed;

            this.HashString = this.GetHashString();
        }

        public override void ToArgsInstance(AlignmentArgsBase arg)
        {
            var targ = arg as CentralAlignArgs;

            targ.Axis = targ.Service.FindLogicalAxisByHashString(this.HorizontalAxis);
            targ.AxisRestriction = this.HorizontalRange;
            targ.ScanIntervalHorizontal = this.HorizontalInterval;

            targ.Axis2 = targ.Service.FindLogicalAxisByHashString(this.VerticalAxis);
            targ.Axis2Restriction = this.VerticalRange;
            targ.ScanIntervalVertical = this.VerticalInterval;

            targ.Instrument = targ.Service.FindInstrumentByHashString(this.Instrument);
            targ.Instrument2 = targ.Service.FindInstrumentByHashString(this.Instrument2);

            targ.MoveSpeed = this.MoveSpeed;
        }
    }
}
