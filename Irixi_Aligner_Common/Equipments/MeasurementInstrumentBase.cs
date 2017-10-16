using Irixi_Aligner_Common.Configuration;
using Irixi_Aligner_Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Irixi_Aligner_Common.Equipments
{
    public class MeasurementInstrumentBase : EquipmentBase, IMeasurementInstrument
    {
        public MeasurementInstrumentBase(ConfigurationBase Config):base(Config)
        {

        }

        public virtual double Fetch()
        {
            throw new NotImplementedException();
        }

        public virtual Task<double> FetchAsync()
        {
            throw new NotImplementedException();
        }

        public virtual void PauseAutoFetching()
        {
            throw new NotImplementedException();
        }

        public virtual void ResumeAutoFetching()
        {
            throw new NotImplementedException();
        }

        public virtual void StartAutoFetching()
        {
            throw new NotImplementedException();
        }

        public virtual void StopAutoFetching()
        {
            throw new NotImplementedException();
        }
    }
}
