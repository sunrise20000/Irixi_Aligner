using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Text;

namespace Irixi_Aligner_Common.Classes.BaseClass
{
    [Serializable]
    public class MassMoveArgs : ObservableCollection<AxisMoveArgs>
    {
        #region Variables

        #endregion

        #region Constructors

        public MassMoveArgs()
        {

        }

        public MassMoveArgs(IEnumerable<AxisMoveArgs> Collection):base(Collection)
        {

        }
        
        #endregion

        #region Properties
        public string LogicalMotionComponent { get; set; }
        
        public string HashString { get; set; }

        #endregion

        #region Methods

        public string GetHashString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(LogicalMotionComponent);

            foreach (var axisArgs in this)
            {
                sb.Append(axisArgs.GetHashString());
            }

            return HashGenerator.GetHashSHA256(sb.ToString());
        }

        public override int GetHashCode()
        {
            return GetHashString().GetHashCode();
        }
        
        #endregion

        #region Commands



        #endregion

    }
}
