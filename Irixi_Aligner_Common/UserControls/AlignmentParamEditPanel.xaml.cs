using Irixi_Aligner_Common.AlignmentArithmetic;
using Irixi_Aligner_Common.Classes.BaseClass;
using Irixi_Aligner_Common.MotionControllerEntities.BaseClass;
using System.Windows;
using System.Windows.Controls;

namespace Irixi_Aligner_Common.UserControls
{
    /// <summary>
    /// Interaction logic for AlignmentParamEditPanel.xaml
    /// </summary>
    public partial class AlignmentParamEditPanel : UserControl
    {
        public AlignmentParamEditPanel()
        {
            ArgsCollection = new ObservableCollectionEx<Alignment1DArgs>();
            InitializeComponent();
        }

        public LogicalMotionComponent MotionComponent
        {
            get { return (LogicalMotionComponent)GetValue(MotionComponentProperty); }
            set { SetValue(MotionComponentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MotionComponent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MotionComponentProperty =
            DependencyProperty.Register("MotionComponent", typeof(LogicalMotionComponent), typeof(AlignmentParamEditPanel), new PropertyMetadata(null, OnMotionComponentChanged));

        private static void OnMotionComponentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(e.NewValue != null)
            {
                var owner = d as AlignmentParamEditPanel;
                var logmc = e.NewValue as LogicalMotionComponent;

                owner.MaxLogicalAxis = logmc.LogicalAxisCollection.Count;

                // clear previous editors
                owner.ArgsCollection.Clear();

                // add new editors
                int order = 0;
                foreach(var axis in logmc.LogicalAxisCollection)
                {
                    var arg = new Alignment1DArgs()
                    {
                        Axis = axis,
                        IsEnabled = false,
                        MoveSpeed = 100,
                        Interval = 0,
                        ScanRange = 0,
                        AlignOrder = order
                    };

                    owner.ArgsCollection.Add(arg);
                }
            }
        }
        

        public int MaxLogicalAxis
        {
            get { return (int)GetValue(MaxLogicalAxisProperty); }
            set { SetValue(MaxLogicalAxisProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxLogicalAxis.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxLogicalAxisProperty =
            DependencyProperty.Register("MaxLogicalAxis", typeof(int), typeof(AlignmentParamEditor), new PropertyMetadata(0));



        public ObservableCollectionEx<Alignment1DArgs> ArgsCollection
        {
            get { return (ObservableCollectionEx<Alignment1DArgs>)GetValue(ArgsCollectionProperty); }
            set { SetValue(ArgsCollectionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ArgsCollection.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ArgsCollectionProperty =
            DependencyProperty.Register("ArgsCollection", typeof(ObservableCollectionEx<Alignment1DArgs>), typeof(AlignmentParamEditor), new PropertyMetadata(null));

    }
}
