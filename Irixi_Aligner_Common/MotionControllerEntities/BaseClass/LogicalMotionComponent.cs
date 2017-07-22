using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Irixi_Aligner_Common.MotionControllerEntities.BaseClass
{

    public class LogicalMotionComponent
    {
        public LogicalMotionComponent(string DisplayName)
        {
            this.DisplayName = DisplayName;
            LogicalAxisCollection = new ObservableCollection<LogicalAxis>();
        }

        /// <summary>
        /// Get the component name displayed caption of document panel
        /// </summary>
        public string DisplayName { private set; get; }

   
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
