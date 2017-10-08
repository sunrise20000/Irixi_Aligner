using Irixi_Aligner_Common.MotionControllerEntities.BaseClass;
using System.Windows;
using System.Windows.Controls;

namespace Irixi_Aligner_Common.UserControls
{
    /// <summary>
    /// Interaction logic for AlignmentParamEditor.xaml
    /// </summary>
    public partial class AlignmentParamEditor : UserControl
    {
        public AlignmentParamEditor()
        {
            InitializeComponent();
        }


        /// <summary>
        /// Get or set how many axes are there in the logical motion component
        /// </summary>
        public int MaxAxesCount
        {
            get { return (int)GetValue(MaxAxesCountProperty); }
            set { SetValue(MaxAxesCountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxAxesCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxAxesCountProperty =
            DependencyProperty.Register("MaxAxesCount", typeof(int), typeof(AlignmentParamEditor), new PropertyMetadata(0, OnMaxAxesCountChanged));

        private static void OnMaxAxesCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var owner = d as AlignmentParamEditor;

            for(int i = 1; i <= (int)e.NewValue; i++)
                owner.edtAlignOrderSel.Items.Add(i);
        }

    }
}
