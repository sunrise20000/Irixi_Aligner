using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Irixi_Aligner_Common.Alignment.BaseClasses;
using Irixi_Aligner_Common.Alignment.Interfaces;
using Irixi_Aligner_Common.Classes.BaseClass;
using Newtonsoft.Json;

namespace Irixi_Aligner_Common.Alignment.AlignmentXD
{
    public class Alignment1DArgsProfile : AlignmentArgsPresetProfileBase
    {
        
        public string Axis { get; set; }
        public bool IsEnable { get; set; }
        public double Interval { get; set; }
        public double ScanRange { get; set; }
        public int ScanOrder { get; set; }


        public override void FromArgsInstance(AlignmentArgsBase arg)
        {
            var targ = arg as Alignment1DArgs;
            this.Axis = targ.Axis.HashString;
            this.IsEnable = targ.IsEnabled;
            this.Interval = targ.Interval;
            this.ScanRange = targ.ScanRange;
            this.ScanOrder = targ.ScanOrder;
            this.HashString = this.GetHashString();
        }


        public override void ToArgsInstance(AlignmentArgsBase arg)
        {
            var targ = arg as Alignment1DArgs;
            targ.IsEnabled = this.IsEnable;
            targ.Interval = this.Interval;
            targ.ScanRange = this.ScanRange;
            targ.ScanOrder = this.ScanOrder;
        }
    }
}
