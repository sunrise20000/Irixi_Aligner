using Irixi_Aligner_Common.Configuration;
using Irixi_Aligner_Common.Interfaces;
using System.Threading.Tasks;
using Zaber;

namespace Irixi_Aligner_Common.MotionControllerEntities
{
    public class LuminosAxis : AxisBase
    {
        #region Constructors

        #endregion

        #region Properties
        /// <summary>
        /// Get the instance of axis from luminos sdk
        /// </summary>
        ////public PositionerLib.Axis LuminosAxisInstance { private set; get; }

        public Conversation ZaberConversation { private set; get; }

        #endregion

        #region Methods

        /// <summary>
        /// Set instance of luminos axis of stage
        /// </summary>
        /// <param name="AxisInstance"></param>
        ////public void RegisterLuminosSDKAxis(PositionerLib.Axis AxisInstance)
        ////{
        ////    this.LuminosAxisInstance = AxisInstance;
        ////    this.LuminosAxisInstance.OnChanged += LuminosAxisInstance_OnChanged;
        ////    this.LuminosAxisInstance.OnPositionUpdate += LuminosAxisInstance_OnPositionUpdate;
        ////}

        /// <summary>
        /// Set the zaber conversation
        /// </summary>
        /// <param name="cs"></param>
        public void RegisterZaberConversation(Conversation cs)
        {
            this.ZaberConversation = cs;
        }
        #endregion

        #region Events raised by luminos sdk
        /// <summary>
        /// the position changing has reported by sdk, update the corresponding property
        /// </summary>
        /// <param name="Position"></param>
        private void LuminosAxisInstance_OnPositionUpdate(int Position)
        {
            this.AbsPosition = Position;
            this.IsBusy = false;
        }

        /// <summary>
        /// The state of the axis has changed, update the corresponding properties
        /// </summary>
        /// <param name="PropID"></param>
        ////private void LuminosAxisInstance_OnChanged(PositionerLib.AxisPropertyID PropID)
        ////{
        ////    switch (PropID)
        ////    {
        ////        // Set the manual control of the actuator
        ////        case PositionerLib.AxisPropertyID.AxisManualEnabledID:
        ////            this.IsManualEnabled = this.LuminosAxisInstance.ManualEnabled;
        ////            break;

        ////        case PositionerLib.AxisPropertyID.AxisRequiresHomingID:
        ////            this.IsHomed = !this.LuminosAxisInstance.RequiresHoming;
        ////            break;

        ////        case PositionerLib.AxisPropertyID.AxisLastPositionID:
        ////            this.AbsPosition = this.LuminosAxisInstance.LastPosition;
        ////            break;

        ////    }
        ////}

        #endregion
    }
}
