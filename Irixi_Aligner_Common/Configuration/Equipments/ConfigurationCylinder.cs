using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Irixi_Aligner_Common.Configuration
{
    public class ConfigurationCylinder : ConfigurationBase
    {
        public int PedalInput { get; set; }
        public int FiberClampOutput { get; set; }
        public int LensVacuumOutput { get; set; }
        public int PLCVacuumOutput { get; set; }
        public int PODVacuumOutput { get; set; }
    }
}
