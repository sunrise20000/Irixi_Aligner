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

namespace Irixi_Aligner_Common.UserControls
{
    /// <summary>
    /// WindowScript.xaml 的交互逻辑
    /// </summary>
    public partial class WindowScript : Window
    {
        private bool bClose = false;
        public WindowScript()
        {
            InitializeComponent();
        }
        ~WindowScript()
        {

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = !bClose;
        }
        public void SetCloseFlag(bool bClose)
        {
            this.bClose = bClose;
        }
    }
}
