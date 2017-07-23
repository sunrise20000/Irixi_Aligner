using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Irixi_Aligner_Common.MotionControllerEntities.BaseClass
{

    public class LogicalMotionComponent
    {
        public LogicalMotionComponent(string Caption, string Icon)
        {
            this.Caption = Caption;
            this.Icon = Icon;
            LogicalAxisCollection = new ObservableCollection<LogicalAxis>();
        }

        /// <summary>
        /// Get the component name displayed caption of document panel
        /// </summary>
        public string Caption { private set; get; }

        /// <summary>
        /// Get the icon shown on the caption and the show/hide button
        /// </summary>
        public string Icon { private set; get; }
   
        /// <summary>
        /// Get the collection contains logical axes, 
        /// these axes are defined in the json file.
        /// </summary>
        public ObservableCollection<LogicalAxis> LogicalAxisCollection
        {
            private set;
            get;
        }
    }
}
