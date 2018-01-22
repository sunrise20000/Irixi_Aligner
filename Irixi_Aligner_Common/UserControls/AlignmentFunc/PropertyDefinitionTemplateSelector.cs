using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Irixi_Aligner_Common.Classes.BaseClass;

namespace Irixi_Aligner_Common.UserControls.AlignmentFunc
{
    public class PropertyDefinitionTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;

            if (element != null && item != null)
            {
                var pd = item as Property;
                var pd_name = pd.Name;

                // find the DataTemplate named with the property name
                var res = element.TryFindResource(pd_name) as DataTemplate;

                if (res != null)
                    return res;

            }

            return element.TryFindResource("DefaultPropDefTemplate") as DataTemplate;
        }
    }
}
