using Irixi_Aligner_Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Irixi_Aligner_Common.Configuration
{
    public class ConfigPhysicalMotionController
    {
        string _controller_name = "";

        /// <summary>
        /// Get the guid that makes each controller exclusively
        /// </summary>
        public Guid DeviceClass { get; set; }

        /// <summary>
        /// Get the model of controller defined by ControllerType
        /// </summary>
        public ControllerType Model { get; set; }

        /// <summary>
        /// Get the communication port of the controller.
        /// this might be serial port name, usb hid device serial number, etc.
        /// </summary>
        public string Port { get; set; }

        /// <summary>
        /// Determine wether the controller is enabled or not.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Get the comment of the controller definition.
        /// normally it used in the JSON file
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Get the collection of the axes controled by the controller
        /// </summary>
        public ConfigPhysicalAxis[] AxisCollection { get; set; }
    }
}
