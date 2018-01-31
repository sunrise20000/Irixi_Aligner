using System.Collections;
using Irixi_Aligner_Common.Classes.BaseClass;
using Irixi_Aligner_Common.Interfaces;

namespace Irixi_Aligner_Common.Alignment.BaseClasses
{
    public class ScanCurveGroup : ObservableCollectionEx<IScanCurve>
    {
        public void ClearCurvesContent()
        {
            foreach(var c in this)
            {
                c.Clear();
            }
        }

        public void ChangeDisplayName(string DisplayName)
        {
            foreach (var c in this)
            {
                c.DisplayName = DisplayName;
            }
        }

        public void ChangePrefix(string Prefix)
        {
            foreach (var c in this)
            {
                c.Prefix = Prefix;
            }

        }

        public void ChangeSuffix(string Suffix)
        {
            foreach (var c in this)
            {
                c.Suffix = Suffix;
            }

        }
    }
}
