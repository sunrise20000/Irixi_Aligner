using Irixi_Aligner_Common.Configuration.MotionController;
using Irixi_Aligner_Common.Interfaces;
using Irixi_Aligner_Common.MotionControllers.Base;

namespace Irixi_Aligner_Common.MotionControllers.Irixi
{
    public class IrixiAxis : AxisBase
    {

        #region Constructors
       
        #endregion


        #region Methods

        public override void SetParameters(int AxisIndex, ConfigPhysicalAxis Config, IMotionController Controller)
        {
            base.SetParameters(AxisIndex, Config, Controller);

            if (Config.ReverseDriveDirection.HasValue)
            {
                this.ReverseDriveDirecton = Config.ReverseDriveDirection.Value;
            }
            else
            {
                this.ReverseDriveDirecton = false;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Get whether the drive direction is reversed (CCWL is set to the zero point)
        /// </summary>
        public bool ReverseDriveDirecton { private set; get; }

        #endregion
    }
}
