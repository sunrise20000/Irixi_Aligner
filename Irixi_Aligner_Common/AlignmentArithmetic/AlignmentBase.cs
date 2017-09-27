using System;
using System.Windows;

namespace Irixi_Aligner_Common.AlignmentArithmetic
{
    public class AlignmentBase
    {
        public delegate void PointUpdatedEventHandler(object send, Point EventArgs);
        public event PointUpdatedEventHandler OnPointUpdated;

        protected IProgress<Point> progressHandler;
        
        public virtual void StartAlign()
        {
            throw new NotImplementedException();
        }

        public virtual void StopAlign()
        {
            throw new NotImplementedException();
        }
    }
}
