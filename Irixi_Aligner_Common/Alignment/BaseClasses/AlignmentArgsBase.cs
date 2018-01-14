using GalaSoft.MvvmLight;
using Irixi_Aligner_Common.Classes.BaseClass;
using Irixi_Aligner_Common.Interfaces;
using Irixi_Aligner_Common.MotionControllers.Base;
using System;

namespace Irixi_Aligner_Common.Alignment.BaseClasses
{
    public class AlignmentArgsBase : ViewModelBase
    {
        IInstrument instrument;
        LogicalMotionComponent motionComponent;
        int moveSpeed;

        public AlignmentArgsBase()
        {
            Log = new ObservableCollectionThreadSafe<string>();
        }


        public ObservableCollectionThreadSafe<string> Log
        {
            get;
        }

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

        public int MoveSpeed
        {
            get => moveSpeed;
            set
            {
                moveSpeed = value;
                RaisePropertyChanged();
            }
        }
        

        public virtual void Validate()
        {
            if(MoveSpeed < 1 || MoveSpeed > 100)
                throw new ArgumentException("move speed must be between 1 ~ 100");
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
