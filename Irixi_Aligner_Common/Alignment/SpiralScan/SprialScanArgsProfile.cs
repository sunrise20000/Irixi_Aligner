﻿using Irixi_Aligner_Common.Alignment.BaseClasses;
using Irixi_Aligner_Common.Alignment.SpiralScan;

namespace Irixi_Aligner_Common.Alignment.SnakeRouteScan
{
    public class SprialScanArgsProfile : AlignmentArgsPresetProfileBase
    {
        public string Instrument { get; set; }
        public string Axis { get; set; }
        public string Axis2 { get; set; }
        public double ScanRange { get; set; }
        public double ScanRange2 { get; set; }
        public double Interval { get; set; }
        public int MoveSpeed { get; set; }


        public override void FromArgsInstance(AlignmentArgsBase arg)
        {
            var targ = arg as SpiralScanArgs;

            this.Axis = targ.Axis.HashString;
            this.Axis2 = targ.Axis2.HashString;
            //this.ScanRange = targ.AxisRestriction;
            //this.ScanRange2 = targ.Axis2Restriction;
            this.ScanRange = targ.ScanRestriction;
            this.ScanRange2 = targ.ScanRestriction;

            this.Interval = targ.ScanInterval;
            this.MoveSpeed = targ.MoveSpeed;

            this.Instrument = targ.Instrument.HashString;

            this.MoveSpeed = targ.MoveSpeed;

            this.HashString = this.GetHashString();
        }

        public override void ToArgsInstance(AlignmentArgsBase arg)
        {
            var targ = arg as SpiralScanArgs;

            targ.Axis = targ.Service.FindLogicalAxisByHashString(this.Axis);
            targ.Axis2 = targ.Service.FindLogicalAxisByHashString(this.Axis2);
            targ.ScanRestriction = this.ScanRange;
            //targ.Axis2Restriction = this.ScanRange2;
   
            targ.ScanInterval = this.Interval;
            targ.MoveSpeed = this.MoveSpeed;
            
            targ.Instrument = targ.Service.FindInstrumentByHashString(this.Instrument);

            targ.MoveSpeed = this.MoveSpeed;
        }
    }
}