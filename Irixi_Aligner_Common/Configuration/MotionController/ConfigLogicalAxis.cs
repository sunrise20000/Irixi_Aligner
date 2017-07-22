using Irixi_Aligner_Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Irixi_Aligner_Common.Configuration
{
    public class ConfigLogicalAxis
    {
        /// <summary>
        /// Determine which motion contoller is used
        /// </summary>
        public Guid DeviceClass { get; set; }

        /// <summary>
        /// Determine the axis name used in the specified motion controller
        /// </summary>
        public string AxisName { get; set; }

        /// <summary>
        /// Get the name displayed on the window
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Get the order of home action
        /// </summary>
        public int HomeOrder { get; set; }
    }
}
