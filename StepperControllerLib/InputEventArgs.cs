using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IrixiStepperControllerHelper
{
    public class InputEventArgs : EventArgs
    {
        public InputEventArgs(int Channel, InputState State)
        {
            this.Channel = Channel;
            this.State = State;
        }

        /// <summary>
        /// Get which input channel has changed
        /// for 3-axis controller, this should be 0 - 7; for 1-axis controller, this should be 0 - 1
        /// </summary>
        public int Channel
        {
            private set;
            get;
        }

        /// <summary>
        /// Get the state of input
        /// for 3-axis controller, this should be 0 - 7; for 1-axis controller, this should be 0 - 1
        /// </summary>
        public InputState State
        {
            private set;
            get;
        }
    }
}
