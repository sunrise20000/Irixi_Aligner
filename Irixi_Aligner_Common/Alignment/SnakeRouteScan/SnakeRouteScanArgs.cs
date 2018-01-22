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
        const string PROP_GRP_AXIS = "Axis Settings";

        LogicalAxis axis, axis2;
        double scanInterval = 1, axisRestriction = 100, axis2Restriction = 100;

        #endregion

        #region Constructors

        public SnakeRouteScanArgs(SystemService Service) : base(Service)
        {
            this.ScanCurve = new ScanCurve3D();

            AxisXTitle = "Horizontal";
            AxisYTitle = "Verical";
            AxisZTitle = "Power";

            Properties.Add(new Property("Instrument"));
            Properties.Add(new Property("Axis"));
            Properties.Add(new Property("AxisRestriction"));
            Properties.Add(new Property("Axis2"));
            Properties.Add(new Property("Axis2Restriction"));
            Properties.Add(new Property("ScanInterval"));
            Properties.Add(new Property("MoveSpeed"));
        }

        #endregion


        #region Properties
        [Browsable(false)]
        public ScanCurve3D ScanCurve
        {
            private set;
            get;
        }

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

        [Display(
            Name = "V Axis",
            GroupName = PROP_GRP_AXIS,
            Description = "The V-Axis moves to positive direction after the H-Axis scanning.")]
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
            Name = "H Restri.",
            GroupName = PROP_GRP_AXIS,
            Description = "The scan scope restriction of the horizontal axis.")]
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
            Name = "V Restri.",
            GroupName = PROP_GRP_AXIS,
            Description = "The scan scope restriction of the vertical axis.")]
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

        #endregion

        #region Methods

        public override void Validate()
        {
            base.Validate();

            if(Axis == null)
                throw new ArgumentException("You must specify the horizontal axis.");

            if (Axis2 == null)
                throw new ArgumentException("You must specify the vertical axis.");

            if (Axis == Axis2)
                throw new ArgumentException("The horizontal axis and the vertical axis must be different.");
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
