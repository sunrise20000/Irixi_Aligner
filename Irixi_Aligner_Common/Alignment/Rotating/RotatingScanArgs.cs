using Irixi_Aligner_Common.Alignment.BaseClasses;
using Irixi_Aligner_Common.Equipments.Base;
using Irixi_Aligner_Common.Interfaces;
using Irixi_Aligner_Common.MotionControllers.Base;
using System;
using System.Linq;

namespace Irixi_Aligner_Common.Alignment.Rotating
{
    public class RotatingScanArgs : AlignmentArgsBase
    {
        #region Variables

        LogicalAxis axisRotating, axisLinear;
        double targetPositionDifferentialOfMaxPower = 5, targetPosDiffChangeRate = 10, 
            gapRotating = 1, gapLinear = 1, 
            rangeRotating = 1, rangeLinear = 100, lenghtOfChannelStartToEnd = 750;
        int moveSpeed = 100;

        //2 channel should be detected at the same time, so we need 2 keithley2400s
        IInstrument instrument, instrument2;

        #endregion

        #region Constructors
        
        public RotatingScanArgs():base()
        {
            ScanCurveGroup = new ScanCurveGroup();

            ScanCurve = new ScanCurve();
            ScanCurve2 = new ScanCurve();
            ScanCurveFitting = new ScanCurve();
            ScanCurveFitting2 = new ScanCurve();

            DeltaPositionTrendCurve = new ScanCurve();

            // add the curves to the group
            ScanCurveGroup.Add(ScanCurve);
            ScanCurveGroup.Add(ScanCurve2);
            ScanCurveGroup.Add(ScanCurveFitting);
            ScanCurveGroup.Add(ScanCurveFitting2);
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

        new public IInstrument Instrument
        {
            get => instrument;
            set
            {
                instrument = value;
                RaisePropertyChanged();

                this.ScanCurve.DisplayName = ((InstrumentBase)instrument).Config.Caption;
            }
        }

        /// <summary>
        /// The instrument to detmonitor the secondary channel
        /// </summary>
        public IInstrument Instrument2
        {
            get => instrument2;
            set
            {
                instrument2 = value;
                RaisePropertyChanged();

                this.ScanCurve2.DisplayName = ((InstrumentBase)instrument2).Config.Caption;
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
        

        public double LengthOfChannelStartToEnd
        {
            get => lenghtOfChannelStartToEnd;
            set
            {
                lenghtOfChannelStartToEnd = value;
                RaisePropertyChanged();
            }
        }

        public ScanCurveGroup ScanCurveGroup { private set; get; }

        /// <summary>
        /// Scan curve of instrument
        /// </summary>
        public ScanCurve ScanCurve { private set; get; }

        /// <summary>
        /// Scan curve of instrument2
        /// </summary>
        public ScanCurve ScanCurve2 { private set; get; }

        /// <summary>
        /// The fitting curve of scan curve
        /// </summary>
        public ScanCurve ScanCurveFitting { private set; get; }

        /// <summary>
        /// The fitting curve of scan curve 2
        /// </summary>
        public ScanCurve ScanCurveFitting2 { private set; get; }

        /// <summary>
        /// Scan curve of delta position of max optical power of 2 channels
        /// </summary>
        public ScanCurve DeltaPositionTrendCurve { private set; get; }

        #endregion

        #region Methods
        public override void Validate()
        {
            base.Validate();

            if (AxisLinear == AxisRotating)
                throw new ArgumentException("linear axis and rotating axis must be different.");

            if(Instrument == Instrument2)
                throw new ArgumentException("the 2 instruments must be different.");
        }

        public override void ClearScanCurve()
        {
            this.ScanCurveGroup.ClearCurvesContent();
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

        /// <summary>
        /// calculate the fitting equation and draw the fitting curve
        /// </summary>
        /// <param name="Curve"></param>
        public void BeautifyScanCurves()
        {
            var points = ScanCurve.GetBeautifiedCurve();
            foreach (var p in points)
                ScanCurveFitting.Add(p);

            points = ScanCurve2.GetBeautifiedCurve();
            foreach (var p in points)
                ScanCurveFitting2.Add(p);
        }

        #endregion
    }
}
