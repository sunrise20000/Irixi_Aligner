using Irixi_Aligner_Common.Alignment.Base;
using Irixi_Aligner_Common.Classes.BaseClass;
using Irixi_Aligner_Common.Interfaces;
using Irixi_Aligner_Common.MotionControllers.Base;
using System.Windows;
using System;

namespace Irixi_Aligner_Common.Alignment.Rotating
{
    public class RotatingScanArgs : AlignmentArgsBase
    {
        #region Variables

        LogicalAxis axisRotating, axisLinear;
        double targetPositionDifferentialOfMaxPower = 5, targetPosDiffChangeRate = 10, 
            gapRotating = 0.1, gapLinear = 1, 
            rangeRotating = 1, rangeLinear = 10;
        int moveSpeed = 100;

        //2 channel should be detected at the same time, so we need 2 keithley2400s
        IInstrument instrument2;

        #endregion

        #region Constructors
        
        public RotatingScanArgs():base()
        {
            ScanCurve = new ObservableCollectionThreadSafe<Point>();
            ScanCurve2 = new ObservableCollectionThreadSafe<Point>();
            PosDiffTrendCurve = new ObservableCollectionThreadSafe<Point>();
        }

        #endregion

        #region Properties

        public LogicalAxis AxisRotating
        {
            get => axisRotating;
            set
            {
                axisRotating = value;
                RaisePropertyChanged();
            }
        }

        public LogicalAxis AxisLinear
        {
            get => axisLinear;
            set
            {
                axisLinear = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// The secondary Keithley2400 is need
        /// </summary>
        public IInstrument Instrument2
        {
            get => instrument2;
            set
            {
                instrument2 = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// The target differential of the two positions at the max power of each channel
        /// </summary>
        public double TargetPositionDifferentialOfMaxPower
        {
            get => targetPositionDifferentialOfMaxPower;
            set
            {
                targetPositionDifferentialOfMaxPower = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// If the Pos. Diff. change rate is less than this value, exit alignment loop, 
        /// the value is in %.
        /// </summary>
        public double TargetPosDiffChangeRate
        {
            get => targetPosDiffChangeRate;
            set
            {
                targetPosDiffChangeRate = value;
                RaisePropertyChanged();
            }
        }

        public double GapRotating
        {
            get => gapRotating;
            set
            {
                gapRotating = value;
                RaisePropertyChanged();
            }
        }

        public double RangeRotating
        {
            get => rangeRotating;
            set
            {
                rangeRotating = value;
                RaisePropertyChanged();
            }
        }

        public double GapLinear
        {
            get => gapLinear;
            set
            {
                gapLinear = value;
                RaisePropertyChanged();
            }
        }

        public double RangeLinear
        {
            get => rangeLinear;
            set
            {
                rangeLinear = value;
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

        public ObservableCollectionThreadSafe<Point> ScanCurve { private set; get; }

        public ObservableCollectionThreadSafe<Point> ScanCurve2 { private set; get; }

        public ObservableCollectionThreadSafe<Point> PosDiffTrendCurve { private set; get; }

        #endregion

        #region Methods
        public override void Validate()
        {
            if (AxisLinear == AxisRotating)
                throw new ArgumentException("linear axis and rotating axis must be different.");

            if (MoveSpeed < 0 || MoveSpeed > 100)
                throw new ArgumentException("move speed must be between 1 ~ 100");
        }

        public override void ClearScanCurve()
        {
            ScanCurve.Clear();
            ScanCurve2.Clear();
        }

        public override void PauseInstruments()
        {
            Instrument.PauseAutoFetching();
            Instrument2.PauseAutoFetching();
        }

        public override void ResumeInstruments()
        {
            Instrument.ResumeAutoFetching();
            Instrument2.ResumeAutoFetching();
        }

        #endregion
    }
}
