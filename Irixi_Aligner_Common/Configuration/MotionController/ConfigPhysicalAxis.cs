using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Irixi_Aligner_Common.Configuration
{
    public class ConfigPhysicalAxis
    {
        /// <summary>
        /// Get the name of the axis
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Indicates wether the axis is enabled or not
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Get the initial position after homing
        /// </summary>
        public int PosHomed { get; set; }

        /// <summary>
        /// Get the maximum distance that the axis supports
        /// </summary>
        public int PosMax { get; set; }

        /// <summary>
        /// Get the CW limitaion
        /// normally this value indicates how far the mounting block could move from the mechanical zero point
        /// </summary>
        public int PosCWL { get; set; }

        /// <summary>
        ///  Get the CCW limitaion
        ///  normally this value indicates how close the mounting block could be to the mechanical zero point 
        /// </summary>
        public int PosCCWL { get; set; }

        /// <summary>
        /// Get the distance per step.
        /// the unit should be nm
        /// </summary>
        public int Dps { get; set; }
    }
}
