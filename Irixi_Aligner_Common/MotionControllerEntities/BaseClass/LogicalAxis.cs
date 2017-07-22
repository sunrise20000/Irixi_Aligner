using Irixi_Aligner_Common.Classes;
using Irixi_Aligner_Common.Configuration;
using Irixi_Aligner_Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Irixi_Aligner_Common.MotionControllerEntities.BaseClass
{
    public class LogicalAxis
    {
        public LogicalAxis(SystemService Service, ConfigLogicalAxis Config, string ParentComponentName, int ID)
        {
            this.Service = Service;
            this.Config = Config;
            this.AxisName = Config.DisplayName;
            this.ParentName = ParentComponentName;
            this.ID = ID;
        }

        #region Properties
        /// <summary>
        /// Get the configuration of logical axis
        /// </summary>
        public ConfigLogicalAxis Config { private set; get; }

        /// <summary>
        /// Get the name display on the window
        /// </summary>
        public string AxisName { private set; get;}

        /// <summary>
        /// Get the index of the logical axis defined in the json file
        /// </summary>
        public int ID { private set; get; }

        /// <summary>
        /// Get the name of parent aliger
        /// </summary>
        public string ParentName { private set; get; }

        /// <summary>
        /// Get the instance of system service class
        /// </summary>
        public SystemService Service
        {
            private set;
            get;
        }

        /// <summary>
        /// Get the instance of physical axis
        /// </summary>
        public IAxis PhysicalAxisInst { get; set; }

        #endregion

        #region Overrided Methods
        public override string ToString()
        {
            return string.Format("*{0}@{1}*", Config.DisplayName, ParentName);
        }
        #endregion
    }
}
