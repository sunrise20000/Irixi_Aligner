using System;
using System.Windows;

namespace Irixi_Aligner_Common.AlignmentArithmetic
{
    public class AlignmentBase
    {
        public virtual void StartAlign(IProgress<Tuple<Alignment1DArgs, Point>> ProgressReport)
        {
            throw new NotImplementedException();
        }

        public virtual void StopAlign()
        {
            throw new NotImplementedException();
        }
    }
}
