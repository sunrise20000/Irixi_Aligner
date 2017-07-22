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
                if (e.Args.Length > 0)
                {
                    string arg = e.Args[0];

                    if (arg.ToLower() == "--defaultlayout")
                        StaticVariables.DefaultLayout = true;
                }

                base.OnStartup(e);

                //DevExpress.Xpf.Core.ApplicationThemeHelper.UpdateApplicationThemeName();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnAppStartup_UpdateThemeName(object sender, StartupEventArgs e)
        {

            //DevExpress.Xpf.Core.ApplicationThemeHelper.UpdateApplicationThemeName();
        }
        
    }
}
