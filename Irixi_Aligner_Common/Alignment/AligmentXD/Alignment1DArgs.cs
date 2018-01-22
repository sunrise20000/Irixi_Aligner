using Irixi_Aligner_Common.Alignment.BaseClasses;
using Irixi_Aligner_Common.Classes;
using Irixi_Aligner_Common.Classes.BaseClass;
using Irixi_Aligner_Common.MotionControllers.Base;
using System.Windows;

namespace Irixi_Aligner_Common.Alignment.AlignmentXD
{
    public class Alignment1DArgs : AlignmentArgsBase
    {
        #region Variables

        bool isEnabled;


        #endregion

        public Alignment1DArgs(SystemService Service) : base(Service)
        {
            ScanCurve = new ObservableCollectionThreadSafe<Point>();
        }
        
        public LogicalAxis Axis { set; get; }
        
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
            }
        }

        public double Interval { set; get; }

        public double ScanRange { set; get; }

        public int ScanOrder { set; get; }

        public int MaxOrder { set; get; }

        public ObservableCollectionThreadSafe<Point> ScanCurve { set; get; }

        #region Methods

        public override void ClearScanCurve()
        {
            if (ScanCurve != null)
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
