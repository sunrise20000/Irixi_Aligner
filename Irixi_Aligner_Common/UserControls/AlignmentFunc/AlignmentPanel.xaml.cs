using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Irixi_Aligner_Common.UserControls.AlignmentFunc
{
    /// <summary>
    /// Interaction logic for AlignmentPanel.xaml
    /// </summary>
    public partial class AlignmentPanel : UserControl
    {
        public AlignmentPanel()
        {
            InitializeComponent();
        }



        public Uri PropertiesTemplate
        {
            get { return (Uri)GetValue(PropertiesTemplateProperty); }
            set { SetValue(PropertiesTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PropertiesTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PropertiesTemplateProperty =
            DependencyProperty.Register("PropertiesTemplate", typeof(Uri), typeof(UserControl), 
                new PropertyMetadata(null, (s, e)=> 
                {
                    var owner = s as AlignmentPanel;
                    owner.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = (Uri)e.NewValue });
                }));

        public ICommand StartCommand
        {
            get { return (ICommand)GetValue(StartCommandProperty); }
            set { SetValue(StartCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StartCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StartCommandProperty =
            DependencyProperty.Register("StartCommand", typeof(ICommand), typeof(UserControl), new PropertyMetadata(null));



        public object StartCommandParameter
        {
            get { return (object)GetValue(StartCommandParameterProperty); }
            set { SetValue(StartCommandParameterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StartCommandParameter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StartCommandParameterProperty =
            DependencyProperty.Register("StartCommandParameter", typeof(object), typeof(UserControl), new PropertyMetadata(null));




        public ICommand StopCommand
        {
            get { return (ICommand)GetValue(StopCommandProperty); }
            set { SetValue(StopCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StopCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StopCommandProperty =
            DependencyProperty.Register("StopCommand", typeof(ICommand), typeof(UserControl), new PropertyMetadata(null));

    }
}
