using System;
using System.Collections.Generic;
using System.Linq;
using Irixi_Aligner_Common.Alignment.BaseClasses;
using Irixi_Aligner_Common.Classes.BaseClass;

namespace Irixi_Aligner_Common.Alignment.AlignmentXD
{
    public class AlignmentXDArgsProfile : AlignmentArgsPresetProfileBase
    {
        public string Instrument { get; set; }
        public string MotionComponent { get; set; }
        public double Target { get; set; }
        public int MaxCycles { get; set; }
        public int MaxOrder { get; set; }
        public int[] ListScanOrder { get; set; }

        [HashIgnore]
        public Alignment1DArgsProfile[] AxisGroup { get; set; }

       
        public override void FromArgsInstance(AlignmentArgsBase arg)
        {
            var targ = arg as AlignmentXDArgs;

            this.Instrument = targ.Instrument.HashString;
            this.MotionComponent = targ.MotionComponent.HashString;
            this.Target = targ.Target;
            this.MaxCycles = targ.MaxCycles;
            this.MaxOrder = targ.MaxOrder;
            this.ListScanOrder = targ.ListScanOrder.ToArray();

            List<Alignment1DArgsProfile> subaxis = new List<Alignment1DArgsProfile>();
            foreach(var item in targ.AxisParamCollection)
            {
                var p = new Alignment1DArgsProfile();
                p.FromArgsInstance(item);
                subaxis.Add(p);
            }
            this.AxisGroup = subaxis.ToArray();

            this.HashString = this.GetHashString();
        }

        public override void ToArgsInstance(AlignmentArgsBase arg)
        {
            var targ = arg as AlignmentXDArgs;
            targ.Instrument = targ.Service.MeasurementInstrumentCollection.FindItemByHashString(this.Instrument);
            ;
            targ.MotionComponent = targ.Service.LogicalMotionComponentCollection.FindItemByHashString(this.MotionComponent);
            targ.Target = this.Target;
            targ.MaxCycles = this.MaxCycles;

            foreach(var targ1d in targ.AxisParamCollection)
            {
                var axishash = targ1d.Axis.HashString;
                var profile = this.AxisGroup.Where(item => item.Axis == axishash).Select(item => { return item; }).First();
                profile.ToArgsInstance(targ1d);
            }
        }

        /// <summary>
        /// Calculate hash string by all the properties except the ones marked with HashIgnore
        /// </summary>
        /// <returns></returns>
        public override string GetHashString()
        {
            var str = base.GetHashString();
            
            // Note that hash string of each array element must be read manually and joined as a whole hash string
            str += String.Join(",", AxisGroup.Select(axis => 
            {
                return axis.HashString;

            }).ToArray());
            return HashGenerator.GetHashSHA256(str);
        }

        public override bool Validate()
        {
            return this.GetHashString() == this.HashString;
        }

    }
}
