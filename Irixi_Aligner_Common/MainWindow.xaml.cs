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
using DevExpress.Xpf.Bars;
using System.Windows.Data;
using Irixi_Aligner_Common.Classes.Converters;
using System.Windows.Media.Imaging;
using Irixi_Aligner_Common.ViewModel;
using Irixi_Aligner_Common.Windows;
using System.Collections;

namespace Irixi_Aligner_Common
{
    public partial class MainWindow : DXRibbonWindow
    {
        Splash splashscreen;

        public MainWindow()
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
                MotionComponentPanel mp = new MotionComponentPanel()
                {
                    // set the datacontext to the LogicalMotionComponent
                    DataContext = motionpart
                };

                // create a document panel shown on the document group
                DocumentPanel dp = new DocumentPanel()
                {
                    Name = string.Format("dp{0}", motionpart.Caption.Replace(" ", "")),
                    Caption = motionpart.Caption,
                    AllowMaximize = false,
                    AllowSizing = false,
                    //AllowClose = false,
                    ClosingBehavior = ClosingBehavior.HideToClosedPanelsCollection,

                    // set the actual content into DocumentPanel
                    // which contains title and axis array
                    Content = mp
                };

                // add the documentpanel to the documentgroup
                MotionComponentPanelHost.Items.Add(dp);

                // find the icon shown in the button
                var image = (BitmapFrame)TryFindResource(motionpart.Icon);

                // add view buttons to Ribbon toolbar
                BarCheckItem chk = new BarCheckItem()
                {
                    Content = motionpart.Caption,
                    LargeGlyph = image
                };

                // bind the IsCheck property to the document panel's Closed property
                Binding b = new Binding()
                {
                    Source = dp,
                    Path = new PropertyPath("Visibility"),
                    Mode = BindingMode.TwoWay,
                    Converter = new BooleanToVisibility()
                };
                chk.SetBinding(BarCheckItem.IsCheckedProperty, b);

                rpgView_MotionComponent.Items.Add(chk);

                // add buttons to show the preset position window 
                BarButtonItem btn = new BarButtonItem()
                {
                    Content = motionpart.Caption,
                    LargeGlyph = image,
                    DataContext = motionpart
                };

                // raise the click event
                btn.ItemClick += (s, e) =>
                {
                    ViewMassMove view = new ViewMassMove(service, motionpart);
                    MassMoveWindow w = new MassMoveWindow();
                    w.DataContext = view;
                    w.ShowDialog();
                };

                rpgPresetPositionButtonsHost.Items.Add(btn);
            }

            // restore workspace layout
            for (int i = 0; i < MotionComponentPanelHost.Items.Count; i++)
            {
                var panel = MotionComponentPanelHost.Items[i];

                if (panel is DocumentPanel)
                {
                    //var layout =
                    //    (from items
                    //    in config.WorkspaceLayoutHelper.WorkspaceLayout
                    //     where items.PanelName == panel.Name
                    //     select items).First();

                    var layout = ((IEnumerable)config.ConfWSLayout.WorkspaceLayout).Cast<dynamic>().Where(item => item.PanelName == panel.Name).First();


                    panel.Visibility = layout.IsClosed ? Visibility.Hidden : Visibility.Visible;
                    ((DocumentPanel)panel).MDILocation = layout.MDILocation;

                    //// if IsClosed property is set to true, the panel will be remove from
                    //// the Items, so the "i" should be rolled back; otherwise, some panel
                    //// will be missed.
                    //if (layout.IsClosed)
                    //    i--;
                }
            }
        }

        private void PopNotificationMessage(NotificationMessage<string> message)
        {
            if (message.Sender is SystemService)
            {
                switch (message.Notification.ToLower())
                {
                    case "error":
                        MessageBox.Show(message.Content, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                }
            }
        }

        private async void DXRibbonWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var service = SimpleIoc.Default.GetInstance<SystemService>();

            try
            {
                // update window immediately
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

            // save the workspace layout
            foreach (DocumentPanel panel in MotionComponentPanelHost.Items)
            {
                // get the layout info
                Layout layout = new Layout()
                {
                    PanelName = panel.Name,
                    MDILocation = panel.MDILocation,
                    IsClosed = (panel.Visibility == Visibility.Hidden ? true : false)
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

        /// <summary>
        /// if a document panel is requsted to be closed, *do not* actually close it, just hide it instead,
        /// otherwise, the panel will be moved to the HidenPanelCollection, and can not be enumerated in the documentgroup items.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dockLayoutManager_DockItemClosing(object sender, DevExpress.Xpf.Docking.Base.ItemCancelEventArgs e)
        {
            if (e.Item is DocumentPanel)
            {
                e.Cancel = true;
                ((DocumentPanel)e.Item).Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// bring the MDI panel to the front
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dockLayoutManager_ItemIsVisibleChanged(object sender, DevExpress.Xpf.Docking.Base.ItemIsVisibleChangedEventArgs e)
        {
            if(e.Item is DocumentPanel)
            {
                if(e.Item.Visibility == Visibility.Visible)
                    dockLayoutManager.MDIController.Activate(e.Item);
            }
        }
    }

}

