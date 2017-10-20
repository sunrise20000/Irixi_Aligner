using GalaSoft.MvvmLight.Messaging;
using Irixi_Aligner_Common.MotionControllers.Base;
using Irixi_Aligner_Common.UserControls;
using Irixi_Aligner_Common.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace Irixi_Aligner_Common.Windows
{
    /// <summary>
    /// Interaction logic for MassMoveWindow.xaml
    /// </summary>
    public partial class MassMoveWindow : Window
    {
        public MassMoveWindow()
        {
            InitializeComponent();

            Messenger.Default.Register<NotificationMessage<string>>(this, OnNotificationMessageReceived);
            Messenger.Default.Register<NotificationMessageAction<MessageBoxResult>>(this, OnNotificationMessageActionReceived);

        }

        private void OnNotificationMessageActionReceived(NotificationMessageAction<MessageBoxResult> obj)
        {
            if(obj.Sender is ViewMassMove)
            {
                switch(obj.Notification.ToString())
                {
                    case "AskForOverwrite":
                        obj.Execute(MessageBox.Show("The preset position has been existed, overwrite ?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Question));
                        break;
                }
            }
        }

        private void OnNotificationMessageReceived(NotificationMessage<string> obj)
        {
            if (obj.Sender is ViewMassMove)
            {
                switch (obj.Notification.ToLower())
                {
                    case "error":
                        MessageBox.Show(obj.Content, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;

                    case "notify":
                        MessageBox.Show(obj.Content, "Message", MessageBoxButton.OK, MessageBoxImage.Information);
                        break;
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // set the height of window dynamically
            ViewMassMove view = DataContext as ViewMassMove;
            this.Height = 180 + view.AxisControlCollection.Count * view.AxisControlCollection[0].ActualHeight;

            // create the preset position list
            cmbPostionList.Items.Add("*");
            cmbPostionList.SelectedIndex = 0;

            // load preset position files
            string[] names = view.GetPresetPositionList();
            foreach(var n in names)
            {
                cmbPostionList.Items.Add(n);
            }

            int _hashcode = view.MotionComponent.GetHashCode();
            txtHashCode.Text = _hashcode.ToString("X");

            
        }

        #region Events
        private void cmbPostionList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewMassMove view = DataContext as ViewMassMove;

            if (cmbPostionList.SelectedIndex == 0)
            {
                // read current position

                btnSave.IsEnabled = true;
                btnDelete.IsEnabled = false;
                btnMove.IsEnabled = false;
                btnStop.IsEnabled = false;

                for (int i = 0; i < view.AxisControlCollection.Count; i ++)
                {
                    Axis4MassMove _axis_view = view.AxisControlCollection[i];
                    LogicalAxis _log_axis = view.MotionComponent.LogicalAxisCollection[i];

                    _axis_view.Position = _log_axis.PhysicalAxisInst.UnitHelper.RelPosition;
                    _axis_view.IsAbsMode = _log_axis.PhysicalAxisInst.IsAbsMode;
                    _axis_view.Speed = 100;
                }
            }
            else
            {

                // load the saved position

                btnSave.IsEnabled = true;
                btnDelete.IsEnabled = true;
                btnMove.IsEnabled = true;
                btnStop.IsEnabled = true;


            }
        }

        #endregion

        #region Events

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            InputBox box = new InputBox("Input Preset Name", "Input");
            bool? ret = box.ShowDialog();
            if (ret.HasValue && ret == true)
            {
                ViewMassMove view = DataContext as ViewMassMove;
                view.SavePresetPosition(box.Input);
            }
        }

        #endregion

    }
}
