using System.Threading.Tasks;

namespace Irixi_Aligner_Common.Interfaces
{
    public interface IInstrument : IEquipmentBase
    {
        #region Properties

        bool IsMultiChannel { get; }

        int ActiveChannel { get; }

        #endregion

        #region Methods

        string GetDescription();

        void Reset();

        double Fetch();

        double Fetch(int Channel);

        Task<double> FetchAsync();

        Task<double> FetchAsync(int Channel);

        void StartAutoFetching();

        void StopAutoFetching();

        void PauseAutoFetching();

        void ResumeAutoFetching();

        #endregion
    }
}
