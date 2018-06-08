using System;
using System.Threading.Tasks;
using System.Windows;
using Irixi_Aligner_Common.Alignment.BaseClasses;
using Irixi_Aligner_Common.MotionControllers.Base;

namespace Irixi_Aligner_Common.Alignment.CentralAlign
{
    public class CentralAlign : AlignmentBase
    {
        #region Constructors

        public CentralAlign(CentralAlignArgs Args) : base(Args)
        {
            this.Args = Args;
        }

        #endregion


        #region Properties

        new public CentralAlignArgs Args
        {
            private set;
            get;
        }


        #endregion

        #region Methods

        public override void Start()
        {
            base.Start();

            int state = 0;

            LogicalAxis activeAxis = Args.Axis;
            double restriction = Args.AxisRestriction, interval = Args.ScanIntervalHorizontal;
            double moved = 0;

            Args.Log.Add(string.Format(">>> Start to align ..."));

            _align:

            // reset arguments
            Args.ScanCurveGroup.ClearCurvesContent();
            moved = 0;
            Random r = new Random();
            // move to the start point
            if (activeAxis.PhysicalAxisInst.Move(MoveMode.REL, Args.MoveSpeed, -(restriction / 2)) == false)
                throw new InvalidOperationException(activeAxis.PhysicalAxisInst.LastError);

            do
            {
#if !FAKE_ME
                // start to scan
                var indensity = Args.Instrument.Fetch();
                var indensity2 = Args.Instrument2.Fetch();
#else
                var indensity =r.NextDouble();
                var indensity2 = r.NextDouble();
#endif
                Args.ScanCurve.Add(new Point(moved, indensity));
                Args.ScanCurve2.Add(new Point(moved, indensity2));

                if (moved < restriction)
                {
                    // move the interval
                    activeAxis.PhysicalAxisInst.Move(MoveMode.REL, Args.MoveSpeed, interval);

                    moved += interval;
                }
                else
                {
                    break;
                }

                // cancel the alignment process
                if (cts_token.IsCancellationRequested)
                {
                    Args.Log.Add(string.Format("{0} is stopped by user!", this.ToString()));
                    return;
                }

            } while (true);

            var maxPos = Args.ScanCurve.FindMaximalPosition();
            var maxPos2 = Args.ScanCurve2.FindMaximalPosition();
            var diffPos = maxPos.X - maxPos2.X;

            // return to the position with maximum indensity
            var returnToPos = maxPos.X - diffPos / 2;

            // output messages
            Args.Log.Add(string.Format("    Position of max power: ({0}{4}, {1})/({2}{4}, {3})",
                new object[] { maxPos.X, maxPos.Y, maxPos2.X, maxPos2.Y, activeAxis.PhysicalAxisInst.UnitHelper }));

            Args.Log.Add(string.Format("    ΔPosition: {0}{1}", diffPos, activeAxis));
            Args.Log.Add(string.Format("    Middle of ΔPosition: {0}{1}", returnToPos, activeAxis.PhysicalAxisInst.UnitHelper));

            if(activeAxis.PhysicalAxisInst.Move(MoveMode.REL, Args.MoveSpeed, -(moved - returnToPos)) == false)
                throw new InvalidOperationException(activeAxis.PhysicalAxisInst.LastError);

            // switch to the next axis to scan
            if (state < 1)
            {
                state++;
                activeAxis = Args.Axis2;
                restriction = Args.Axis2Restriction;

                Task.Delay(500);

                goto _align;
            }

            Args.Log.Add(string.Format("{0} is done!", this));
        }


        public override string ToString()
        {
            return "Central Align Process";
        }

#endregion
    }
}
