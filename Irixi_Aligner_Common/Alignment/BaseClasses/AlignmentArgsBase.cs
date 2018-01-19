using GalaSoft.MvvmLight;
using Irixi_Aligner_Common.Classes.BaseClass;
using Irixi_Aligner_Common.Interfaces;
using Irixi_Aligner_Common.MotionControllers.Base;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Irixi_Aligner_Common.Alignment.BaseClasses
{
    public class AlignmentArgsBase : ViewModelBase
    {
        #region Variables
        protected const string PROP_GRP_COMMON = "Common";
        protected const string PROP_GRP_TARGET = "Goal";

        IInstrument instrument;
        LogicalMotionComponent motionComponent;
        int moveSpeed = 100;

        #endregion

        #region Constructors
        
        public AlignmentArgsBase()
        {
            Log = new ObservableCollectionThreadSafe<string>();
            ScanCurveGroup = new ScanCurveGroup();
        }

        #endregion

        #region Properties

        [Browsable(false)]
        public ScanCurveGroup ScanCurveGroup { private set; get; }

        [Browsable(false)]
        public ObservableCollectionThreadSafe<string> Log
        {
            get;
        }

        [Display(AutoGenerateField = true, Name = "Instrument", GroupName = PROP_GRP_COMMON, Description = "The valid instrument like powermeter, keithley 2400, etc.")]
        public virtual IInstrument Instrument
        {
            get => instrument;
            set
            {
                instrument = value;
                RaisePropertyChanged();
            }
        }

        [Display(AutoGenerateField = true, Name = "Motion Component", GroupName = PROP_GRP_COMMON, Description = "Which motion component belongs to of the axes to align.")]
        public LogicalMotionComponent MotionComponent
        {
            get => motionComponent;
            set
            {
                motionComponent = value;
                RaisePropertyChanged();
            }
        }

        [Display(AutoGenerateField = true, Name = "Move Speed(%)", GroupName = PROP_GRP_COMMON, Description = "The move speed while aligning which is in %, the range is 1 - 100.")]
        public int MoveSpeed
        {
            get => moveSpeed;
            set
            {
                moveSpeed = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Validate the parameters
        /// </summary>
        public virtual void Validate()
        {
            if(MoveSpeed < 1 || MoveSpeed > 100)
                throw new ArgumentException("move speed must be between 1 ~ 100");
        }

        /// <summary>
        /// Clear the previous scan curve, it must be implemented in the inheritance class
        /// </summary>
        public virtual void ClearScanCurve()
        {
            this.ScanCurveGroup.ClearCurvesContent();
        }

        /// <summary>
        /// Pause the feedback instruments, due to the software reads the instruments continuously, the communication port
        /// is occupied, so the reading loop should be halted while alignment process reading the instruments
        /// </summary>
        public virtual void PauseInstruments()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Release the instruments
        /// <see cref="PauseInstruments"/>
        /// </summary>
        public virtual void ResumeInstruments()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
