
using Irixi_Aligner_Common.Alignment.Base;
using System;
using System.Linq;
using System.Windows;
using Irixi_Aligner_Common.Classes;
using Irixi_Aligner_Common.MotionControllers.Base;

namespace Irixi_Aligner_Common.Alignment.Rotating
{
    public class RotatingScan : AlignmentBase
    {
        #region Constructors
        public RotatingScan(RotatingScanArgs Args) : base(Args)
        {
            this.Args = Args;
        }
        #endregion

        #region Properties

        public new RotatingScanArgs Args { private set; get; }

        #endregion

        public override void Start()
        {
            base.Start();

            int cycles = 0;
            double pos_diff = 9999;

            Args.Log.Clear();

            do
            {
                cycles++;

                double distMovedLinear = 0, distMovedRotating = 0;
                double halfRangeLinear = Args.RangeLinear / 2, halfRangeRotating = Args.RangeRotating / 2;

                /// <summary>
                /// move to alignment start position.
                /// the move methods of the physical axis MUST BE called because the move methods of logical
                /// axis will trigger the changing of system status in SystemService.
                /// <see cref="SystemService.MoveLogicalAxis(LogicalAxis, MoveByDistanceArgs)"/>
                /// </summary>
                if (Args.AxisRotating.PhysicalAxisInst.Move(MoveMode.REL, Args.MoveSpeed, -halfRangeRotating) == false)
                    throw new InvalidOperationException(Args.AxisRotating.PhysicalAxisInst.LastError);

                Args.Log.Add(string.Format(">>> Start to align, cycle = {0} ...", cycles));

                // start to linear scan
                while (distMovedRotating <= Args.RangeRotating)
                {
                    // clear the previous scan curve
                    Args.ClearScanCurve();

                    // clear the variables to restart now process of linear align
                    distMovedLinear = 0;
                    if (Args.AxisLinear.PhysicalAxisInst.Move(MoveMode.REL, Args.MoveSpeed, -halfRangeLinear) == false)
                        throw new InvalidOperationException(Args.AxisLinear.PhysicalAxisInst.LastError);

                    Args.Log.Add(string.Format("*Current angle: {0}{1}, aligning ...", distMovedRotating, Args.AxisRotating.PhysicalAxisInst.UnitHelper));

                    while (distMovedLinear <= Args.RangeLinear)
                    {
                        // read power of channel 1
                        var ret = Args.Instrument.Fetch();
                        var p = new Point(distMovedLinear, ret);
                        Args.ScanCurve.Add(p);

                        // read power of channel 2
                        ret = Args.Instrument2.Fetch();
                        p = new Point(distMovedLinear, ret);
                        Args.ScanCurve2.Add(p);

                        // record distance moved
                        distMovedLinear += Args.GapLinear;

                        // move to the next point
                        if (Args.AxisLinear.PhysicalAxisInst.Move(MoveMode.REL, Args.MoveSpeed, Args.GapLinear) == false)
                            throw new InvalidOperationException(Args.AxisLinear.PhysicalAxisInst.LastError);

                        // cancel the alignment process
                        if (cts_token.IsCancellationRequested)
                            break;
                        
                    }

                    // find the position of max power
                    var ordered = Args.ScanCurve.OrderByDescending(a => a.Y);
                    var pos = ordered.First().X;
                    var power = ordered.First().Y;

                    // find the position of max power2
                    ordered = Args.ScanCurve2.OrderByDescending(a => a.Y);
                    var pos2 = ordered.First().X;
                    var power2 = ordered.First().Y;

                    // calculate the position differential
                    pos_diff = pos - pos2;

                    // move to the middle position of the max power
                    var target_pos = pos - pos_diff / 2;

                    var unitLinearAxis = Args.AxisLinear.PhysicalAxisInst.UnitHelper;

                    Args.Log.Add(string.Format("    Position of max power: ({0}{4}, {1})/({2}{4}, {3})", 
                        new object[] { pos, power, pos2, power2, unitLinearAxis }));

                    Args.Log.Add(string.Format("    Position Differential: {0}{1}", pos_diff, unitLinearAxis));
                    Args.Log.Add(string.Format("    Position of middle: {0}{1}", target_pos, unitLinearAxis));


                    // move to the position of max power
                    if (Args.AxisLinear.PhysicalAxisInst.Move(MoveMode.REL, Args.MoveSpeed, -(distMovedLinear - target_pos)) == false)
                        throw new InvalidOperationException(Args.AxisLinear.PhysicalAxisInst.LastError);

                    // if it's touched the target, exit the loop
                    if (Math.Abs(pos_diff) <= Args.TargetPositionDifferentialOfMaxPower)
                        break;

                    // record the distance moved
                    distMovedRotating += Args.GapRotating;

                    // move the axis to the next point
                    if (Args.AxisRotating.PhysicalAxisInst.Move(MoveMode.REL, Args.MoveSpeed, Args.GapRotating) == false)
                        throw new InvalidOperationException(Args.AxisRotating.PhysicalAxisInst.LastError);
                }

                Args.Log.Add(string.Format(">>> Cycle {0} done!", cycles));

            } while (cycles < Args.MaxCycles && pos_diff > Args.TargetPositionDifferentialOfMaxPower);
        }

        public override string ToString()
        {
            return "Rotating Alignment Process";
        }

    }
}
