using GalaSoft.MvvmLight;
using Irixi_Aligner_Common.Interfaces;
using Irixi_Aligner_Common.MotionControllers.Base;
using System;

namespace Irixi_Aligner_Common.Alignment.Base
{
    public class AlignmentArgsBase : ViewModelBase
    {
        IInstrument instrument;
        LogicalMotionComponent motionComponent;

        public virtual IInstrument Instrument
        {
            get => instrument;
            set
            {
                instrument = value;
                RaisePropertyChanged();
            }
        }

        public LogicalMotionComponent MotionComponent
        {
            get => motionComponent;
            set
            {
                motionComponent = value;
                RaisePropertyChanged();
            }
        }

        public virtual void Validate()
        {
            throw new NotImplementedException();
        }

        public virtual void ClearScanCurve()
        {
            throw new NotImplementedException();
        }

        public virtual void PauseInstruments()
        {
            throw new NotImplementedException();
        }

        public virtual void ResumeInstruments()
        {
            throw new NotImplementedException();
        }
    }
}
