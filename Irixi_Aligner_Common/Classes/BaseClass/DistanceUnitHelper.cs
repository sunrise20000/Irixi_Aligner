using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Irixi_Aligner_Common.Classes.BaseClass
{
    public class RealworldDistanceUnitHelper : INotifyPropertyChanged, ICloneable
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

        /// <summary>
        /// Convert the steps to real world distance, vice versa
        /// </summary>
        /// <param name="Dps">distance per step</param>
        /// <param name="Unit">the unit display on the window</param>
        /// <param name="Digits">decimal digits of distance shown on window</param>
        public RealworldDistanceUnitHelper(int MaxSteps, double MaxStroke, UnitType Unit, int Digits)
        {
            this.Digits = Digits;
            this.Unit = Unit;
            this.MaxSteps = MaxSteps;
            this.MaxStroke = MaxStroke;
            this.Dps = MaxStroke / MaxSteps;
        }

        #region Properties 
        public int MaxSteps
        {
            private set;
            get;
        }

        public double MaxStroke
        {
            private set;
            get;
        }

        public int Digits
        {
            private set;
            get;
        }

        /// <summary>
        /// Set or get the real world distance per step
        /// </summary>
        public double Dps
        {
            private set;
            get;
        }

        public UnitType Unit
        {
            private set;
            get;
        }


        double _abs_distance = 0;
        /// <summary>
        /// Get the absolut distance in real world unit
        /// </summary>
        public double AbsDistance
        {
            private set
            {
                this.RelDistance += (value - _abs_distance);
                UpdateProperty<double>(ref _abs_distance, value);
            }
            get
            {
                return Math.Round(_abs_distance, this.Digits);
            }
        }

        double _rel_distance = 0;
        /// <summary>
        /// Get the relative distance in real world unit
        /// </summary>
        public double RelDistance
        {
            private set
            {
                UpdateProperty<double>(ref _rel_distance, value);
            }

            get
            {
                return Math.Round(_rel_distance, this.Digits);
            }
        }
        #endregion


        #region Methods
        private double ConvertUnit(double Value, UnitType InputUnit, UnitType OutputUnit)
        {
            double _tmp = 0;

            // Convert to nm
            switch(InputUnit)
            {
                case UnitType.mm:
                    _tmp = Value * 1000000;
                    break;

                case UnitType.um:
                    _tmp = Value * 1000;
                    break;

                case UnitType.nm:
                    _tmp = Value;
                    break;

            }

            // Convert to the specified unit
            switch(OutputUnit)
            {
                case UnitType.mm:
                    _tmp /= 1000000;
                    break;

                case UnitType.um:
                    _tmp /= 1000;
                    break;

                case UnitType.nm:

                    break;
            }

            return _tmp;
        }

        public int ConvertToSteps(double RealworldDistance)
        {
            return (int)(RealworldDistance / this.Dps);
        }

        public double ConvertToRealworldDistance(int Steps)
        {
            return Math.Round(Steps * this.Dps, this.Digits);
        }

        /// <summary>
        /// set the abs position in steps to convert to real world distance
        /// </summary>
        /// <param name="steps"></param>
        public void SetAbsPosition(int steps)
        {
            this.AbsDistance = ConvertToRealworldDistance(steps);
        }

        public object Clone()
        {
            return new RealworldDistanceUnitHelper(this.MaxSteps, this.MaxStroke, this.Unit, this.Digits);
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
