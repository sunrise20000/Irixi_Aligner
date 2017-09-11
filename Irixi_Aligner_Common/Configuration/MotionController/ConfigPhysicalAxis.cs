using static Irixi_Aligner_Common.Classes.BaseClass.RealworldPositionManager;

namespace Irixi_Aligner_Common.Configuration
{
    public class ConfigPhysicalAxis
    {
        #region Constructor

        #endregion

        public string Comment { get; set; }

        /// <summary>
        /// Get the name of the axis
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Indicates wether the axis is enabled or not
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Get the initial position after homing
        /// </summary>
        public int OffsetAfterHome { get; set; }

        /// <summary>
        /// Get the max drive speed
        /// </summary>
        public int MaxSpeed { get; set; }

        /// <summary>
        /// Get how many steps to accelerate to the max speed
        /// </summary>
        public int AccelerationSteps { get; set; }

        /// <summary>
        /// Get the subdivsion set of the motor driver hardware
        /// </summary>
        public int SubDivision { get; set; }

        /// <summary>
        /// Get the decimal digits of the real world distance displayed on the window
        /// </summary>
        public int ScaleDisplayed { get; set; }

        /// <summary>
        /// Get the vendor of the motorized actuator
        /// </summary>
        public string Vendor { get; set; }

        /// <summary>
        /// Get the model of the motorized actuator
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// Get the unit of the real world distance
        /// </summary>
        public UnitType Unit { get; set; }

        /// <summary>
        /// Get whether the home point should be reversed
        /// </summary>
        public bool ReverseHomePoint { get; set; }


        public MotorizedStageProfile MotorizedStageProfile { private set; get; }

        /// <summary>
        /// Some of parameters should be re-calculated while the profile is set,
        /// because the Unit/Subdivision settings effect the profile.
        /// </summary>
        /// <param name="Profile"></param>
        public void SetProfile(MotorizedStageProfile Profile)
        {
            /*
             * NOTE: Make sure that the execution order is:
             *   1. Change Unit
             *   2. Recalculate parameters
             */

            // note that the desired unit might be different from the definition in the motorized stage profile,
            // it because that the profile is the common definition, but for each particular usage, we may want
            // the different definition.
            Profile.ChangeUnit(this.Unit);

            // re-set the resolution occording the subdivision
            Profile.Resolution /= this.SubDivision;

            this.MotorizedStageProfile = Profile;
        }

        public override string ToString()
        {
            return Comment;
        }
    }
}
