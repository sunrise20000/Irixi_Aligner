using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Irixi_Aligner_Common.Classes.BaseClass
{
    public class AxisMoveArgs : EventArgs, INotifyPropertyChanged, ICloneable
    {
        #region Variables

        string axisCaption = "null";
        int speed = 50;
        double distance = 0;
        MoveMode mode = MoveMode.ABS;
        bool isMoveable = false;
        int moveOrder = 0;
        int maxMoveOrder = 0;
        string unit = "um";

        #endregion

        #region Constructors

        public AxisMoveArgs()
        {

        }

        public AxisMoveArgs(MoveMode Mode, int Speed, double Distance, string Unit)
        {
            this.Mode = Mode;
            this.Speed = Speed;
            this.Distance = Distance;
            this.Unit = Unit;
        }

        //public AxisMoveArgs(MoveMode Mode, int Speed, double Distance, bool Moveable, int MoveOrder, int MaxMoveOrder)
        //{
        //    this.Mode = Mode;
        //    this.Speed = Speed;
        //    this.Distance = Distance;
        //    this.IsMoveable = Moveable;
        //    this.MoveOrder = MoveOrder;
        //    this.MaxMoveOrder = MaxMoveOrder;
        //}

        #endregion

        #region Properties

        public string LogicalAxisHashString { get; set; }


        /// <summary>
        /// Get or set the caption of axis which is defined in the config file
        /// <see cref="Irixi_Aligner_Common.Configuration.MotionController.ConfigLogicalAxis.DisplayName"/>
        /// </summary>
        public string AxisCaption
        {
            get
            {
                return axisCaption;
            }
            set
            {
                UpdateProperty(ref axisCaption, value);
            }
        }

        /// <summary>
        /// The speed to move in %
        /// </summary>
        public int Speed
        {
            get
            {
                return speed;
            }
            set
            {
                UpdateProperty(ref speed, value);
            }
        }

        /// <summary>
        /// The distance to move
        /// </summary>
        public double Distance
        {
            get
            {
                return distance;
            }
            set
            {
                UpdateProperty(ref distance, value);
            }
        }

        /// <summary>
        /// Abs/Rel
        /// </summary>
        public MoveMode Mode
        {
            get
            {
                return mode;
            }
            set
            {
                UpdateProperty(ref mode, value);
            }
        }

        /// <summary>
        /// Is it moveable? The property is specically used in Mass Move Functions
        /// </summary>
        public bool IsMoveable
        {
            get
            {
                return isMoveable;
            }
            set
            {
                UpdateProperty(ref isMoveable, value);
            }
        }

        /// <summary>
        /// The order in the queue. The property is specically used in Mass Move Functions
        /// </summary>
        public int MoveOrder
        {
            get
            {
                return moveOrder;
            }
            set
            {
                UpdateProperty(ref moveOrder, value);
            }
        }

        /// <summary>
        /// How many axes to be moved in the queue. The property is specically used in Mass Move Functions
        /// </summary>
        public int MaxMoveOrder
        {
            get
            {
                return maxMoveOrder;
            }
            set
            {
                UpdateProperty(ref maxMoveOrder, value);
            }
        }

        /// <summary>
        /// Get the unit of the distance to move
        /// </summary>
        public string Unit
        {
            get
            {
                return unit;
            }
            set
            {
                UpdateProperty(ref unit, value);
            }
        }

        #endregion

        #region Methods

        public object Clone()
        {
            var obj = new AxisMoveArgs()
            {
                LogicalAxisHashString = this.LogicalAxisHashString,
                AxisCaption = this.AxisCaption,
                Speed = this.Speed,
                Distance = this.Distance,
                Mode = this.Mode,
                Unit = this.Unit
            };

            return obj;
        }

        public override string ToString()
        {
            return string.Format("{0}/{1}/{2}{3}",
                new object[]
                {
                    Mode,
                    Speed,
                    Distance,
                    Unit
                });
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
            //if (object.Equals(OldValue, NewValue))  // To save resource, if the value is not changed, do not raise the notify event
            //    return;

            OldValue = NewValue;                // Set the property value to the new value
            OnPropertyChanged(PropertyName);    // Raise the notify event
        }

        protected void OnPropertyChanged([CallerMemberName]string PropertyName = "")
        {
            //PropertyChangedEventHandler handler = PropertyChanged;
            //if (handler != null)
            //    handler(this, new PropertyChangedEventArgs(PropertyName));
            //RaisePropertyChanged(PropertyName);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));

        }

        #endregion
    }
}
