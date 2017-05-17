using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Irixi_Aligner_Common.Classes.BaseClass
{
    public class MoveArgs
    {
        public int Speed { get; set; }
        public int Distance { get; set; }
        public MoveMode Mode { get; set; }

        public override string ToString()
        {
            return string.Format("{0}/{1}/{2}", Mode, Speed, Distance);
        }
    }
}
