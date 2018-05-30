using Irixi_Aligner_Common.Alignment.BaseClasses;
using Irixi_Aligner_Common.Classes;
using Irixi_Aligner_Common.Classes.BaseClass;
using Irixi_Aligner_Common.MotionControllers.Base;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media.Media3D;

namespace Irixi_Aligner_Common.Alignment.SpiralScan
{
    public class SpiralScanArgs : AlignmentArgsBase
    {
        #region Variables
        const string PROP_GRP_AXIS = "Axis Settings";

        private LogicalAxis axis, axis2;
        private double scanInterval, scanRestriction;
        #endregion

        #region Constructors

        public SpiralScanArgs(SystemService Service) : base(Service)
        {
            ScanCurve = new ScanCurve3D();
            this.ScanCurveGroup.Add(ScanCurve);

            AxisXTitle = "Horizontal";
            AxisYTitle = "Verical";
            AxisZTitle = "Intensity";

            Properties.Add(new Property("Instrument"));
            Properties.Add(new Property("Axis"));
            Properties.Add(new Property("Axis2"));
            Properties.Add(new Property("ScanInterval"));
            Properties.Add(new Property("ScanRestriction"));
            Properties.Add(new Property("MoveSpeed"));
        }

        #endregion

        #region Properties

        [Display(
            Name = "H Axis",
            GroupName = PROP_GRP_AXIS,
            Description = "The scan process starts along the horizontal axis first.")]
        public LogicalAxis Axis
        {
            get => axis;
            set
            {
                axis = value;
                RaisePropertyChanged();
            }
        }

        [Browsable(false)]
        public string AxisHashString
        {
            private get; set;
        }

        [Display(
            Name = "V Axis",
            GroupName = PROP_GRP_AXIS,
            Description = "The scan process starts along the horizontal axis first.")]
        public LogicalAxis Axis2
        {
            get => axis2;
            set
            {
                axis2 = value;
                RaisePropertyChanged();
            }
        }

        [Browsable(false)]
        public string Axis2HashString
        {
            private get; set;
        }

        [Display(
            Name = "Interval",
            GroupName = PROP_GRP_AXIS,
            Description = "The scan interval for both H-Axis and V-Axis.")]
        public double ScanInterval
        {
            get => scanInterval;
            set
            {
                scanInterval = value;
                RaisePropertyChanged();
            }
        }

        [Display(
            Name = "Restriction",
            GroupName = PROP_GRP_AXIS,
            Description = "The scan scope restriction.")]
        public double ScanRestriction
        {
            get => scanRestriction;
            set
            {
                scanRestriction = value;
                RaisePropertyChanged();
            }
        }
        
        [Browsable(false)]
        public ScanCurve3D ScanCurve
        {
            get;
        }

        #endregion

        #region Methods
        public override void Validate()
        {
            base.Validate();

            if (Axis == null)
                throw new ArgumentException("You must specify the horizontal axis.");

            if (Axis2 == null)
                throw new ArgumentException("You must specify the vertical axis.");

            if (Axis.Equals(Axis2))
                throw new ArgumentException("the two axes must be different");

            if (Axis.PhysicalAxisInst.UnitHelper.Unit != Axis2.PhysicalAxisInst.UnitHelper.Unit)
                throw new ArgumentException("the two axes have different unit");
        }

        public override void ClearScanCurve()
        {
            ScanCurve.Clear();
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
