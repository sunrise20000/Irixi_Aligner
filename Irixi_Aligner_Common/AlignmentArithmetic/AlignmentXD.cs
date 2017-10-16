using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Irixi_Aligner_Common.AlignmentArithmetic
{
    public class AlignmentXD : AlignmentBase
    {

        #region Constructors

        public AlignmentXD(AlignmentXDArgs Args) : base(Args)
        {
            this.Args = Args;
        }

        #endregion

        #region Properties
        
        public new AlignmentXDArgs Args { private set; get;}
        
        #endregion

        public override void Start()
        {
            base.Start();

            // select enabled axes
            var arg_enabled = Args.AxisParamCollection.Where(a => a.IsEnabled == true);

            // sort by Axis-ID and AlignOrder
            var args =  arg_enabled.OrderBy(a => a.ScanOrder).ThenBy(a=>a.Axis.ID);
            
            foreach (var arg1d in args)
            {
                var points_origin = new PointCollection();

                double dist_moved = 0;
                double halfrange = arg1d.ScanRange / 2;

                // move to start position
                if (arg1d.Axis.PhysicalAxisInst.Move(MoveMode.REL, arg1d.MoveSpeed, -halfrange) == false)
                    throw new InvalidOperationException(arg1d.Axis.PhysicalAxisInst.LastError);

                // start to 1D scan
                while (dist_moved <= arg1d.ScanRange)
                {
                    // read measurement value
                    var ret = Args.Instrument.Fetch();
                    var p = new Point(dist_moved, ret);
                    points_origin.Add(p); // this list is used to return to the maximum point
                    arg1d.ScanCurve.Add(p);

                    // record distance moved
                    dist_moved += arg1d.Interval;

                    // move to the next point
                    if (arg1d.Axis.PhysicalAxisInst.Move(MoveMode.REL, arg1d.MoveSpeed, arg1d.Interval) == false)
                        throw new InvalidOperationException(arg1d.Axis.PhysicalAxisInst.LastError);

                    // cancel the alignment process
                    if (cts_token.IsCancellationRequested)
                        break;
                }

                // cancel the alignment process
                if (cts_token.IsCancellationRequested)
                    break;

                // return to the position with the maximnm measurement data
                points_origin.OrderByDescending(a => a.Y);
                var max_pos = points_origin[0].X;

                // Note: The distance to move is minus
                if (arg1d.Axis.PhysicalAxisInst.Move(MoveMode.REL, arg1d.MoveSpeed, -(dist_moved - max_pos)) == false)
                    throw new InvalidOperationException(arg1d.Axis.PhysicalAxisInst.LastError);

            }
        }

        public override string ToString()
        {
            return "Alignment-XD Process";
        }
    }
}
