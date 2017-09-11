using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Irixi_Aligner_Common.Classes.BaseClass
{
    public class RealworldPositionManager : INotifyPropertyChanged
    {
        public enum UnitType
        {
            mm,
            um,
            nm,
            deg,
            min,
            sec
        }

        #region Variables
        double _abs_pos = 0;
        double _rel_pos = 0;
        #endregion

        public RealworldPositionManager(double TravelDistance, double Resolution, UnitType Unit = UnitType.mm, int ScaleDisplayed = 2)
        {
            this.ScaleDisplayed = ScaleDisplayed;
            this.Unit = Unit;
            this.TravelDistance = TravelDistance;
            this.Resolution = Resolution;
            this.MaxSteps = (int)(this.TravelDistance / this.Resolution);
        }
        #region Properties 

        /// <summary>
        /// Get the maximum travel distance of the stage
        /// </summary>
        public double TravelDistance
        {
            private set;
            get;
        }

        /// <summary>
        /// Get the distance of the stage travels each step
        /// </summary>
        public double Resolution
        {
            private set;
            get;
        }

        /// <summary>
        /// Get the total steps of the stage traveling from one limit position to the other
        /// </summary>
        public int MaxSteps
        {
            private set;
            get;
        }

        /// <summary>
        /// Get the real-world unit used to measure the position in system
        /// </summary>
        public UnitType Unit
        {
            internal set;
            get;
        }

        
        /// <summary>
        /// Get the absolut distance in real world unit
        /// </summary>
        public double AbsPosition
        {
            set
            {
                UpdateProperty<double>(ref _abs_pos, value);
            }
            get
            {
                return Math.Round(_abs_pos, this.ScaleDisplayed);
            }
        }

        /// <summary>
        /// Get the relative distance in real world unit
        /// </summary>
        public double RelPosition
        {
            set
            {
                UpdateProperty<double>(ref _rel_pos, value);
            }

            get
            {
                return Math.Round(_rel_pos, this.ScaleDisplayed);
            }
        }

        /// <summary>
        /// Get the scale of the number that displayed on the screen
        /// </summary>
        public int ScaleDisplayed
        {
            internal set;
            get;
        }

        #endregion


        #region Methods

        public int ConvertPositionToSteps(double RealworldPosition)
        {
            return (int)(RealworldPosition / this.Resolution);
        }

        /// <summary>
        /// set the abs position in steps to convert to real world distance
        /// </summary>
        /// <param name="steps"></param>
        public double ConvertStepsToPosition(int Steps)
        {
            return Steps * this.Resolution;
        }

        /// <summary>
        /// this method exists specially for the Luminos P6A.
        /// For the P6A, the travel distance and max steps are given, 
        /// but the max steps are stored in the external flash of the P6A, so this property should be re-set after
        /// initializing the P6A.
        /// </summary>
        /// <param name="MaxSteps"></param>
        public void ChangeMaxSteps(int MaxSteps)
        {
            this.MaxSteps = MaxSteps;

            // if the property was set, the related properties must be recalculated
            this.Resolution = this.TravelDistance / (double)this.MaxSteps;
        }
        #endregion  

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
            //PropertyChangedEventHandler handler = PropertyChanged;
            //if (handler != null)
            //    handler(this, new PropertyChangedEventArgs(PropertyName));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));

        }

        
        #endregion
    }
}
