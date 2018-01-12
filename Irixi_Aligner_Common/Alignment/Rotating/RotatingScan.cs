
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
            double posDiff = 9999;
            double rotatingDirection = 1;
            double previousPosDiff = 0, previousPosDiff2 = 0;
            double workingRotatingGap = Args.GapRotating;

            Args.Log.Clear();
            Args.PosDiffTrendCurve.Clear();

            while(true)
            {
                cycles++;

                double distMovedLinear = 0, distMovedRotating = 0;
                double halfRangeLinear = Args.RangeLinear / 2;


                Args.Log.Add(string.Format(">>> Start to align, cycle = {0} ...", cycles));


                #region Linear Alignment

                // clear the previous scan curve
                Args.ClearScanCurve();

                // clear the variables to restart now process of linear align
                distMovedLinear = 0;

                /// <summary>
                /// move to alignment start position.
                /// the move methods of the physical axis MUST BE called because the move methods of logical
                /// axis will trigger the changing of system status in SystemService.
                /// <see cref="SystemService.MoveLogicalAxis(LogicalAxis, MoveByDistanceArgs)"/>
                /// </summary>
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
                posDiff = pos - pos2;

                // move to the middle position of the max power
                var returnToPos = pos - posDiff / 2;

                var unitLinearAxis = Args.AxisLinear.PhysicalAxisInst.UnitHelper;

                Args.Log.Add(string.Format("    Position of max power: ({0}{4}, {1:F3})/({2}{4}, {3:F3})",
                    new object[] { pos, power, pos2, power2, unitLinearAxis }));

                Args.Log.Add(string.Format("    Position Differential: {0}{1}", posDiff, unitLinearAxis));
                Args.Log.Add(string.Format("    Middle Position: {0}{1}", returnToPos, unitLinearAxis));


                // move to the middle position
                if (Args.AxisLinear.PhysicalAxisInst.Move(MoveMode.REL, Args.MoveSpeed, -(distMovedLinear - returnToPos)) == false)
                    throw new InvalidOperationException(Args.AxisLinear.PhysicalAxisInst.LastError);

                // if it's touched the target, exit the loop
                if (Math.Abs(posDiff) <= Args.TargetPositionDifferentialOfMaxPower)
                    break;

                #endregion

                Args.PosDiffTrendCurve.Add(new Point(cycles, posDiff));

                // if the loop runned more than 3 times, check the change rate and shrink the rotating gap
                if (Args.PosDiffTrendCurve.Count > 3)
                {
                    var lastChange = posDiff - previousPosDiff;
                    var lastChange2 = previousPosDiff - previousPosDiff2;
                    var posDiffChangeRate = (lastChange - lastChange2) / lastChange * 100;
                    if (Math.Abs(posDiffChangeRate) <= Args.TargetPosDiffChangeRate)
                        break;

                    Args.Log.Add(string.Format("    Pos. Diff Change Rate: {0}%", posDiffChangeRate));

                    // it indicates that the order of positions of max power has changed
                    // if the sign of the pos-diff-change-rate was changed, shrink the rotating gap.
                    if (lastChange * lastChange2 < 0)
                    {
                        workingRotatingGap /= 2;
                        Args.Log.Add(string.Format("    Rotating Gap shrinked to {0}{1}", workingRotatingGap, Args.AxisRotating.PhysicalAxisInst.UnitHelper));
                    }

                }

                // determine the rotating direction
                // if the posDiff tend to be larger, the rotating direction was error for the last,
                // change the direction.
                if (posDiff > previousPosDiff)
                {
                    rotatingDirection *= -1;
                    Args.Log.Add(string.Format("    Next rotating direction has changed"));
                }

                previousPosDiff2 = previousPosDiff;
                previousPosDiff = posDiff;

                // move roll axis
                var rotating = rotatingDirection * workingRotatingGap;
                if (Args.AxisRotating.PhysicalAxisInst.Move(MoveMode.REL, Args.MoveSpeed, rotating) == false)
                    throw new InvalidOperationException(Args.AxisRotating.PhysicalAxisInst.LastError);

                // record the distance moved
                distMovedRotating += rotating;
                Args.Log.Add(string.Format("    Last/Accumulated Rotated: {0}{2}/{1}{2}", rotating, distMovedRotating, Args.AxisRotating.PhysicalAxisInst.UnitHelper));


                Args.Log.Add(string.Format(">>> Cycle {0} done!", cycles));

                if(distMovedRotating >= Args.RangeRotating)
                {
                    Args.Log.Add(string.Format("    Runs out of the rotating range, exit!"));
                    break;
                }
            }

            Args.Log.Add(string.Format("Rotating Alignment Process is done!"));
        }

        public override string ToString()
        {
            return "Rotating Alignment Process";
        }

    }
}
