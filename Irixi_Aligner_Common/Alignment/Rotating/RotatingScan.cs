
using System;
using System.Threading;
using System.Windows;
using Irixi_Aligner_Common.Alignment.BaseClasses;

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
            double deltaPos = double.MaxValue;
            double rotatingDirection = 1;
            double previousPosDiff = 0, previousPosDiff2 = 0;
            double workingRotatingGap = Args.GapRotating;

            Args.Log.Clear();
            Args.DeltaPositionTrendCurve.Clear();

            while(true)
            {
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

                // beautify(polynomial fitting) the scan curves to find the accurate position of max optical power
                Args.BeautifyScanCurves();

                // find the position of max power and draw the constant lines
                /// <seealso cref="ScanCurve.MaxPowerConstantLine"/>
                var maxPos = Args.ScanCurve.FindMaximalPosition();
                var maxPos2 = Args.ScanCurve2.FindMaximalPosition();

                // calculate the position differential
                deltaPos = maxPos.X - maxPos2.X;

                // move to the middle position of the max power
                var returnToPos = maxPos.X - deltaPos / 2;

                // get the unit of linear axis
                var unitLinearAxis = Args.AxisLinear.PhysicalAxisInst.UnitHelper;

                // output messages
                Args.Log.Add(string.Format("    Position of max power: ({0}{4}, {1})/({2}{4}, {3})",
                    new object[] { maxPos.X, maxPos.Y, maxPos2.X, maxPos2.Y, unitLinearAxis }));

                Args.Log.Add(string.Format("    ΔPosition: {0}{1}", deltaPos, unitLinearAxis));
                Args.Log.Add(string.Format("    Middle of ΔPosition: {0}{1}", returnToPos, unitLinearAxis));

                // move to the middle position
                if (Args.AxisLinear.PhysicalAxisInst.Move(MoveMode.REL, Args.MoveSpeed, -(distMovedLinear - returnToPos)) == false)
                    throw new InvalidOperationException(Args.AxisLinear.PhysicalAxisInst.LastError);

                // if it's touched the target, exit the loop
                if (Math.Abs(deltaPos) <= Args.TargetPositionDifferentialOfMaxPower)
                    break;

                Args.DeltaPositionTrendCurve.Add(new Point(cycles, deltaPos));

                #endregion

                if (cycles == 0)
                {
                    // if the first cycle, rotate to the position calculate according to the delta position and the length of the two DUTs
                    double angle = Math.Asin(deltaPos / Args.LengthOfChannelStartToEnd) * (180 / Math.PI);
                    Args.Log.Add(string.Format("    The angle to rotate for the 1st cycle is: {0}{1}", returnToPos, unitLinearAxis));

                    // record the angle rotated
                    distMovedRotating += angle;

                    if (Args.AxisRotating.PhysicalAxisInst.Move(MoveMode.REL, Args.MoveSpeed, angle) == false)
                        throw new InvalidOperationException(Args.AxisRotating.PhysicalAxisInst.LastError);
                }
                else
                {

                    // if the loop runned equals or more than 3 times, check the delta position changing rate and shrink the rotating gap
                    if (cycles >= 2)
                    {
                        var lastChange = deltaPos - previousPosDiff;
                        var lastChange2 = previousPosDiff - previousPosDiff2;
                        var posDiffChangeRate = (lastChange - lastChange2) / lastChange * 100;
                        if (Math.Abs(posDiffChangeRate) <= Args.TargetPosDiffChangeRate)
                            break;

                        Args.Log.Add(string.Format("    ΔPosition Changing Rate: {0}%", posDiffChangeRate));

                        // it indicates that the order of positions of max power has changed
                        // if the sign of the pos-diff-change-rate was changed, shrink the rotating gap.
                        if (lastChange * lastChange2 < 0)
                        {
                            workingRotatingGap /= 2;
                            Args.Log.Add(string.Format("    Half the rotating interval to {0}{1}", workingRotatingGap, Args.AxisRotating.PhysicalAxisInst.UnitHelper));
                        }
                    }

                    // determine the rotating direction
                    // if the posDiff tend to be larger, the rotating direction was error for the last,
                    // change the direction.
                    if (deltaPos > previousPosDiff)
                    {
                        rotatingDirection *= -1;
                        Args.Log.Add(string.Format("    Next rotating direction has changed"));
                    }

                    // rotate the axis
                    var angle = rotatingDirection * workingRotatingGap;
                    if (Args.AxisRotating.PhysicalAxisInst.Move(MoveMode.REL, Args.MoveSpeed, angle) == false)
                        throw new InvalidOperationException(Args.AxisRotating.PhysicalAxisInst.LastError);

                    // record the angle rotated
                    distMovedRotating += angle;
                    Args.Log.Add(string.Format("    Last/Accumulated Rotated: {0}{2}/{1}{2}", angle, distMovedRotating, Args.AxisRotating.PhysicalAxisInst.UnitHelper));

                    // the previous loop is done
                    Args.Log.Add(string.Format(">>> Cycle {0} done!", cycles));

                    // the delay makes #Lichang has the opportunity to see the previous output clearly
                    Thread.Sleep(500);

                    // if runs out of the maximum rotating range, exit
                    if (distMovedRotating >= Args.RangeRotating)
                    {
                        Args.Log.Add(string.Format("    Runs out of the rotating range, exit!"));
                        break;
                    }
                }

                previousPosDiff2 = previousPosDiff;
                previousPosDiff = deltaPos;

                cycles++;
            }

            Args.Log.Add(string.Format("Rotating Alignment Process is done!"));
        }

        public override string ToString()
        {
            return "Rotating Alignment Process";
        }

    }
}
