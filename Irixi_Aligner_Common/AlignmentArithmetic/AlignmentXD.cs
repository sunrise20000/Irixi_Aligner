using Irixi_Aligner_Common.MotionControllerEntities.BaseClass;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Irixi_Aligner_Common.AlignmentArithmetic
{
    public class AlignmentXD : AlignmentBase
    {

        #region Constructors

        public AlignmentXD(AlignmentXDArgs Args)
        {
            this.Args = Args;
        }

        #endregion

        #region Properties

        public LogicalAxis AxisAligning { private set; get; }

        public AlignmentXDArgs Args { private set; get; }
        
        #endregion

        public override void StartAlign(IProgress<Tuple<Alignment1DArgs, Point>> ProgressReport)
        {
            // validate the parameters
            // an argumentexception will be throw if it's failed
            Args.Validate();

            // select enabled axes
            var arg_enabled = Args.AxisParamCollection.Where(a => a.IsEnabled == true);

            // sort by Axis-ID and AlignOrder
            arg_enabled.OrderBy(a => a.AlignOrder).ThenBy(a=>a.Axis.ID);

            
            foreach (var arg1d in arg_enabled)
            {
                var points_origin = new PointCollection();

                // save the axis aligning to stopping it
                AxisAligning = arg1d.Axis;

                double dist_moved = 0;
                double halfrange = arg1d.ScanRange / 2;

                try
                {
                    // move to start position
                    if (arg1d.Axis.PhysicalAxisInst.Move(MoveMode.REL, arg1d.MoveSpeed, -halfrange) == false)
                        throw new InvalidOperationException(arg1d.Axis.PhysicalAxisInst.LastError);

                    // start to scan
                    while (dist_moved <= arg1d.ScanRange)
                    {
                        // read measurement value
                        var ret = Args.Instrument.Fetch();
                        var p = new Point(dist_moved, ret);
                        points_origin.Add(p); // this list is used to return to the maximum point
                        ProgressReport.Report(new Tuple<Alignment1DArgs, Point>(arg1d, p)); // this list is used to draw the curve on the window

                        // record distance moved
                        dist_moved += arg1d.Interval;

                        // move to the next point
                        if (arg1d.Axis.PhysicalAxisInst.Move(MoveMode.REL, arg1d.MoveSpeed, arg1d.Interval) == false)
                            throw new InvalidOperationException(arg1d.Axis.PhysicalAxisInst.LastError);
                    }

                    // return to the position with the maximnm measurement data
                    points_origin.OrderByDescending(a => a.Y);
                    var max_pos = points_origin[0].X;

                    // Note: The distance to move is minus
                    if (arg1d.Axis.PhysicalAxisInst.Move(MoveMode.REL, arg1d.MoveSpeed, -(dist_moved - max_pos)) == false)
                        throw new InvalidOperationException(arg1d.Axis.PhysicalAxisInst.LastError);

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
            this.AxisAligning.PhysicalAxisInst.Stop();
        }
    }
}
