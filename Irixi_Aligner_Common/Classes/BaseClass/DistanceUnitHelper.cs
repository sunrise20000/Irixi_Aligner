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
            nm
        }

        /// <summary>
        /// Convert the steps to real world distance, vice versa
        /// </summary>
        /// <param name="Dps">distance per step</param>
        /// <param name="DpsUnit">the unit of dps</param>
        /// <param name="Digits">decimal digits of distance shown on window</param>
        public RealworldDistanceUnitHelper( double Dps, UnitType DpsUnit, int Digits)
        {
            this.Digits = Digits;
            this.DpsUnit = DpsUnit;
            this.Dps = Dps;
        }

        public int Digits
        {
            private set;
            get;
        }

        public double Dps
        {
            private set;
            get;
        }

        public UnitType DpsUnit
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
