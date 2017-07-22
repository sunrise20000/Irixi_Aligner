using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Irixi_Aligner_Common.Configuration
{
    public class ConfigLogicalMotionComponent
    {
        string _displayname = "undefined";
        /// <summary>
        /// Get the name of the aligner shown on the window which is defined in the json file
        /// </summary>
        public string DisplayName
        {
            set
            {
                _displayname = value;
            }
            get
            {
                return _displayname;
            }
        }

        /// <summary>
        /// Get the array of logical axis defined in the json file
        /// </summary>
        public ConfigLogicalAxis[] LogicalAxisArray { set; get; }

    }
}
