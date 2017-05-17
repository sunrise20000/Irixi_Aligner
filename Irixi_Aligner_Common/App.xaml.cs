using System;
using System.Windows;

namespace Irixi_Aligner_Common
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
            
                base.OnStartup(e);
                DevExpress.Xpf.Core.ApplicationThemeHelper.UpdateApplicationThemeName();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnAppStartup_UpdateThemeName(object sender, StartupEventArgs e)
        {

            DevExpress.Xpf.Core.ApplicationThemeHelper.UpdateApplicationThemeName();
        }
        
    }
}
