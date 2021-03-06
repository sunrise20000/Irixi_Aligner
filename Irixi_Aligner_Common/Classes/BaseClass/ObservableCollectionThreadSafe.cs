﻿using System.Collections.ObjectModel;
using System.Windows;

namespace Irixi_Aligner_Common.Classes.BaseClass
{
    public class ObservableCollectionThreadSafe<T>: System.Collections.ObjectModel.ObservableCollection<T>
    {
        protected override void ClearItems()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                base.ClearItems();
            });

        }

        protected override void InsertItem(int index, T item)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                base.InsertItem(index, item);
            });
        }
    }
}
