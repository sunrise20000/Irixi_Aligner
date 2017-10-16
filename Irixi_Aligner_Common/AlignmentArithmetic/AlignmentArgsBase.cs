using GalaSoft.MvvmLight;
using System;

namespace Irixi_Aligner_Common.AlignmentArithmetic
{
    public class AlignmentArgsBase : ViewModelBase
    {
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
