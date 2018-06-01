using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Irixi_Aligner_Common.Vision.LightSource
{
    public abstract class LightBase
    {
        protected int MAXCH, MINCH;
        public abstract bool Init(string nPort);
        public abstract bool Deint();
        public abstract bool OpenLight(int nCh,int nValue=0);
        public abstract bool CloseLight(int nCh,int nValue=0);
        public abstract bool SetLightValue(int nCh,int nValue);
        public abstract int GetLightValue(int nCh);
    }
}
