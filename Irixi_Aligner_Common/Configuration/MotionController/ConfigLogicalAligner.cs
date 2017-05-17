using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Irixi_Aligner_Common.Configuration
{
    public class ConfigLogicalAligner : INotifyPropertyChanged
    {
        string _displayname = "Unknown";
        /// <summary>
        /// Get or set the name of the aligner which is shown on the UI
        /// </summary>
        public string DisplayName
        {
            set
            {
                UpdateProperty<string>(ref _displayname, value);
            }
            get
            {
                return _displayname;
            }
        }


        public ConfigLogicalAxis AxisX { get; set; }
        public ConfigLogicalAxis AxisY { get; set; }
        public ConfigLogicalAxis AxisZ { get; set; }
        public ConfigLogicalAxis AxisRoll { get; set; }
        public ConfigLogicalAxis AxisYaw { get; set; }
        public ConfigLogicalAxis AxisPitch { get; set; }


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
