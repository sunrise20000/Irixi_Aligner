using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using GalaSoft.MvvmLight;
using Irixi_Aligner_Common.Classes;
using Irixi_Aligner_Common.Classes.BaseClass;
using Irixi_Aligner_Common.Interfaces;
using Irixi_Aligner_Common.MotionControllers.Base;

namespace Irixi_Aligner_Common.Alignment.BaseClasses
{
    public class AlignmentArgsBase : ViewModelBase
    {
        #region Variables
        protected const string PROP_GRP_COMMON = "Common";
        protected const string PROP_GRP_TARGET = "Goal";

        IInstrument instrument;
        LogicalMotionComponent motionComponent;
        int moveSpeed = 100;
        string axisXTitle = "", axisYTitle = "", axisY2Title = "", axisZTitle = "";

        #endregion

        #region Constructors
        
        public AlignmentArgsBase(SystemService Service)
        {
            Log = new ObservableCollectionThreadSafe<string>();
            ScanCurveGroup = new ScanCurveGroup();

            Properties = new ObservableCollectionEx<Property>();
            this.Service = Service;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Get what properties are allowed to edit
        /// </summary>
        [Browsable(false)]
        public ObservableCollectionEx<Property> Properties
        {
            private set;
            get;
        }

        /// <summary>
        /// The instance of System Service Class
        /// </summary>
        [Browsable(false)]
        public SystemService Service
        {
            private set;
            get;
        }

        [Browsable(false)]
        public string AxisXTitle
        {
            set
            {
                axisXTitle = value;
                RaisePropertyChanged();
            }
            get
            {
                return axisXTitle;
            }
        }

        [Browsable(false)]
        public string AxisYTitle
        {
            set
            {
                axisYTitle = value;
                RaisePropertyChanged();
            }
            get
            {
                return axisYTitle;
            }
        }

        [Browsable(false)]
        public string AxisY2Title
        {
            set
            {
                axisY2Title = value;
                RaisePropertyChanged();
            }
            get
            {
                return axisY2Title;
            }
        }

        [Browsable(false)]
        public string AxisZTitle
        {
            set
            {
                axisZTitle = value;
                RaisePropertyChanged();
            }
            get
            {
                return axisZTitle;
            }
        }

        [Browsable(false)]
        public ScanCurveGroup ScanCurveGroup { private set; get; }

        [Browsable(false)]
        public ObservableCollectionThreadSafe<string> Log
        {
            get;
        }
        

        [Display( 
            Order = 100, 
            Name = "Motion Component", 
            GroupName = PROP_GRP_COMMON, 
            Description = "Which motion component belongs to of the axes to align.")]
        public LogicalMotionComponent MotionComponent
        {
            get => motionComponent;
            set
            {
                motionComponent = value;
                RaisePropertyChanged();
            }
        }

        [Display(
            Order = 200,
            Name = "Instrument",
            GroupName = PROP_GRP_COMMON,
            Description = "The valid instrument like powermeter, keithley 2400, etc.")]
        public virtual IInstrument Instrument
        {
            get => instrument;
            set
            {
                instrument = value;
                RaisePropertyChanged();
            }
        }

        [Display(
            Order = 300,
            Name = "Move Speed(%)", 
            GroupName = PROP_GRP_COMMON, 
            Description = "The move speed while aligning which is in %, the range is 1 - 100.")]
        public int MoveSpeed
        {
            get => moveSpeed;
            set
            {
                moveSpeed = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Validate the parameters
        /// </summary>
        public virtual void Validate()
        {
            if(MoveSpeed < 1 || MoveSpeed > 100)
                throw new ArgumentException("move speed must be between 1 ~ 100");

            if(Instrument == null)
                throw new ArgumentException(string.Format("you must specify the {0}",
                    ((DisplayAttribute)TypeDescriptor.GetProperties(this)["Instrument"].Attributes[typeof(DisplayAttribute)]).Name) ?? "instrument");
        }

        /// <summary>
        /// Clear the previous points scan curve
        /// </summary>
        public virtual void ClearScanCurve()
        {
            this.ScanCurveGroup.ClearCurvesContent();
        }

        /// <summary>
        /// Pause the feedback instruments due to the software reads the instruments continuously, the communication port
        /// is occupied, so the background reading loop should be halted while alignment process reading the instruments
        /// </summary>
        public virtual void PauseInstruments()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Release the instruments
        /// <see cref="PauseInstruments"/>
        /// </summary>
        public virtual void ResumeInstruments()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
