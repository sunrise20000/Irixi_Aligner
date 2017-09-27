using Irixi_Aligner_Common.MotionControllerEntities.BaseClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Irixi_Aligner_Common.AlignmentArithmetic
{
    public class Align1DArgs
    {
        public LogicalAxis Axis { set; get; }
        public int MoveSpeed { set; get; }
        public double Interval { set; get; }
        public double ScanRange { set; get; }
    }
}
