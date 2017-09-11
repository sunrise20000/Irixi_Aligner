using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Irixi_Aligner_Common.Windows
{
    /// <summary>
    /// Interaction logic for InputPresetNameWindow.xaml
    /// </summary>
    public partial class InputBox : Window
    {
        public InputBox(string WindowTitle, string Caption)
        {
            InitializeComponent();
            this.Title = WindowTitle;
            txtCaption.Text = Caption;
        }

        public string Input
        {
            get
            {
                return txtInput.Text;
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
