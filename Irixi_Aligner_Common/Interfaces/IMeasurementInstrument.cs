using System.Threading.Tasks;

namespace Irixi_Aligner_Common.Interfaces
{
    public interface IMeasurementInstrument : IEquipmentBase
    {
        double Fetch();

        Task<double> FetchAsync();

        void StartAutoFetching();

        void StopAutoFetching();


    }
}
