using System.ComponentModel;
using System.Runtime.CompilerServices;
using Irixi_Aligner_Common.Classes.BaseClass;

namespace Irixi_Aligner_Common.Alignment.BaseClasses
{
    public class ScanCurveBase<T>: ObservableCollectionThreadSafe<T>, INotifyPropertyChanged
        where T:struct
    {
        string displayName = "-";

        public ScanCurveBase()
        {
        }

        public ScanCurveBase(string DisplayName)
        {
            this.DisplayName = DisplayName;
        }

        /// <summary>
        /// Get the name of the curve
        /// </summary>
        public string DisplayName
        {
            set
            {
                UpdateProperty(ref displayName, value);
            }
            get
            {
                return displayName;
            }
        }


        #region RaisePropertyChangedEvent

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="OldValue"></param>
        /// <param name="NewValue"></param>
        /// <param name="PropertyName"></param>
        protected void UpdateProperty<T>(ref T OldValue, T NewValue, [CallerMemberName]string PropertyName = "")
        {
            OldValue = NewValue;                // Set the property value to the new value
            OnPropertyChanged(PropertyName);    // Raise the notify event
        }

        protected void OnPropertyChanged([CallerMemberName]string PropertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        #endregion
    }
}
