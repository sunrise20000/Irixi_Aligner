using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Irixi_Aligner_Common.Configuration
{
    
    public class ConfigurationMotionController
    {
        /// <summary>
        /// Get wether record the log or not
        /// </summary>
        public bool LogEnabled { get; set; }


        public ConfigurationCylinder Cylinder { get; set; }

        /// <summary>
        /// Layout of physical motion controllers
        /// </summary>
        public ConfigPhysicalMotionController[] PhysicalMotionControllers { get; set; }

        /// <summary>
        /// Logical layout of the aligner
        /// </summary>
        public ConfigLogicalMotionComponent[] LogicalMotionComponents{ get; set; }

       
    }
}
