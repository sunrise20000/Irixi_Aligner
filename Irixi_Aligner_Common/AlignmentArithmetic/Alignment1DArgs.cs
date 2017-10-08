using GalaSoft.MvvmLight;
using Irixi_Aligner_Common.MotionControllerEntities.BaseClass;
using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace Irixi_Aligner_Common.AlignmentArithmetic
{
    public class Alignment1DArgs : ViewModelBase
    {
        public Alignment1DArgs()
        {
            ScanCurve = new ObservableCollection<Point>();
            //var r = new Random((int)DateTime.Now.Ticks);
            //for(int i = 0; i < 100; i++)
            //{
            //    ScanCurve.Add(new Point(i, r.NextDouble() * 10));
            //}
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
        public int AlignOrder { set; get; }
        public ObservableCollection<Point> ScanCurve { set; get; }
       
    }
}
