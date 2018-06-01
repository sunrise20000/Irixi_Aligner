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
    /// UC_MessageBox.xaml 的交互逻辑
    /// </summary>
    public partial class UC_MessageBox : Window
    {
        private static MessageBoxResult result;
        private static UC_MessageBox _inst =null;
        public static UC_MessageBox Instance
        {
            get
            {
                if (_inst == null)
                    _inst = new UC_MessageBox();
                return _inst;
            }
        }
        private UC_MessageBox()
        {
            InitializeComponent();
            StrCaption = "GPAS";
            StrContent = "Message";
        }
        public MessageBoxResult ShowBox(string strContent, string strCaption="GPAS")
        {
            this.StrCaption = strCaption;
            this.StrContent = strContent;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ShowDialog();
            return result;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            result = MessageBoxResult.Yes;
            Close();
            _inst = null;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            result = MessageBoxResult.No;
            Close();
            _inst = null;
        }
        public string StrCaption { get { return GetValue(StrCaptionProperty).ToString(); } set { SetValue(StrCaptionProperty, value); } }
        public string StrContent { get { return GetValue(StrContentProperty).ToString(); } set { SetValue(StrContentProperty, value); } }
        public static readonly DependencyProperty StrCaptionProperty = DependencyProperty.Register("StrCaption", typeof(string), typeof(UC_MessageBox));
        public static readonly DependencyProperty StrContentProperty = DependencyProperty.Register("StrContent", typeof(string), typeof(UC_MessageBox));

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}
