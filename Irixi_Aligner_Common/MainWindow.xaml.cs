using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Docking;
using DevExpress.Xpf.Ribbon;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Irixi_Aligner_Common.Classes;
using Irixi_Aligner_Common.Classes.Converters;
using Irixi_Aligner_Common.Configuration.Common;
using Irixi_Aligner_Common.Configuration.Layout;
using Irixi_Aligner_Common.Equipments.Instruments;
using Irixi_Aligner_Common.Interfaces;
using Irixi_Aligner_Common.MotionControllers.Base;
using Irixi_Aligner_Common.UserControls;
using Irixi_Aligner_Common.ViewModel;


namespace Irixi_Aligner_Common
{
    public partial class MainWindow : DXRibbonWindow
    {
        // splash screen instance
        Splash splashscreen;
        Dictionary<string, Control> windowDic = new Dictionary<string, Control>();
        // thread to open splash screen
        Thread SplashThread;

        // signal indicates that the splash screen has opened successfully
        ManualResetEvent ResetSplashCreated;
        WindowScript ScriptWindow = new WindowScript();
        public MainWindow()
        {          
            #region Show Splash Screen
            // show splash screen
            ResetSplashCreated = new ManualResetEvent(false);

            // Create a new thread for the splash screen to run on
            SplashThread = new Thread(ShowSplash);
            SplashThread.SetApartmentState(ApartmentState.STA);
            SplashThread.IsBackground = true;
            SplashThread.Name = "Splash Screen";
            SplashThread.Start();

            ResetSplashCreated.WaitOne();

            #endregion

            splashscreen.ShowMessage("Initializing main window ...");
            InitializeComponent();

            DevExpress.Xpf.Core.DXGridDataController.DisableThreadingProblemsDetection = true;

            Messenger.Default.Register<NotificationMessage<string>>(this, PopNotificationMessage);

            // create DocumentPanel per the logical motion components defined in the config file
            var service = SimpleIoc.Default.GetInstance<SystemService>();

            #region Create logical motioin components panels
            foreach (LogicalMotionComponent aligner in service.LogicalMotionComponentCollection)
            {
                splashscreen.ShowMessage(string.Format("Initializing {0} panel ...", aligner));

                // create a motion component panel control
                // which is the content of the document panel
                MotionComponentPanel mcPanel = new MotionComponentPanel()
                {
                    // set the datacontext to the LogicalMotionComponent
                    DataContext = aligner
                };
                Binding bind = new Binding()
                {
                    Source = service,
                    Path = new PropertyPath("ScriptState"),
                    Converter = new ScriptStateToGridEnable(),
                    Mode = BindingMode.OneWay

                };
                mcPanel.SetBinding(IsEnabledProperty, bind);

                // create a document panel
                DocumentPanel panel = new DocumentPanel()
                {
                    Name = string.Format("dp{0}", aligner.Caption.Replace(" ", "")),
                    Caption = aligner.Caption,
                    AllowContextMenu = false,
                    AllowMaximize = false,
                    AllowSizing = false,
                    AllowFloat = false,
                    AllowDock = false,
                    //AllowClose = false,
                    ClosingBehavior = ClosingBehavior.HideToClosedPanelsCollection,

                    // put the user control into the panel
                    Content = mcPanel
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
               
            }
            #endregion

            #region Create control panels for instruments

            ViewModelBase viewInstr;
            foreach (IInstrument instr in service.CollectionViewDefinedInstruments)
            {
                UserControl uctrl = null;
                //TODO The following codes is not elegant, the code must be expanded if new type of instrument added into the system
                if (instr is Keithley2400)
                {
                    // create the user control for k2400
                    viewInstr = new ViewKeithley2400(instr as Keithley2400);

                    uctrl = new Keithley2400ControlPanel()
                    {
                        DataContext = viewInstr,
                        
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
                
                splashscreen.ShowMessage(string.Format("Initializing {0} panel ...", instr));


                //Set IsEnable Binding
                Binding bind = new Binding
                {
                    Path = new PropertyPath("ScriptState"),
                    Source = service,
                    Converter=new ScriptStateToGridEnable(),
                    Mode=BindingMode.OneWay
                };
                uctrl.SetBinding(IsEnabledProperty, bind);

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
                windowDic.Add(instr.Config.Caption.Replace(" ", "_"), panel);
                windowDic.Add(panelAlignmentXD.Name, panelAlignmentXD);
                windowDic.Add(panelBlindSearch.Name, panelBlindSearch);
                windowDic.Add(panelCentralAlign.Name, panelCentralAlign);
                windowDic.Add(panelRotatingScan.Name, panelRotatingScan);
                windowDic.Add(panelSnakeRouteScan.Name, panelSnakeRouteScan);



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

            splashscreen.ShowMessage(string.Format("Restoring workspace ..."));

            #region Restore workspace layout

            var config = SimpleIoc.Default.GetInstance<ConfigManager>();
            for (int i = 0; i < MotionComponentPanelHost.Items.Count; i++)
            {
                var panel = MotionComponentPanelHost.Items[i];

                if (panel is DocumentPanel)
                {
                    try
                    {
                        var setting = ((IEnumerable)config.ConfWSLayout.WorkspaceLayout).Cast<dynamic>().Where(item => item.PanelName == panel.Name).First();

                        // set visibility
                        panel.Visibility = setting.IsClosed ? Visibility.Hidden : Visibility.Visible;

                        // set location
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

        private void ShowSplash()
        {
            // create the instance of the splash screen
            splashscreen = new Splash();
            splashscreen.Show();

            // the splash screen has initialized, the main thread can go on 
            ResetSplashCreated.Set();
            System.Windows.Threading.Dispatcher.Run();
        }

        private void PopNotificationMessage(NotificationMessage<string> message)
        {

            switch (message.Notification.ToLower())
            {
                case "error":
                    MessageBox.Show(message.Content, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                case "postwindow":
                    if (windowDic.Keys.Contains<string>(message.Content))
                        Application.Current.Dispatcher.Invoke(() => windowDic[message.Content].Visibility = Visibility.Visible);
                    break;
            }          
        }

        private void DXRibbonWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var service = SimpleIoc.Default.GetInstance<SystemService>();

            try
            {
                splashscreen.ShowMessage(string.Format("Starting system service ..."));

                service.Init();

                // close the splash screen
                splashscreen.LoadComplete();

                service.StartMonitor();
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
            service.StopMonitor();
            service.Dispose();
            #endregion

            #region Close ScriptWindow
            ScriptWindow.SetCloseFlag(true);
            ScriptWindow.Close();
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

        private void btnOpenPositionPresetPanel_ItemClick(object sender, ItemClickEventArgs e)
        {
            panelPositionPreset.Visibility = Visibility.Visible;
            dockLayoutManager.MDIController.Activate(panelPositionPreset);
        }

        private void btn_Script_ItemClick(object sender, ItemClickEventArgs e)
        {  
                ScriptWindow.Show();
        }
    }

}

