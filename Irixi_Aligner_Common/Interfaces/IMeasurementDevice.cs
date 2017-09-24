using System.Threading.Tasks;

namespace Irixi_Aligner_Common.Interfaces
{
    interface IMeasurementDevice : IEquipmentBase
    {
        double Fetch();

        Task<double> FetchAsync();
    }
}
