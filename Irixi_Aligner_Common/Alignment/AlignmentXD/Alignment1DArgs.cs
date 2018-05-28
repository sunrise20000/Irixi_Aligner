using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Irixi_Aligner_Common.Alignment.BaseClasses;
using Irixi_Aligner_Common.Classes;
using Irixi_Aligner_Common.MotionControllers.Base;

namespace Irixi_Aligner_Common.Alignment.AlignmentXD
{
    public class Alignment1DArgs : AlignmentArgsBase
    {
        #region Variables

        LogicalAxis axis;
        bool isEnabled;
        
        #endregion

        public Alignment1DArgs(SystemService Service) : base(Service)
        {
            ScanCurve = new ScanCurve();
        }

        [Display(
            Name = "Axis",
            GroupName = PROP_GRP_COMMON,
            Description = "")]
        
        public LogicalAxis Axis
        {
            get => axis;
            set
            {
                axis = value;
                RaisePropertyChanged();

                this.ScanCurve.DisplayName = Axis.AxisName;
            }
        }

        [Display(
            Name = "Enabled",
            GroupName = PROP_GRP_COMMON,
            Description = "")]
        public bool IsEnabled
        {
            get
            {
                return isEnabled;
            }
            set
            {
                isEnabled = value;
                RaisePropertyChanged();

                ScanCurve.Visible = isEnabled;
            }
        }

        [Display(
            Name = "Interval",
            GroupName = PROP_GRP_COMMON,
            Description = "")]
        public double Interval { set; get; }

        [Display(
            Name = "Range",
            GroupName = PROP_GRP_COMMON,
            Description = "")]
        public double ScanRange { set; get; }

        [Display(
            Name = "Order",
            GroupName = PROP_GRP_COMMON,
            Description = "")]
        public int ScanOrder { set; get; }
        
        [Browsable(false)]
        public ScanCurve ScanCurve { set; get; }

        #region Methods

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


        public override string ToString()
        {
            return string.Format("Axis {0}, {1}, {2}, {3}{4}/{5}{6}",
                new object[]
                {
                    Axis.AxisName,
                    IsEnabled ? "Enabled" : "Disabled",
                    ScanOrder,
                    Interval, Axis.PhysicalAxisInst.UnitHelper,
                    ScanRange, Axis.PhysicalAxisInst.UnitHelper
                });
        }

        #endregion
    }
}
