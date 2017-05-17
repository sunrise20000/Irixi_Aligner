using DevExpress.Xpf.Ribbon;
using GalaSoft.MvvmLight.Messaging;
using Irixi_Aligner_Common.ViewModel;
using System;
using System.Windows;

namespace Irixi_Aligner_Common
{
    public partial class MainWindow : DXRibbonWindow
    {
        public MainWindow()
        {
            try
            {
                InitializeComponent();

                Messenger.Default.Register<NotificationMessage<string>>(this, PopNotiicationMessage);

            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PopNotiicationMessage(NotificationMessage<string> message)
        {
            switch(message.Notification.ToLower())
            {
                case "error":
                    MessageBox.Show(message.Content, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
            }
        }

        private void DXRibbonWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var locator = Application.Current.Resources["Locator"] as ViewModelLocator;

            try
            {
                locator.Service.Init();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Unable to initialize the system service, \r\n{0}", ex.Message), "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DXRibbonWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var locator = Application.Current.Resources["Locator"] as ViewModelLocator;
            var config = locator.Configuration;
            config.WriteSnapshotGUI();

                
        }
    }
}
