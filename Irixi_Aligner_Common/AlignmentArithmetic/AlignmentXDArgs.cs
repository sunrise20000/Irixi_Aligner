using Irixi_Aligner_Common.Interfaces;
using Irixi_Aligner_Common.MotionControllerEntities.BaseClass;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace Irixi_Aligner_Common.AlignmentArithmetic
{
    public class AlignmentXDArgs : AlignmentArgsBase
    {
        #region Variables
        private IMeasurementInstrument instrument;
        private LogicalMotionComponent motionComponent;
        private double target;
        private int maxCycles;
        private ObservableCollection<Alignment1DArgs> axisParamCollection;
        #endregion

        #region Constructors

        public AlignmentXDArgs()
        {
            this.target = 0;
            this.maxCycles = 1;

            AxisParamCollection = new ObservableCollection<Alignment1DArgs>();
        }

        #endregion

        #region Properties

        public IMeasurementInstrument Instrument
        {
            get => instrument;
            set
            {
                instrument = value;
                RaisePropertyChanged();
            }
        }

        public LogicalMotionComponent MotionComponent
        {
            get => motionComponent;
            set
            {
                motionComponent = value;
                RaisePropertyChanged();

                // add new editors
                AxisParamCollection.Clear();
                foreach (var axis in value.LogicalAxisCollection)
                {
                    var arg = new Alignment1DArgs()
                    {
                        Axis = axis,
                        IsEnabled = false,
                        MoveSpeed = 100,
                        Interval = 10,
                        ScanRange = 100,
                        ScanOrder = 0,
                        MaxOrder = value.LogicalAxisCollection.Count
                    };

                    AxisParamCollection.Add(arg);
                }
            }
        }

        public double Target
        {
            get => target;
            set
            {
                target = value;
                RaisePropertyChanged();
            }
        }

        public int MaxCycles
        {
            get => maxCycles;
            set
            {
                maxCycles = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<Alignment1DArgs> AxisParamCollection
        {
            get => axisParamCollection;

            set
            {
                axisParamCollection = value;
            }
        }

        #endregion

        #region Methods
        
        public override void Validate()
        {

            if (Instrument == null)
                throw new ArgumentException("no instrument selected");

            if (Instrument.IsEnabled == false)
                throw new ArgumentException("the instrument is disabled");

            if (MotionComponent == null)
                throw new ArgumentException("no motion component selected");

            if(Target <= 0)
                throw new ArgumentException("the measurement target is not set");

            if(MaxCycles <= 0)
                throw new ArgumentException("the max cycles is not set");

            if (AxisParamCollection == null || AxisParamCollection.Count < 1)
                throw new ArgumentNullException("not axis param found");
            
            foreach(var arg in AxisParamCollection)
            {
                // check if the align order is unique
                if (arg.IsEnabled)
                {
                    if(arg.ScanRange <= 0)
                        throw new ArgumentException(string.Format("the range is error"));

                    if(arg.Interval <= 0)
                        throw new ArgumentException(string.Format("the step is error"));

                    if(arg.MoveSpeed < 0 || arg.MoveSpeed > 100)
                        throw new ArgumentException(string.Format("the speed is out of range"));
                }
            }
        }

        public override void ClearScanCurve()
        {
            foreach (var arg in AxisParamCollection)
            {
                arg.ClearScanCurve();
            }
        }

        public override void PauseInstruments()
        {
            Instrument.PauseAutoFetching();
        }

        public override void ResumeInstruments()
        {
            Instrument.ResumeAutoFetching();
        }

        #endregion
    }
}
