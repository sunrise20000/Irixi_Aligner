using Irixi_Aligner_Common.Classes.BaseClass;
using Irixi_Aligner_Common.Configuration;
using Irixi_Aligner_Common.Interfaces;
using Irixi_Aligner_Common.Message;
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
        /// Get the instance of zaber conversation object
        /// </summary>
        public Conversation ZaberConversation { private set; get; }

        #endregion

        #region Methods

        /// <summary>
        /// Set the zaber conversation
        /// </summary>
        /// <param name="cs"></param>
        public void RegisterZaberConversation(Conversation cs)
        {
            this.ZaberConversation = cs;
            GetCurrentState();
        }

        /// <summary>
        /// read the current state of the stage
        /// </summary>
        private void GetCurrentState()
        {
            // read home state
            DeviceModes mode =  (DeviceModes)ZaberConversation.Request(Command.ReturnSetting, (int)Command.SetDeviceMode).Data;
            this.IsHomed = ((mode & DeviceModes.HomeStatus) == DeviceModes.HomeStatus);

            // read current position
            if(int.TryParse(ZaberConversation.Request(Command.ReturnSetting, (int)Command.SetCurrentPosition).Data.ToString(), out int pos))
            {
                this.AbsPosition = pos;
            }
            else
            {
                this.AbsPosition = -1;
            }

            // read max position
            if (int.TryParse(ZaberConversation.Request(Command.ReturnSetting, (int)Command.SetMaximumPosition).Data.ToString(), out int max))
            {
                this.CWL = max;
                this.UnitHelper.Dps = this.MaxStroke / max;

                LogHelper.WriteLine("{0} CWL is set to {1}", this, max, LogHelper.LogType.NORMAL);
            }
            else
            {
                // keep the value set in the config file
            }
        }

        #endregion
    }
}
