using System.Collections.ObjectModel;

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

        public override int GetHashCode()
        {
            int _hashcode = 0;
            foreach(var axis in LogicalAxisCollection)
            {
                _hashcode ^= axis.GetHashCode();
            }

            _hashcode ^= this.LogicalAxisCollection.Count.GetHashCode();
            return _hashcode;
        }

        public override string ToString()
        {
            return this.Caption;
        }
    }
}
