using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using GalaSoft.MvvmLight.Views;
using Irixi_Aligner_Common.DialogService.HomeConfirmDialog;
using Irixi_Aligner_Common.DialogService.InputDialog;

namespace Irixi_Aligner_Common.DialogService
{
    public class DialogService : IDialogService
    {

        private bool? ShowDialog(DialogViewBase vm, Window Owner)
        {
            DialogWindow win = new DialogWindow();
            if (Owner != null)
                win.Owner = Owner;
            win.DataContext = vm;
            win.ShowDialog();
            return (win.DataContext as DialogViewBase).DialogResult;
        }

        public void OpenInputDialog(string Title, string Message, Window Owner, Action<string> CallBack)
        {
            InputDialogViewModel vm = new InputDialogViewModel(Title, Message);
            var ret = ShowDialog(vm, Owner);
            if(ret.HasValue && ret == true)
            {
                CallBack(vm.InputText);
            }
        }

        public void OpenHomeConfirmDialog(Window Owner, Action<bool> CallBack)
        {
            HomeConfirmDialogViewModel vm = new HomeConfirmDialogViewModel("Confirm to Home", "");
            var ret = ShowDialog(vm, Owner);
            if(ret.HasValue)
            {
                CallBack(ret.Value);
            }
        }

        public Task ShowError(string message, string title, string buttonText, Action afterHideCallback)
        {
            return Task.Run(() =>
            {
                System.Windows.Forms.MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                afterHideCallback.Invoke();

            });
        }

        public Task ShowError(Exception error, string title, string buttonText, Action afterHideCallback)
        {
            return Task.Run(() =>
            {
                System.Windows.Forms.MessageBox.Show(error.Message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                afterHideCallback.Invoke();

            });
        }

        public Task ShowMessage(string message, string title)
        {
            return Task.Run(() =>
            {
                System.Windows.Forms.MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
            });
        }

        public Task ShowMessage(string message, string title, string buttonText, Action afterHideCallback)
        {
            return Task.Run(() =>
            {
                System.Windows.Forms.MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
            });
        }

        public Task<bool> ShowMessage(string message, string title, string buttonConfirmText, string buttonCancelText, Action<bool> afterHideCallback)
        {
            throw new NotImplementedException();
        }

        public Task ShowMessageBox(string message, string title)
        {
            throw new NotImplementedException();
        }
    }
}
