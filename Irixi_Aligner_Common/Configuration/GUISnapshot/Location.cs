using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Irixi_Aligner_Common.Configuration
{
    public class Location : INotifyPropertyChanged
    {
        string _mdilocation = "";
        public string MDILocation
        {
            set
            {
                UpdateProperty<string>(ref _mdilocation, value);
            }
            get
            {
                return _mdilocation;
            }
        
        }

        bool _visibility = false;
        public bool Visibility
        {
            set
            {
                UpdateProperty<bool>(ref _visibility, value);
            }
            get
            {
                return _visibility;
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
            if (object.Equals(OldValue, NewValue))  // To save resource, if the value is not changed, do not raise the notify event
                return;

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
