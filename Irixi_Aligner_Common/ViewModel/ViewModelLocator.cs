/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:Irixi_Aligner_Common"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Irixi_Aligner_Common.Classes;
using Irixi_Aligner_Common.Configuration.Common;
using Microsoft.Practices.ServiceLocation;

namespace Irixi_Aligner_Common.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            if (ViewModelBase.IsInDesignModeStatic)
            {
                // Create design time view services and models
            }
            else
            {
                // Create run time view services and models
                SimpleIoc.Default.Register<SystemService>();
                SimpleIoc.Default.Register<ConfigManager>();
            }

            
        }

        public SystemService Service
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SystemService>();
            }
        }

        public ConfigManager Configuration
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ConfigManager>();
            }
        }
        
        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}