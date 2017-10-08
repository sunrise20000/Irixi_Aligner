using Irixi_Aligner_Common.Interfaces;
using Irixi_Aligner_Common.MotionControllerEntities.BaseClass;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Irixi_Aligner_Common.AlignmentArithmetic
{
    public class AlignmentXDArgs
    {
        public IMeasurementInstrument Instrument { set; get; }
        public LogicalMotionComponent MotionComponent { set; get; }
        public double Target { set; get; }
        public int MaxCycles { set; get; }
        public List<Alignment1DArgs> AxisParamCollection { set; get; }

        /// <summary>
        /// Check if the parameters are following the rule
        /// </summary>
        public void Validate()
        {

            if (Instrument == null)
                throw new ArgumentException("no instrument selected");

            if (Instrument.IsEnabled == false)
                throw new ArgumentException("the instrument is disabled");

            if (MotionComponent == null)
                throw new ArgumentException("no motion component selected");

            if(Target <= 0)
                throw new ArgumentException("the measurement target is not set");

            if(MaxCycles <= 0)
                throw new ArgumentException("the max cycles is not set");

            if (AxisParamCollection == null || AxisParamCollection.Count < 1)
                throw new ArgumentNullException("not axis param found");
            
            foreach(var arg in AxisParamCollection)
            {
                // check if the align order is unique
                if (arg.IsEnabled)
                {
                    if(arg.ScanRange <= 0)
                        throw new ArgumentException(string.Format("the range is error"));

                    if(arg.Interval <= 0)
                        throw new ArgumentException(string.Format("the step is error"));

                    if(arg.MoveSpeed < 0 || arg.MoveSpeed > 100)
                        throw new ArgumentException(string.Format("the speed is out of range"));
                }
            }


        }

    }
}
