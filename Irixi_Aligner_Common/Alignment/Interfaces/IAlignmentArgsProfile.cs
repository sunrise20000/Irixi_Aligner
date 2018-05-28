using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irixi_Aligner_Common.Alignment.BaseClasses;
using Irixi_Aligner_Common.Interfaces;

namespace Irixi_Aligner_Common.Alignment.Interfaces
{
    public interface IAlignmentArgsProfile
    {

        /// <summary>
        /// Set the hash string which had been saved in the profile.
        /// This is used to validate the profile after it is loaded from the saved file.
        /// </summary>
        string HashString { set; get; }

        void FromArgsInstance(AlignmentArgsBase arg);

        void ToArgsInstance(AlignmentArgsBase arg);

        string GetHashString();

        bool Validate();
    }
}
