using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Irixi_Aligner_Common.Alignment.BaseClasses;
using Irixi_Aligner_Common.Classes;
using Irixi_Aligner_Common.Classes.BaseClass;
using Irixi_Aligner_Common.MotionControllers.Base;

namespace Irixi_Aligner_Common.Alignment.SnakeRouteScan
{
    public class SnakeRouteScanArgs: AlignmentArgsBase
    {
        #region Variables
        const string PROP_GRP_H = "Horizontal Settings";
        const string PROP_GRP_V = "Vertical Settings";

        LogicalAxis axis, axis2;
        double scanInterval = 1, axisRestriction = 100, axis2Restriction = 100;

        #endregion

        #region Constructors

        public SnakeRouteScanArgs(SystemService Service) : base(Service)
        {
            ScanCurve = new ScanCurve3D();
            ScanCurveGroup.Add(ScanCurve);

            AxisXTitle = "Horizontal";
            AxisYTitle = "Verical";
            AxisZTitle = "Intensity";

            Properties.Add(new Property("Instrument"));
            Properties.Add(new Property("Axis"));
            Properties.Add(new Property("AxisRestriction"));
            Properties.Add(new Property("Axis2"));
            Properties.Add(new Property("Axis2Restriction"));
            Properties.Add(new Property("ScanInterval"));
            Properties.Add(new Property("MoveSpeed"));

            this.PresetProfileManager =
                new AlignmentArgsPresetProfileManager<SnakeRouteScanArgs, SnakeRouteScanArgsProfile>(this);
        }

        #endregion

        #region Properties
        
        public override string SubPath
        {
            get
            {
                return "SnakeScan";
            }
        }

        [Browsable(false)]
        public AlignmentArgsPresetProfileManager<SnakeRouteScanArgs, SnakeRouteScanArgsProfile> PresetProfileManager
        {
            get;
        }

        [Browsable(false)]
        public ScanCurve3D ScanCurve
        {
            private set;
            get;
        }

        [Display(
            Name = "Axis",
            GroupName = PROP_GRP_H,
            Description = "The axis is the first one moved at the beginning of scan.")]
        public LogicalAxis Axis
        {
            get => axis;
            set
            {
                axis = value;
                RaisePropertyChanged();
            }
        }

        [Display(
            Name = "Range",
            GroupName = PROP_GRP_H,
            Description = "The scan range of horizontal axis.")]
        public double AxisRestriction
        {
            get => axisRestriction;
            set
            {
                axisRestriction = value;
                RaisePropertyChanged();
            }
        }

        [Display(
            Name = "Vertical",
            GroupName = PROP_GRP_V,
            Description = "The axis is the second one moved.")]
        public LogicalAxis Axis2
        {
            get => axis2;
            set
            {
                axis2 = value;
                RaisePropertyChanged();
            }
        }
        
        [Display(
            Name = "Range",
            GroupName = PROP_GRP_V,
            Description = "The scan range of vertical axis.")]
        public double Axis2Restriction
        {
            get => axis2Restriction;
            set
            {
                axis2Restriction = value;
                RaisePropertyChanged();
            }
        }

        [Display(
            Name = "Interval",
            GroupName = PROP_GRP_COMMON,
            Description = "")]
        public double ScanInterval
        {
            get => scanInterval;
            set
            {
                scanInterval = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Methods

        public override void Validate()
        {
            if(Axis == null)
                throw new ArgumentException("You must specify the horizontal axis.");

            if (Axis2 == null)
                throw new ArgumentException("You must specify the vertical axis.");

            if (Axis == Axis2)
                throw new ArgumentException("The horizontal axis and the vertical axis must be different.");

            if (Axis.PhysicalAxisInst.UnitHelper.Unit != Axis2.PhysicalAxisInst.UnitHelper.Unit)
                throw new ArgumentException("the two axes have different unit");
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
