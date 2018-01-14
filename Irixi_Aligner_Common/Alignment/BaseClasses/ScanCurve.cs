using Irixi_Aligner_Common.Classes.BaseClass;
using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Irixi_Aligner_Common.Alignment.BaseClasses
{
    public class ScanCurve : ScanCurveBase<Point>
    {

        public ScanCurve() : base()
        {
            Construct();
        }

        public ScanCurve(string DisplayName) : base()
        {
            Construct();
        }

        private void Construct()
        {
            MaxPowerConstantLine = new ObservableCollectionThreadSafe<Point>();

            this.CollectionChanged += ((s, e)=>
            {
                if (e.Action == NotifyCollectionChangedAction.Reset)
                    MaxPowerConstantLine.Clear();
            });
        }


        /// <summary>
        /// The constant line indicates where is the max power of the Scan Curve
        /// </summary>
        public ObservableCollectionThreadSafe<Point> MaxPowerConstantLine
        {
            private set;
            get;
        }

        /// <summary>
        /// Get the factors list of the polynomial
        /// </summary>
        public double[] PolyFittingEquationFactors { private set; get; }

        /// <summary>
        /// Get the polynomial equation
        /// </summary>
        /// <returns></returns>
        private Func<double, double> PolyFittingEqution
        {
            get
            {
                return new Func<double, double>(x =>
                {
                    double y = 0;
                    for (int p = 0; p < PolyFittingEquationFactors.Length; p++)
                        y += PolyFittingEquationFactors[p] * Math.Pow(x, p);
                    return y;
                });
            }
        }
        
        /// <summary>
        /// Fit the scan curve to a polynomial with the order defined inside of the methods
        /// </summary>
        /// <returns></returns>
        private double[] PolyFit()
        {
            const int FITTING_ORDER = 3;

            if (this.Count > 1)
            {
                List<double> xData = new List<double>();
                List<double> yData = new List<double>();

                foreach (var p in this)
                {
                    xData.Add(p.X);
                    yData.Add(p.Y);
                }

                PolyFittingEquationFactors = Fit.Polynomial(xData.ToArray(), yData.ToArray(), FITTING_ORDER);
                return PolyFittingEquationFactors;
            }
            else
            {
                PolyFittingEquationFactors = null;
                throw new FormatException("There are not enough points to fit.");
            }
        }
        
        /// <summary>
        /// Calculate the equation of the fitting curve and return the fitting curve by a list
        /// </summary>
        /// <returns></returns>
        public List<Point> GetBeautifiedCurve()
        {
            // the fitting curve consist of these points
            const double POINTS_IN_BEAUTIFY_CURVE = 20;

            List<Point> curve = new List<Point>();

            // fitting the curve
            double[] equation = PolyFit();

            // the range to draw the fitting curve
            double range = this.Last().X - this.First().X;

            // the start point to draw the fitting curve
            double start = this.First().X;

            // the steps to draw the fitting curve
            double step = range / POINTS_IN_BEAUTIFY_CURVE;

            for (int i = 0; i < POINTS_IN_BEAUTIFY_CURVE; i++)
            {
                double x = start + i * step;
                double y = PolyFittingEqution(x);
                curve.Add(new Point(x, y));
            }

            return curve;
        }

        /// <summary>
        /// Calculate the position where the maximal value is in the scan curve.
        /// The 3-order polynomal are used to fit the scan curve.
        /// </summary>
        /// <returns></returns>
        public Point FindMaximalPosition()
        {
            //! NOTE:
            //! The function base on the condition that the polynomial order to fit is 3
            //! See the function this.PolyFit() for the detail

            // calculate the roots of the derivation of the 3-order polynomial to find the two extremum of the polynomial
            // build the factor of the derivation, the format of the equation is like ax^2 + bx + c = 0,
            // the roots of the equation are x = [-b ± sqrt(b^2 - 4ac)] / 2a
            double a = 3 * PolyFittingEquationFactors[3];
            double b = 2 * PolyFittingEquationFactors[2];
            double c = PolyFittingEquationFactors[1];

            double[] root = new double[2];
            root[0] = (-b + Math.Sqrt(b * b - 4 * a * c)) / 2 * a;
            root[1] = (-b - Math.Sqrt(b * b - 4 * a * c)) / 2 * a;

            // the condition of the maximal value: f'(x) = 0 and f''(x) < 0
            // calculate the factors of f''(x)
            a = 2 * PolyFittingEquationFactors[2];
            b = 6 * PolyFittingEquationFactors[3];
            double x_at_maximal = 0;
            if ((a + b * root[0]) < 0)
                x_at_maximal = root[0];
            else if ((a + b * root[1]) < 0)
                x_at_maximal = root[1];
            else
                throw new InvalidOperationException(string.Format(
                    "unable to find the maximal value of the 3-order polynomial, the factors of the poly func are [{0}, {1}, {2}, {3}], the roots of secondary derivation are [{4}, {5}].",
                    new object[] { PolyFittingEquationFactors[0], PolyFittingEquationFactors[1], PolyFittingEquationFactors[2], PolyFittingEquationFactors[3], root[0], root[1] }));

            // return the position where the maximal value is
            Point maxPoint;
            if (this.First().X > x_at_maximal)
                maxPoint = this.First();
            else if (this.Last().X < x_at_maximal)
                maxPoint = this.Last();
            else
            {
                // calulate extremum
                double extremum = PolyFittingEqution(x_at_maximal);
                maxPoint = new Point(x_at_maximal, extremum);
            }

            MaxPowerConstantLine.Clear();
            MaxPowerConstantLine.Add(new Point(maxPoint.X, 0));
            MaxPowerConstantLine.Add(maxPoint);

            return maxPoint;
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
