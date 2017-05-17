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
        public Guid Class { get; set; }

        /// <summary>
        /// Get the name of the controller.
        /// this property is used to determine what type of class to be implemented as the instance of the controller
        /// </summary>
        public string Name
        {
            get
            {
                return _controller_name;
            }
            set
            {
                _controller_name = value;
                
                // determine the model of the controller according the controller name
                switch(_controller_name)
                {
                    case "Luminos P6A":
                        this.Model = MotionControllerModel.LUMINOS_P6A;
                        break;

                    case "Thorlabs TDC001":
                        this.Model = MotionControllerModel.THORLABS_TDC001;
                        break;

                    case "Irixi EE0017":
                        this.Model = MotionControllerModel.IRIXI_EE0017;
                        break;

                }
            }
        }

        public MotionControllerModel Model { get; set; }

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
