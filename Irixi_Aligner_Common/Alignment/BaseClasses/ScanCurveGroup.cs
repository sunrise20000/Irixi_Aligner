using System.Collections;
using Irixi_Aligner_Common.Classes.BaseClass;

namespace Irixi_Aligner_Common.Alignment.BaseClasses
{
    public class ScanCurveGroup : ObservableCollection<IList>
    {
        public void ClearCurvesContent()
        {
            foreach(var c in this)
            {
                c.Clear();
            }
        }
    }
}
