using GalaSoft.MvvmLight;
using Irixi_Aligner_Common.Classes.BaseClass;
using Irixi_Aligner_Common.MotionControllerEntities.BaseClass;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Data;

namespace Irixi_Aligner_Common.AlignmentArithmetic
{
    public class Alignment1DArgs : ViewModelBase
    {
        static object list_lock = new object();

        public Alignment1DArgs()
        {
            ScanCurve = new ObservableCollectionThreadSafe<Point>();

            BindingOperations.EnableCollectionSynchronization(ScanCurve, list_lock);
        }

        public LogicalAxis Axis { set; get; }
        private bool isEnabled;
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

        public void ClearScanCurve()
        {
            if (ScanCurve != null)
                ScanCurve.Clear();
        }
       
    }
}
