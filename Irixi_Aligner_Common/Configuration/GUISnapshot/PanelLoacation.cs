using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Irixi_Aligner_Common.Configuration
{
    public class PanelLoacation
    {
        public Location VGrooveAligner { get; set; }
        public Location LensAligner { get; set; }
        public Location PODAligner { get; set; }
        public Location TopCameraBracket { get; set; }
        public Location AngularCameraBracket { get; set; }
        public Location FrontCameraBracket { get; set; }
        public Location CylinderControlPanel { get; set; }
    }
}
