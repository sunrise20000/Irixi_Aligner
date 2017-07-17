using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Irixi_Aligner_Common.Configuration
{
    public class PanelLayout
    {
        public Layout VGrooveAligner { get; set; }
        public Layout LensAligner { get; set; }
        public Layout PODAligner { get; set; }
        public Layout TopCameraBracket { get; set; }
        public Layout AngularCameraBracket { get; set; }
        public Layout FrontCameraBracket { get; set; }
        public Layout CylinderControlPanel { get; set; }
    }
}
