using System;
using System.Collections.ObjectModel;
using System.Text;
using Irixi_Aligner_Common.Classes.BaseClass;
using Irixi_Aligner_Common.Interfaces;

namespace Irixi_Aligner_Common.MotionControllers.Base
{

    public class LogicalMotionComponent : ObservableCollection<LogicalAxis>, IHashable
    {
        #region Constructors

        public LogicalMotionComponent(string Caption, string Icon, bool IsAligner)
        {
            this.Caption = Caption;
            this.Icon = Icon;
            this.IsAligner = IsAligner;
        }

        #endregion
        
        #region Properties

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

        #endregion

        #region Methods

        /// <summary>
        /// Move a set of axes
        /// </summary>
        /// <returns></returns>
        public void MoveToPresetPosition(MassMoveArgs Args)
        {
            throw new NotImplementedException();
        }

        public string GetHashString()
        {
            StringBuilder factor = new StringBuilder();

            foreach(var axis in this)
            {
                factor.Append(axis.GetHashString());
            }

            return HashGenerator.GetHashSHA256(factor.ToString());
        }

        public override string ToString()
        {
            return this.Caption;
        } 

        #endregion
    }
}
