using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using GalaSoft.MvvmLight;
using Irixi_Aligner_Common.Classes;
using Irixi_Aligner_Common.Classes.BaseClass;
using Irixi_Aligner_Common.Interfaces;
using Irixi_Aligner_Common.MotionControllers.Base;
using Newtonsoft.Json;

namespace Irixi_Aligner_Common.Alignment.BaseClasses
{
    public class AlignmentArgsBase : ViewModelBase, IAlignmentArgs
    {
        #region Variables

        protected const string PROP_GRP_COMMON = "Common";
        protected const string PROP_GRP_TARGET = "Goal";

        private IInstrument instrument;
        private LogicalMotionComponent motionComponent;
        private int moveSpeed = 100;
        private string axisXTitle = "", axisYTitle = "", axisY2Title = "", axisZTitle = "";

        #endregion Variables

        #region Constructors

        public AlignmentArgsBase(SystemService Service)
        {
            Log = new ObservableCollectionThreadSafe<string>();
            ScanCurveGroup = new ScanCurveGroup();

            Properties = new ObservableCollection<Property>();
            this.Service = Service;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Get the sub path of the preset profile where it saved
        /// </summary>
        [Browsable(false)]
        public virtual string SubPath { get => throw new NotImplementedException(); }

        /// <summary>
        /// Get what properties are allowed to edit
        /// </summary>
        [Browsable(false)]
        public ObservableCollection<Property> Properties { private set; get; }

        /// <summary>
        /// The instance of System Service class
        /// This property is the binding source of the Logical Motion Components and instruments
        /// </summary>
        [Browsable(false)]
        public SystemService Service { private set; get; }

        /// <summary>
        /// Get the title of the Axis X for both 2D&3D chart
        /// </summary>
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

        /// <summary>
        /// Get the title of the Axis Y for both 2D&3D chart
        /// </summary>
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

        /// <summary>
        /// Get the title of the Secondary Axis Y for both 2D&3D chart
        /// </summary>
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

        /// <summary>
        /// Get the title of the Axis X for both 3D chart
        /// </summary>
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

        /// <summary>
        /// Get the group of scanned curves, NOTE it's only used to contain the 2D scanned curves,
        /// for 3D curve, the ScanCurve is bound directly to UI because of issuses of Series3D binding.
        /// </summary>
        [Browsable(false)]
        public ScanCurveGroup ScanCurveGroup { private set; get; }

        /// <summary>
        /// Get the messages of the alignment process
        /// </summary>
        [Browsable(false)]
        public ObservableCollectionThreadSafe<string> Log { get; }

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
        [JsonIgnore]
        public IInstrument Instrument
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

        #endregion Properties

        #region Methods

        public virtual void Validate() => throw new NotImplementedException();

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

        protected void WriteCVS(string FileName, string Content)
        {
            var path = Path.GetFullPath(FileName);

            if (Directory.Exists(path))
            {
                File.WriteAllText(FileName, Content);
            }
            else
            {
                throw new DirectoryNotFoundException($"{path} does not exist.");
            }
        }

        /// <summary>
        /// Export raw data to cvs file
        /// </summary>
        /// <returns></returns>
        public virtual string ExportToCVS(string FileName)
        {
            throw new NotImplementedException();
        }

        #endregion Methods
    }
}