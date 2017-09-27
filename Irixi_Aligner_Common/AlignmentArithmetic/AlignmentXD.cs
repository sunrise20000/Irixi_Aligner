using Irixi_Aligner_Common.Classes.BaseClass;
using Irixi_Aligner_Common.Interfaces;
using System;
using System.Windows;

namespace Irixi_Aligner_Common.AlignmentArithmetic
{
    public class AlignmentXD : AlignmentBase
    {

        #region Constructors
        public AlignmentXD(Align1DArgs Arg0, IMeasurementDevice Instrument)
        {
            AxisCount = 1;

            ScanCurveCollection = new ObservableCollectionEx<Point>[1];
            ArgsCollection = new Align1DArgs[1];
            ArgsCollection[0] = Arg0;

            this.Instrument = Instrument;
        }

        public AlignmentXD(Align1DArgs Arg0, Align1DArgs Arg1, IMeasurementDevice Instrument)
        {
            AxisCount = 2;

            ScanCurveCollection = new ObservableCollectionEx<Point>[2];
            ArgsCollection = new Align1DArgs[2];
            ArgsCollection[0] = Arg0;
            ArgsCollection[1] = Arg1;

            this.Instrument = Instrument;
        }

        public AlignmentXD(Align1DArgs Arg0, Align1DArgs Arg1, Align1DArgs Arg2, IMeasurementDevice Instrument)
        {
            AxisCount = 3;

            ScanCurveCollection = new ObservableCollectionEx<Point>[3];
            ArgsCollection = new Align1DArgs[3];
            ArgsCollection[0] = Arg0;
            ArgsCollection[1] = Arg1;
            ArgsCollection[2] = Arg2;

            this.Instrument = Instrument;
        }
        #endregion

        #region Properties
        public int AxisCount
        {
            private set;
            get;
        }

        public int CurrentAxisAligning { private set; get; }

        public IMeasurementDevice Instrument { private set; get; }

        public Align1DArgs[] ArgsCollection
        {
            private set;
            get;
        }
        
        public ObservableCollectionEx<Point>[] ScanCurveCollection { private set; get; }
        #endregion

        public override void StartAlign()
        {
            for (int i = 0; i < ArgsCollection.Length; i++)
            {
                CurrentAxisAligning = i;

                var arg = ArgsCollection[i];

                double dist_moved = 0;
                double halfrange = arg.ScanRange / 2;

                try
                {
                    // move to start position
                    if (arg.Axis.PhysicalAxisInst.Move(MoveMode.REL, arg.MoveSpeed, -halfrange) == false)
                        throw new InvalidOperationException(arg.Axis.PhysicalAxisInst.LastError);

                    // start to scan
                    while (dist_moved < arg.ScanRange)
                    {
                        // move one step
                        if (arg.Axis.PhysicalAxisInst.Move(MoveMode.REL, arg.MoveSpeed, arg.Interval) == false)
                            throw new InvalidOperationException(arg.Axis.PhysicalAxisInst.LastError);

                        // read measurement value
                        var ret = this.Instrument.Fetch();

                        // record distance moved
                        dist_moved += arg.Interval;

                        //progressHandler.Report(new Point(dist_moved, ret));
                        this.ScanCurveCollection[i].Add(new Point(dist_moved, ret));
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            
        }

        public override void StopAlign()
        {
            // stop the moving axis
            this.ArgsCollection[this.CurrentAxisAligning].Axis.PhysicalAxisInst.Stop();
        }
    }
}
