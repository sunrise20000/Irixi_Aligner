using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Docking;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Irixi_Aligner_Common.Classes;
using Irixi_Aligner_Common.Classes.Converters;
using Irixi_Aligner_Common.Configuration.Common;
using Irixi_Aligner_Common.Configuration.Layout;
using Irixi_Aligner_Common.Equipments.Instruments;
using Irixi_Aligner_Common.UserControls;
using Irixi_Aligner_Common.ViewModel;
using Irixi_Aligner_Common.Windows;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Irixi_Aligner_Common
{
    public partial class MainWindow : DevExpress.Xpf.Ribbon.DXRibbonWindow
    {
        Splash splashscreen;

        public MainWindow()
        {

            // show splash screen
            splashscreen = new Splash();
            splashscreen.Show();

            InitializeComponent();

            Messenger.Default.Register<NotificationMessage<string>>(this, PopNotificationMessage);

            // create DocumentPanel per the logical motion components defined in the config file
            var service = SimpleIoc.Default.GetInstance<SystemService>();

            #region Create logical motioin components panels
            foreach (var aligner in service.LogicalMotionComponentCollection)
            {
                // create a motion component panel control
                // which is the content of the document panel
                MotionComponentPanel uc = new MotionComponentPanel()
                {
                    // set the datacontext to the LogicalMotionComponent
                    DataContext = aligner
                };

                // create a document panel in the window
                DocumentPanel panel = new DocumentPanel()
                {
                    Name = string.Format("dp{0}", aligner.Caption.Replace(" ", "")),
                    Caption = aligner.Caption,
                    AllowMaximize = false,
                    AllowSizing = false,
                    AllowFloat = false,
                    AllowDock = false,
                    //AllowClose = false,
                    ClosingBehavior = ClosingBehavior.HideToClosedPanelsCollection,

                    // put the user control into the panel
                    Content = uc
                };

                // add the documentpanel to the documentgroup
                MotionComponentPanelHost.Items.Add(panel);

                // find the icon shown in the button
                var image = (BitmapFrame)TryFindResource(aligner.Icon);

                // add view buttons to Ribbon toolbar
                BarCheckItem chk = new BarCheckItem()
                {
                    Content = aligner.Caption,
                    LargeGlyph = image
                };

                // bind the IsCheck property to the document panel's Closed property
                Binding b = new Binding()
                {
                    Source = panel,
                    Path = new PropertyPath("Visibility"),
                    Mode = BindingMode.TwoWay,
                    Converter = new VisibilityToBoolean()
                };
                chk.SetBinding(BarCheckItem.IsCheckedProperty, b);

                rpgView_MotionComponent.Items.Add(chk);

                // add buttons to show the preset position window 
                BarButtonItem btn = new BarButtonItem()
                {
                    Content = aligner.Caption,
                    LargeGlyph = image,
                    DataContext = aligner
                };

                // raise the click event
                btn.ItemClick += (s, e) =>
                {
                    //var view = new ViewMassMove(service, aligner);
                    //var win = new MassMoveWindow
                    //{
                    //    DataContext = view
                    //};
                    //win.ShowDialog();
                };

                rpgPresetPositionButtonsHost.Items.Add(btn);
            }
            #endregion

            #region Create control panels for instruments

            ViewModelBase viewInstr;
            foreach (var instr in service.MeasurementInstrumentCollection)
            {
                UserControl uctrl = null;

                //TODO The following codes is not elegant, the code must be expanded if new type of instrument added into the system
                if (instr is Keithley2400)
                {
                    // create the user control for k2400
                    viewInstr = new ViewKeithley2400(instr as Keithley2400);
                    uctrl = new Keithley2400ControlPanel()
                    {
                        DataContext = viewInstr
                    };
                }
                else if(instr is Newport2832C)
                {
                    // create the user control for k2400
                    viewInstr = new ViewNewport2832C(instr as Newport2832C);
                    uctrl = new Newport2832cControlPanel()
                    {
                        DataContext = viewInstr
                    };
                }

                // create document panel in the window
                DocumentPanel panel = new DocumentPanel()
                {
                    Name = string.Format("dp{0}", instr.DeviceClass.ToString("N")),
                    Caption = instr.Config.Caption,
                    AllowMaximize = false,
                    AllowSizing = false,
                    AllowDock = false,
                    AllowFloat = false,
                    ClosingBehavior = ClosingBehavior.HideToClosedPanelsCollection,

                    // put the user control into the panel
                    Content = uctrl
                };

                // add the documentpanel to the documentgroup
                MotionComponentPanelHost.Items.Add(panel);

                // find the icon shown in the button
                var image = (BitmapFrame)TryFindResource(instr.Config.Icon);

                // add view buttons to Ribbon toolbar
                BarCheckItem chk = new BarCheckItem()
                {
                    Content = instr.Config.Caption,
                    LargeGlyph = image
                };

                // bind the IsCheck property to the document panel's Closed property
                Binding b = new Binding()
                {
                    Source = panel,
                    Path = new PropertyPath("Visibility"),
                    Mode = BindingMode.TwoWay,
                    Converter = new VisibilityToBoolean()
                };
                chk.SetBinding(BarCheckItem.IsCheckedProperty, b);

                rpgView_Equipments.Items.Add(chk);
            }

            #endregion

            #region Restore workspace layout
            var config = SimpleIoc.Default.GetInstance<ConfigManager>();
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

                    try
                    {
                        var setting = ((IEnumerable)config.ConfWSLayout.WorkspaceLayout).Cast<dynamic>().Where(item => item.PanelName == panel.Name).First();
                        panel.Visibility = setting.IsClosed ? Visibility.Hidden : Visibility.Visible;
                        ((DocumentPanel)panel).MDILocation = setting.MDILocation;
                    }
                    catch
                    {
                        ; // do nothing if the panel was not found in layout setting file
                       
                    }

                }
            }
            #endregion
        }

        private void PopNotificationMessage(NotificationMessage<string> message)
        {
            //if (message.Sender is SystemService)
            //{
                switch (message.Notification.ToLower())
                {
                    case "error":
                        MessageBox.Show(message.Content, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        
                        break;
                }
            //}
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

        private void btnAlignmentXD_ItemClick(object sender, ItemClickEventArgs e)
        {
            panelAlignmentXD.Visibility = Visibility.Visible;
            dockLayoutManager.MDIController.Activate(panelAlignmentXD);
        }

        private void btnBlindSearch_ItemClick(object sender, ItemClickEventArgs e)
        {
            panelBlindSearch.Visibility = Visibility.Visible;
            dockLayoutManager.MDIController.Activate(panelBlindSearch);
        }

        private void btnRotatingScan_ItemClick(object sender, ItemClickEventArgs e)
        {
            panelRotatingScan.Visibility = Visibility.Visible;
            dockLayoutManager.MDIController.Activate(panelRotatingScan);
        }

        private void btnSnakeScan_ItemClick(object sender, ItemClickEventArgs e)
        {
            panelSnakeRouteScan.Visibility = Visibility.Visible;
            dockLayoutManager.MDIController.Activate(panelSnakeRouteScan);
        }

        private void btnCentralAlign_ItemClick(object sender, ItemClickEventArgs e)
        {
            panelCentralAlign.Visibility = Visibility.Visible;
            dockLayoutManager.MDIController.Activate(panelCentralAlign);
        }
    }

}

