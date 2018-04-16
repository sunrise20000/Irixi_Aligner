using System;

namespace Irixi_Aligner_Common.Configuration.MotionController
{
    public class ConfigLogicalAxis
    {
        /// <summary>
        /// Get or set which motion contoller is used
        /// </summary>
        public Guid DeviceClass { get; set; }

        /// <summary>
        /// Get or set the physical axis name
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
