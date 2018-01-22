using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using DevExpress.Xpf.Charts;
using Irixi_Aligner_Common.Classes.BaseClass;
using Irixi_Aligner_Common.Interfaces;

namespace Irixi_Aligner_Common.Alignment.BaseClasses
{
    public class ScanCurveBase<T>: ObservableCollectionThreadSafe<T>, IScanCurve, INotifyPropertyChanged
        where T:struct
    {
        string displayName = "-", prefix = "", suffix = "";

        public ScanCurveBase()
        {
            Constructor();
        }

        public ScanCurveBase(string DisplayName)
        {
            Constructor();
            this.DisplayName = DisplayName;
        }

        private void Constructor()
        {
            LineStyle = new LineStyle(2);
            MarkerVisible = false;
            MarkerSize = 7;
            MarkerModel = new CircleMarker2DModel();
            Visible = true;
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
                return string.Join(" ", new object[] { prefix, displayName, suffix });
            }
        }

        public string Prefix
        {
            set
            {
                UpdateProperty(ref prefix, value);
                OnPropertyChanged("DisplayName");
            }
            get
            {
                return prefix;
            }
        }

        public string Suffix
        {
            set
            {
                UpdateProperty(ref suffix, value);
                OnPropertyChanged("DisplayName");
            }
            get
            {
                return suffix;
            }
        }
        
        public LineStyle LineStyle { set; get; }
        public Marker2DModel MarkerModel { get; set; }
        public int MarkerSize { get; set; }
        public bool MarkerVisible { get; set; }
        public SolidColorBrush Brush { set; get; }
        public bool Visible{ set; get; }


        #region RaisePropertyChangedEvent

        public event PropertyChangedEventHandler PropertyChanged;
        
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="OldValue"></param>
        /// <param name="NewValue"></param>
        /// <param name="PropertyName"></param>
        protected void UpdateProperty<X>(ref X OldValue, X NewValue, [CallerMemberName]string PropertyName = "")
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
