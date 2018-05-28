using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Irixi_Aligner_Common.Alignment.BaseClasses;
using Irixi_Aligner_Common.Classes;
using Irixi_Aligner_Common.Classes.BaseClass;
using Irixi_Aligner_Common.Equipments.Base;
using Irixi_Aligner_Common.Interfaces;
using Irixi_Aligner_Common.MotionControllers.Base;

namespace Irixi_Aligner_Common.Alignment.CentralAlign
{
    public class CentralAlignArgs : AlignmentArgsBase
    {
        #region Variables
        const string PROP_GRP_H_AXIS = "Horizontal Axis";
        const string PROP_GRP_V_AXIS = "Vertical Axis";

        IInstrument instrument, instrument2;

        LogicalAxis axis, axis2;
        double hScanInterval = 1, vScanInterval = 1, axisRestriction = 100, axis2Restriction = 100;


        #endregion

        #region Constructors

        public CentralAlignArgs(SystemService Service) : base(Service)
        {
            ScanCurve = new ScanCurve();
            ScanCurve2 = new ScanCurve();
            ScanCurveGroup.Add(ScanCurve);
            ScanCurveGroup.Add(ScanCurve2);
            ScanCurveGroup.Add(ScanCurve.FittingCurve);
            ScanCurveGroup.Add(ScanCurve2.FittingCurve);
            ScanCurveGroup.Add(ScanCurve.MaxPowerConstantLine);
            ScanCurveGroup.Add(ScanCurve2.MaxPowerConstantLine);

            AxisXTitle = "Position";
            AxisYTitle = "Indensity";

            Properties.Add(new Property("Instrument"));
            Properties.Add(new Property("Instrument2"));
            Properties.Add(new Property("MoveSpeed"));

            Properties.Add(new Property("Axis"));
            Properties.Add(new Property("ScanIntervalHorizontal"));
            Properties.Add(new Property("AxisRestriction"));
            Properties.Add(new Property("Axis2"));
            Properties.Add(new Property("ScanIntervalVertical"));
            Properties.Add(new Property("Axis2Restriction"));

            
        }

        #endregion


        #region Properties
        [Browsable(false)]
        public ScanCurve ScanCurve
        {
            private set;
            get;
        }

        [Browsable(false)]
        public ScanCurve ScanCurve2
        {
            private set;
            get;
        }

        [Display(
            Order = 10,
            Name = "H Axis",
            GroupName = PROP_GRP_H_AXIS,
            Description = "The scan process starts along the horizontal axis first.")]
        public LogicalAxis Axis
        {
            get => axis;
            set
            {
                axis = value;
                RaisePropertyChanged();
            }
        }

        [Browsable(false)]
        public string AxisHashString
        {
            private get; set;
        }

        [Display(
            Name = "H Interval",
            GroupName = PROP_GRP_H_AXIS,
            Description = "The scan interval for both H-Axis.")]
        public double ScanIntervalHorizontal
        {
            get => hScanInterval;
            set
            {
                hScanInterval = value;
                RaisePropertyChanged();
            }
        }

        [Display(
           Name = "H Restri.",
           GroupName = PROP_GRP_H_AXIS,
           Description = "The scan scope restriction of the horizontal axis.")]
        public double AxisRestriction
        {
            get => axisRestriction;
            set
            {
                axisRestriction = value;
                RaisePropertyChanged();
            }
        }

        [Display(
            Name = "V Axis",
            GroupName = PROP_GRP_V_AXIS,
            Description = "The V-Axis moves to positive direction after the H-Axis scanning.")]
        public LogicalAxis Axis2
        {
            get => axis2;
            set
            {
                axis2 = value;
                RaisePropertyChanged();
            }
        }

        [Browsable(false)]
        public string Axis2HashString
        {
            private get; set;
        }

        [Display(
            Name = "V Interval",
            GroupName = PROP_GRP_V_AXIS,
            Description = "The scan interval for both V-Axis.")]
        public double ScanIntervalVertical
        {
            get => vScanInterval;
            set
            {
                vScanInterval = value;
                RaisePropertyChanged();
            }
        }

        [Display(
            Name = "V Restri.",
            GroupName = PROP_GRP_V_AXIS,
            Description = "The scan scope restriction of the vertical axis.")]
        public double Axis2Restriction
        {
            get => axis2Restriction;
            set
            {
                axis2Restriction = value;
                RaisePropertyChanged();
            }
        }
        

        [Display(
            Order = 1,
            Name = "Instrument 1",
            GroupName = PROP_GRP_COMMON,
            Description = "The valid instrument like powermeter, keithley 2400, etc.")]
        new public IInstrument Instrument
        {
            get => instrument;
            set
            {
                instrument = value;
                RaisePropertyChanged();

                ScanCurve.DisplayName = ((InstrumentBase)instrument).Config.Caption;
            }
        }

        [Display(
            Order = 2,
            Name = "Instrument 2",
            GroupName = PROP_GRP_COMMON,
            Description = "The valid instrument like powermeter, keithley 2400, etc.")]
        public IInstrument Instrument2
        {
            get => instrument2;
            set
            {
                instrument2 = value;
                RaisePropertyChanged();

                ScanCurve2.DisplayName = ((InstrumentBase)instrument2).Config.Caption;
            }
        }


        [Browsable(false)]
        public string Instrument2HashString
        { 
             private get; set;
        }

        #endregion

        #region Methods

        public override void Validate()
        {
            //base.Validate();

            if (MoveSpeed < 1 || MoveSpeed > 100)
                throw new ArgumentException("move speed must be between 1 ~ 100");

            if (Instrument == null)
                throw new ArgumentException(string.Format("you must specify the {0}",
                    ((DisplayAttribute)TypeDescriptor.GetProperties(this)["Instrument"].Attributes[typeof(DisplayAttribute)]).Name) ?? "instrument");

            if (Instrument2 == null)
                throw new ArgumentException(string.Format("you must specify the {0}",
                    ((DisplayAttribute)TypeDescriptor.GetProperties(this)["Instrument2"].Attributes[typeof(DisplayAttribute)]).Name) ?? "instrument2");

            if (Instrument == Instrument2)
                throw new ArgumentException("the two instruments must be different.");


            if (Axis == null)
                throw new ArgumentException("You must specify the horizontal axis.");

            if (Axis2 == null)
                throw new ArgumentException("You must specify the vertical axis.");

            if (Axis == Axis2)
                throw new ArgumentException("The horizontal axis and the vertical axis must be different.");

            if (Axis.PhysicalAxisInst.UnitHelper.Unit != Axis2.PhysicalAxisInst.UnitHelper.Unit)
                throw new ArgumentException("the two axes have different unit");
        }

        public override void PauseInstruments()
        {
            Instrument.PauseAutoFetching();
            Instrument2.PauseAutoFetching();
        }

        public override void ResumeInstruments()
        {
            Instrument.ResumeAutoFetching();
            Instrument2.ResumeAutoFetching();
        }

        #endregion
    }
}
