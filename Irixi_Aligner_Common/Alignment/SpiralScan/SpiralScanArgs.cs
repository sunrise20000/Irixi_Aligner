using Irixi_Aligner_Common.Alignment.BaseClasses;
using Irixi_Aligner_Common.Classes.BaseClass;
using Irixi_Aligner_Common.MotionControllers.Base;
using System;
using System.Windows.Media.Media3D;

namespace Irixi_Aligner_Common.Alignment.SpiralScan
{
    public class SpiralScanArgs : AlignmentArgsBase
    {
        #region Variables
        private LogicalAxis axis0;
        private LogicalAxis axis1;
        private double target;
        private int maxCycles;
        private double gap;
        private double range;
        private int moveSpeed;
        private Size3D aspectRatio;
        #endregion

        #region Constructors
        public SpiralScanArgs()
        {
            ScanCurve = new ObservableCollectionThreadSafe<Point3D>();

            Target = 0;
            MaxCycles = 1;
            Gap = 10;
            Range = 100;
            MoveSpeed = 100;
            AspectRatio = new Size3D(1, 1, 1);

            Random r = new Random();

            for (int x = 0; x < 100; x++)
            {
                for (int y = 0; y < 100; y++)
                {
                    ScanCurve.Add(new Point3D(x, y, r.NextDouble()));
                }
            }
        }
        #endregion

        #region Properties
        
        public LogicalAxis Axis0
        {
            get => axis0;
            set
            {
                axis0 = value;
                RaisePropertyChanged();
            }
        }
        public LogicalAxis Axis1
        {
            get => axis1;
            set
            {
                axis1 = value;
                RaisePropertyChanged();
            }
        }

        public double Target
        {
            get => target;
            set
            {
                target = value;
                RaisePropertyChanged();
            }
        }

        public int MaxCycles
        {
            get => maxCycles;
            set
            {
                maxCycles = value;
                RaisePropertyChanged();
            }
        }

        public double Gap
        {
            get => gap;
            set
            {
                gap = value;
                RaisePropertyChanged();
            }
        }

        public double Range
        {
            get => range;
            set
            {
                range = value;
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

        public ObservableCollectionThreadSafe<Point3D> ScanCurve
        {
            get;
        }
        
        public Size3D AspectRatio
        {
            get => aspectRatio;
            set
            {
                aspectRatio = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region Methods
        public override void Validate()
        {
            if (Axis0.Equals(Axis1))
                throw new ArgumentException("the two axes must be different");

            if (Axis0.PhysicalAxisInst.UnitHelper.Unit != Axis1.PhysicalAxisInst.UnitHelper.Unit)
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
