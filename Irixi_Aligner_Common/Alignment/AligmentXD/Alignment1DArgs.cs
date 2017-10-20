using Irixi_Aligner_Common.Alignment.Base;
using Irixi_Aligner_Common.Classes.BaseClass;
using Irixi_Aligner_Common.MotionControllers.Base;
using System.Windows;

namespace Irixi_Aligner_Common.Alignment.AlignmentXD
{
    public class Alignment1DArgs : AlignmentArgsBase
    {
        private bool isEnabled;

        public Alignment1DArgs()
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

        public int MoveSpeed { set; get; }

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
