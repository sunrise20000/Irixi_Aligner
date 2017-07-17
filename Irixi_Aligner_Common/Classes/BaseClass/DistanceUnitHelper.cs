using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Irixi_Aligner_Common.Classes.BaseClass
{
    public class RealworldDistanceUnitHelper
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
            this.Dps = MaxStroke / MaxSteps;
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
            set;
            get;
        }

        public UnitType Unit
        {
            private set;
            get;
        }

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
    }
}
