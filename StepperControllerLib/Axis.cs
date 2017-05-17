using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace IrixiStepperControllerHelper
{
    public class Axis : INotifyPropertyChanged
    {
        /// <summary>
        /// Get the max distance(steps) the axis supports
        /// Typically, this value is set in the application's config files.
        /// </summary>
        public int MaxDistance
        {
            set;
            get;
        }

        /// <summary>
        /// Get the position after the home process
        /// Typically, this value is set in the application's config files.
        /// </summary>
        public int PosAfterHome
        {
            set;
            get;
        }

        /// <summary>
        /// Get the soft CWLS
        /// </summary>
        public int SoftCWLS
        {
            set;
            get;
        }

        /// <summary>
        /// Get the soft CCWLS
        /// </summary>
        public int SoftCCWLS
        {
            set;
            get;
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
            ////RaisePropertyChanged(PropertyName);

        }
        #endregion
    }
}
