using System.Windows;
using System.Windows.Controls;
using Irixi_Aligner_Common.Alignment.BaseClasses;

namespace Irixi_Aligner_Common.UserControls.AlignmentFunc
{
    public class ChartTypeSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;
            AlignmentArgsBase args = item as AlignmentArgsBase;
            var grp = args.ScanCurveGroup;

            if(grp != null)
            {
                if(grp.Count == 0 || grp[0].GetType() == typeof(ScanCurve))
                {
                    return (DataTemplate)element.TryFindResource("TemplateChart2D");
                }
                else
                {
                    return (DataTemplate)element.TryFindResource("TemplateChart3D");
                }
            }
            else
            {
                return null;
            }
        }
    }
}
