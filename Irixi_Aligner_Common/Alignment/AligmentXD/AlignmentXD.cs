using Irixi_Aligner_Common.Alignment.BaseClasses;
using System;
using System.Linq;
using System.Windows;

namespace Irixi_Aligner_Common.Alignment.AlignmentXD
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
            int cycles = 0;
            double max_measval = 0;

            base.Start();

            do
            {
                Args.ClearScanCurve();

                // select enabled axes
                var arg_enabled = Args.AxisParamCollection.Where(a => a.IsEnabled == true);

                // sort by Axis-ID and AlignOrder
                var args = arg_enabled.OrderBy(a => a.ScanOrder).ThenBy(a => a.Axis.ID);

                foreach (var arg1d in args)
                {
                    double dist_moved = 0;
                    double halfrange = arg1d.ScanRange / 2;

                    /// <summary>
                    /// move to alignment start position.
                    /// the move methods of the physical axis MUST BE called because the move methods of logical
                    /// axis will trigger the changing of system status in SystemService.
                    /// <see cref="SystemService.MoveLogicalAxis(MotionControllers.Base.LogicalAxis, MoveByDistanceArgs)"/>
                    /// </summary>
                    if (arg1d.Axis.PhysicalAxisInst.Move(MoveMode.REL, arg1d.MoveSpeed, -halfrange) == false)
                        throw new InvalidOperationException(arg1d.Axis.PhysicalAxisInst.LastError);

                    // start to 1D scan
                    while (dist_moved <= arg1d.ScanRange)
                    {
                        // read measurement value
                        var ret = Args.Instrument.Fetch();
                        var p = new Point(dist_moved, ret);
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
                    var ordered = arg1d.ScanCurve.OrderByDescending(a => a.Y);
                    var max_pos = ordered.First().X;
                    max_measval = ordered.First().Y;

                    // move to the position of max power
                    // Note: The distance to move is minus
                    if (arg1d.Axis.PhysicalAxisInst.Move(MoveMode.REL, arg1d.MoveSpeed, -(dist_moved - max_pos)) == false)
                        throw new InvalidOperationException(arg1d.Axis.PhysicalAxisInst.LastError);
                }

                cycles++;

            } while(cycles < Args.MaxCycles && max_measval < Args.Target);
        }

        public override string ToString()
        {
            return "Alignment-XD Process";
        }
    }
}
