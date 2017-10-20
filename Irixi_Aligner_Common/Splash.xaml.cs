using System;
using System.Reflection;
using System.Windows;

namespace Irixi_Aligner_Common
{
    /// <summary>
    /// Interaction logic for Splash.xaml
    /// </summary>
    public partial class Splash : Window
    {
        public Splash()
        {
            InitializeComponent();

            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            txtVersionInfo.Text = string.Format("v{0}.{1}.{2}", version.Major, version.Minor, version.Revision);
        }
    }
}
