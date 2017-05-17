using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IrixiStepperControllerHelper
{
    public class ConnectionEventArgs : EventArgs
    {
        public enum EventType
        {
            /// <summary>
            /// The device reported the total axes it supports
            /// </summary>
            TotalAxesReturned,

            /// <summary>
            /// The connection has generated
            /// </summary>
            ConnectionSuccess,

            /// <summary>
            /// Retried to connect to the device
            /// </summary>
            ConnectionRetried,

            /// <summary>
            /// The device disconnected
            /// </summary>
            ConnectionLost
        }

        public ConnectionEventArgs(EventType Event, object Content)
        {
            this.Event = Event;
            this.Content = Content;
        }

        /// <summary>
        /// Get the event type
        /// </summary>
        public EventType Event { private set;  get;}

        /// <summary>
        /// Get the arguments of the event
        /// </summary>
        public object Content { private set; get; }
    }
}
