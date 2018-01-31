using Irixi_Aligner_Common.Alignment.BaseClasses;
using Irixi_Aligner_Common.Classes;
using Irixi_Aligner_Common.Classes.BaseClass;
using Irixi_Aligner_Common.MotionControllers.Base;
using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Irixi_Aligner_Common.Alignment.AlignmentXD
{
    public class AlignmentXDArgs : AlignmentArgsBase
    {
        #region Variables
        LogicalMotionComponent motionComponent;
        double target;
        int maxCycles;
        int maxOrder;
        ObservableCollectionEx<int> listScanOrder;

       ObservableCollection<Alignment1DArgs> axisParamCollection;
       ReadOnlyObservableCollection<Alignment1DArgs> readonlyAxisParamCollection;
        #endregion

        #region Constructors

        public AlignmentXDArgs(SystemService Service):base(Service)
        {
            this.target = 0;
            this.maxCycles = 1;

            // build valid scan order
            listScanOrder = new ObservableCollectionEx<int>();

            // build list contains each single axis parameter object
            axisParamCollection = new ObservableCollection<Alignment1DArgs>();
            readonlyAxisParamCollection = new ReadOnlyObservableCollection<Alignment1DArgs>(axisParamCollection);

            Properties.Add(new Property("MotionComponent"));
            Properties.Add(new Property("Instrument"));
            Properties.Add(new Property("Target"));
            Properties.Add(new Property("MaxCycles"));
            Properties.Add(new Property() { CollectionName = "AxisParamCollection" });

            AxisXTitle = "Position";
            AxisYTitle = "Indensity";

        }

        #endregion

        #region Properties

        [Display(
            Name = "Motion Component",
            GroupName = PROP_GRP_COMMON,
            Description = "Which motion component belongs to of the axes to align.")]
        new public LogicalMotionComponent MotionComponent
        {
            get => motionComponent;
            set
            {
                motionComponent = value;
                RaisePropertyChanged();

                // add new editors
                axisParamCollection.Clear();
                ScanCurveGroup.Clear();
                foreach (var axis in value.LogicalAxisCollection)
                {
                    var arg = new Alignment1DArgs(this.Service)
                    {
                        Axis = axis,
                        IsEnabled = false,
                        MoveSpeed = 100,
                        Interval = 10,
                        ScanRange = 100,
                        ScanOrder = 1
                    };

                    axisParamCollection.Add(arg);
                    ScanCurveGroup.Add(arg.ScanCurve);
                    //ScanCurveGroup.Add(arg.ScanCurve.MaxPowerConstantLine);
                }

                this.MaxOrder = value.LogicalAxisCollection.Count;
            }
        }

        [Display(
            Name = "Target",
            GroupName = PROP_GRP_COMMON,
            Description = "")]
        public double Target
        {
            get => target;
            set
            {
                target = value;
                RaisePropertyChanged();
            }
        }

        [Display(
            Name = "Max Cycles",
            GroupName = PROP_GRP_COMMON,
            Description = "")]
        public int MaxCycles
        {
            get => maxCycles;
            set
            {
                maxCycles = value;
                RaisePropertyChanged();
            }
        }

        [Display(
            Name = "Axes Setting",
            GroupName = PROP_GRP_COMMON,
            Description = "")]
        public ReadOnlyObservableCollection<Alignment1DArgs> AxisParamCollection
        {
            get => readonlyAxisParamCollection;
        }

        [Browsable(false)]
        public int MaxOrder
        {
            set
            {
                maxOrder = value;

                listScanOrder.Clear();
                for (int i = 1; i <= maxOrder; i++)
                {
                    listScanOrder.Add(i);
                }
            }
            get => maxOrder;
        }

        [Browsable(false)]
        public ObservableCollectionEx<int> ListScanOrder
        {
            get => listScanOrder;
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
