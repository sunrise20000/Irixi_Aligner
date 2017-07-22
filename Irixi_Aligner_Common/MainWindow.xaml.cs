using DevExpress.Xpf.Docking;
using DevExpress.Xpf.Ribbon;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Irixi_Aligner_Common.Classes;
using Irixi_Aligner_Common.Configuration;
using Irixi_Aligner_Common.UserControls;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace Irixi_Aligner_Common
{
    public partial class MainWindow : DXRibbonWindow
    {
        Splash splashscreen;

        public MainWindow()
        {
            try
            {
                // show splash screen
                splashscreen = new Splash();
                splashscreen.Show();

                InitializeComponent();

                Messenger.Default.Register<NotificationMessage<string>>(this, PopNotificationMessage);

                // create DocumentPanel by the logical motion components defined in the config file
                var service = SimpleIoc.Default.GetInstance<SystemService>();
                var config = SimpleIoc.Default.GetInstance<ConfigManager>();

                foreach (var motionpart in service.LogicalMotionComponentCollection)
                {
                    // create a motion component panel control
                    // which is the content of the document panel
                    MotionComponentPanel mp = new MotionComponentPanel();

                    // set the datacontext to the LogicalMotionComponent
                    mp.DataContext = motionpart;

                    // create a document panel shown on the document group
                    DocumentPanel dp = new DocumentPanel();
                    dp.Name = string.Format("dp{0}", motionpart.DisplayName.Replace(" ", ""));
                    dp.Caption = motionpart.DisplayName;
                    dp.AllowMaximize = false;
                    dp.AllowSizing = false;
                    dp.AllowClose = false;

                    // set the actual content into DocumentPanel
                    // which contains title and axis array
                    dp.Content = mp;

                    MotionComponentPanelHost.Items.Add(dp);
                }

                // restore workspace layout
                foreach(DocumentPanel panel in MotionComponentPanelHost.Items)
                {
                    // restore the layout of panels by config file
                    var layout =
                        (from items
                        in config.WorkspaceLayoutHelper.WorkspaceLayout
                         where items.PanelName == panel.Name
                         select items).First();

                    panel.Closed = layout.IsClosed;
                    panel.MDILocation = layout.MDILocation;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PopNotificationMessage(NotificationMessage<string> message)
        {
            switch(message.Notification.ToLower())
            {
                case "error":
                    MessageBox.Show(message.Content, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
            }
        }

        private async void DXRibbonWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var service = SimpleIoc.Default.GetInstance<SystemService>();

            try
            {
                await Task.Delay(100);

                service.Init();

                splashscreen.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Unable to initialize the system service, \r\n{0}", ex.Message), "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DXRibbonWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            #region Configuration
            var config = SimpleIoc.Default.GetInstance<ConfigManager>();

            // create layout object for each panel on the screen
            List<Layout> layout_arr = new List<Layout>();

            // create layout object for document panel (motion component panel, equipments control panel, etc .)
            foreach (DocumentPanel panel in MotionComponentPanelHost.Items)
            {
                // get the layout info
                Layout layout = new Layout()
                {
                    PanelName = panel.Name,
                    MDILocation = panel.MDILocation,
                    IsClosed = panel.IsClosed
                };

                layout_arr.Add(layout);
            }
            
            // save the layout to json file
            config.SaveLayout(new LayoutManager() { WorkspaceLayout = layout_arr.ToArray() });

            #endregion
            
            #region System Service            
            // close all devices in the system service object
            var service = SimpleIoc.Default.GetInstance<SystemService>();
            service.Dispose();
            #endregion

        }
    }

}
