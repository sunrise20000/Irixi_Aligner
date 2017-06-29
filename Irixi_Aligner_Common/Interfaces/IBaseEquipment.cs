using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Irixi_Aligner_Common.Interfaces
{
    public interface IBaseEquipment : IDisposable
    {
        /// <summary>
        /// Initialize the controller
        /// </summary>
        /// <returns></returns>
        Task<bool> Init();


        /// <summary>
        /// Get whehter the controller is available or not
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Get whether the controller has been initialized
        /// </summary>
        bool IsInitialized { get; }


        /// <summary>
        /// Get the last error message
        /// </summary>
        string LastError { get; }
    }
}
