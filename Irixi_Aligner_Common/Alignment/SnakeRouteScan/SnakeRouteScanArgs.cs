using System;
using System.ComponentModel.DataAnnotations;
using Irixi_Aligner_Common.Alignment.BaseClasses;
using Irixi_Aligner_Common.MotionControllers.Base;

namespace Irixi_Aligner_Common.Alignment.SnakeRouteScan
{
    public class SnakeRouteScanArgs: AlignmentArgsBase
    {
        #region Variables

        LogicalAxis axis, axis2;
        double targetPower = 100, scanInterval = 1, axisRestriction = 100, axis2Restriction = 100;

        #endregion

        #region Constructors

        public SnakeRouteScanArgs()
        {
            this.ScanCurve = new ScanCurve3D();
        }

        #endregion


        #region Properties

        public ScanCurve3D ScanCurve
        {
            private set;
            get;
        }

        [Display(Name="Axis", GroupName = "Alignment Params")]
        public LogicalAxis Axis
        {
            get => axis;
            set
            {
                axis = value;
                RaisePropertyChanged();
            }
        }

        public LogicalAxis Axis2
        {
            get => axis2;
            set
            {
                axis2 = value;
                RaisePropertyChanged();
            }
        }

        public double AxisRestriction
        {
            get => axisRestriction;
            set
            {
                axisRestriction = value;
                RaisePropertyChanged();
            }
        }

        public double Axis2Restriction
        {
            get => axis2Restriction;
            set
            {
                axis2Restriction = value;
                RaisePropertyChanged();
            }
        }

        public double ScanInterval
        {
            get => scanInterval;
            set
            {
                scanInterval = value;
                RaisePropertyChanged();
            }
        }


        public double TargetPower
        {
            get => targetPower;
            set
            {
                targetPower = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Methods

        public override void Validate()
        {
            base.Validate();

            if(Axis == Axis2)
                throw new ArgumentException("The two associated axes must be different.");
        }

        public override void ClearScanCurve()
        {
            this.ScanCurve.Clear();
        }

        public override void PauseInstruments()
        {
            Instrument.PauseAutoFetching();
        }

        public override void ResumeInstruments()
        {
            Instrument.ResumeAutoFetching();
        }

        #endregion

    }
}
