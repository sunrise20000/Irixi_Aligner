using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irixi_Aligner_Common.Alignment.BaseClasses;

namespace Irixi_Aligner_Common.Alignment.SnakeRouteScan
{
    public class SnakeRouteScan : AlignmentBase
    {
        #region Variables

        #endregion

        #region Constructors

        public SnakeRouteScan(SnakeRouteScanArgs Args) : base(Args)
        {
            this.Args = Args;
        }

        #endregion

        #region Properties

        public new SnakeRouteScanArgs Args
        {
            private set;
            get;
        }

        #endregion

        #region Methods

        public override void Start()
        {
            base.Start();


        }

        public override string ToString()
        {
            return "Blind Search 2 Process";
        }

        #endregion
    }
}
