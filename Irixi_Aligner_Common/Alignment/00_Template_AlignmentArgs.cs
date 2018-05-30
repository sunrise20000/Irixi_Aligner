using Irixi_Aligner_Common.Alignment.BaseClasses;
using Irixi_Aligner_Common.Classes;
using System;
using System.ComponentModel;

namespace Irixi_Aligner_Common.Alignment
{ 
    public class _00_Template_AlignmentArgs : AlignmentArgsBase
    {
        #region Variables

        #endregion

        #region Constructors
        public _00_Template_AlignmentArgs(SystemService Service) : base(Service)
        {

        }
        #endregion

        #region Properties
        public override string SubPath => base.SubPath;

        [Browsable(false)]
        public AlignmentArgsPresetProfileManager<AlignmentArgsBase, AlignmentArgsPresetProfileBase> PresetProfileManager
        {
            get;
        }
        #endregion

        #region Methods
        /*
         * The following methods must be implemented !
         */

        public override void Validate()
        {
            throw new NotImplementedException();
        }

        public override void ClearScanCurve()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
