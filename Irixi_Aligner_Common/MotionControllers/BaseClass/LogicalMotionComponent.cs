using System;
using System.Collections.ObjectModel;

namespace Irixi_Aligner_Common.MotionControllers.Base
{

    public class LogicalMotionComponent : ObservableCollection<LogicalAxis>
    {
        public LogicalMotionComponent(string Caption, string Icon, bool IsAligner)
        {
            this.Caption = Caption;
            this.Icon = Icon;
            this.IsAligner = IsAligner;
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
        /// Get whether the motion component is the alignment component
        /// </summary>
        public bool IsAligner { private set; get; }

        /// <summary>
        /// Generate the json file contains the preset position information
        /// </summary>
        /// <returns></returns>
        public string GetPresetPositionJsonString()
        {
            throw new NotImplementedException();
        }


        public override int GetHashCode()
        {
            int _hashcode = 0;
            foreach(var axis in this)
            {
                _hashcode ^= axis.GetHashCode();
            }

            _hashcode ^= this.Count.GetHashCode();
            return _hashcode;
        }

        public override string ToString()
        {
            return this.Caption;
        }
    }
}
