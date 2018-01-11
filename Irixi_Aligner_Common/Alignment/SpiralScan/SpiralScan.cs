using Irixi_Aligner_Common.Alignment.Base;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Irixi_Aligner_Common.Alignment.SpiralScan
{
    public class SpiralScan : AlignmentBase
    {
        #region variables
        enum MoveSequence
        {
            Down = 0,
            Right,
            Up,
            Left
        }
        #endregion
        
        #region Constructor
        public SpiralScan(SpiralScanArgs Args) : base(Args)
        {
            this.Args = Args;
        }
        #endregion

        #region Properties
        public new SpiralScanArgs Args { set; get; }
        #endregion

        #region Private Methods
        private void MoveBySequence(MoveSequence Sequence, int Speed, double Distance, ref Point LastPosition)
        {
            switch(Sequence)
            {
                // move along the logical X axis
                case MoveSequence.Right:
                    if (this.Args.Axis0.PhysicalAxisInst.Move(MoveMode.REL, Speed, Distance))
                        LastPosition.X += Distance;
                    else
                        throw new InvalidOperationException(this.Args.Axis0.PhysicalAxisInst.LastError);

                    break;

                // move along the logical X axis
                case MoveSequence.Left:
                    if (this.Args.Axis0.PhysicalAxisInst.Move(MoveMode.REL, Speed, -Distance))
                        LastPosition.X -= Distance;
                    else
                        throw new InvalidOperationException(this.Args.Axis0.PhysicalAxisInst.LastError);
                    break;
                    
                // move along the logical Y axis
                case MoveSequence.Up:
                    if (this.Args.Axis1.PhysicalAxisInst.Move(MoveMode.REL, Speed, Distance))
                        LastPosition.Y += Distance;
                    else
                        throw new InvalidOperationException(this.Args.Axis1.PhysicalAxisInst.LastError);

                    break;

                // move along the logical Y axis
                case MoveSequence.Down:
                    if (this.Args.Axis1.PhysicalAxisInst.Move(MoveMode.REL, Speed, -Distance))
                        LastPosition.Y -= Distance;
                    else
                        throw new InvalidOperationException(this.Args.Axis1.PhysicalAxisInst.LastError);

                    break;

                
            }
        }

        #endregion

        #region Override Methods
        public override void Start()
        {
            double max_measval = 0;
            int cycle = 0;
            
            base.Start();

            do
            {
                int moved_points = 0;
                var curr_pos = new Point(0, 0);
                var curr_point3d = new Point3D();

                Args.ClearScanCurve();

                while (true)
                {
                    // determine how many times and which direction to move
                    int move_times = (int)Math.Ceiling((decimal)moved_points / 2);
                    MoveSequence move_to = (MoveSequence)(moved_points % 4);

                    if (move_times == 0) // initital point
                    {
                        var fb = this.Args.Instrument.Fetch();
                        curr_point3d = new Point3D(curr_pos.X, curr_pos.Y, fb);
                        this.Args.ScanCurve.Add(curr_point3d);
                    }
                    else
                    {
                        for (int i = 0; i < move_times; i++)
                        {
                            /// <summary>
                            /// move to alignment start position.
                            /// the move methods of the physical axis MUST BE called because the move methods of logical
                            /// axis will trigger the changing of system status in SystemService.
                            /// <see cref="SystemService.MoveLogicalAxis(MotionControllers.Base.LogicalAxis, MoveByDistanceArgs)"/>
                            /// </summary>
                            MoveBySequence(move_to, this.Args.MoveSpeed, this.Args.Gap, ref curr_pos);

                            var fb = this.Args.Instrument.Fetch();
                            curr_point3d = new Point3D(curr_pos.X, curr_pos.Y, fb);
                            this.Args.ScanCurve.Add(curr_point3d);

                            // cancel the alignment process
                            if (cts_token.IsCancellationRequested)
                                break;
                        }
                    }

                    // cancel the alignment process
                    if (cts_token.IsCancellationRequested)
                        break;

                    if (Math.Abs(curr_point3d.X) >= this.Args.Range
                        && Math.Abs(curr_point3d.Y) >= this.Args.Range)
                        break;

                    moved_points++;
                }

                // move the position with maximum measurement value
                var ordered = Args.ScanCurve.OrderByDescending(a => a.Z);
                var last_x = Args.ScanCurve[Args.ScanCurve.Count - 1].X;
                var last_y = Args.ScanCurve[Args.ScanCurve.Count - 1].Y;
                var max_x = ordered.First().X;
                var max_y = ordered.First().Y;

                max_measval = ordered.First().Z;

                // Axis0 acts logical X
                Args.Axis0.PhysicalAxisInst.Move(MoveMode.REL, Args.MoveSpeed, -(last_x - max_x));
                // Axis1 acts logical Y
                Args.Axis1.PhysicalAxisInst.Move(MoveMode.REL, Args.MoveSpeed, -(last_y - max_y));


                cycle++;

            } while (cycle < Args.MaxCycles && max_measval < Args.Target);
        }

        public override string ToString()
        {
            return "Spiral-Scan Process";
        }

        #endregion
    }
}
