using Irixi_Aligner_Common.Classes.BaseClass;
using Irixi_Aligner_Common.Interfaces;
using System.Threading;

namespace Irixi_Aligner_Common.Alignment.BaseClasses
{
    public class AlignmentBase : IServiceSystem
    {
        protected CancellationTokenSource cts;
        protected CancellationToken cts_token;

        public AlignmentBase(AlignmentArgsBase Args)
        {
            this.Args = Args;
        }
       
        public AlignmentArgsBase Args
        {
            protected set; get;
        }

        public virtual void Start()
        {
            cts = new CancellationTokenSource();
            cts_token = cts.Token;
                
            Args.Validate();
            Args.ClearScanCurve();
        }

        public virtual void Stop()
        {
            if(cts!=null)
                cts.Cancel();
        }
        
    }
}
