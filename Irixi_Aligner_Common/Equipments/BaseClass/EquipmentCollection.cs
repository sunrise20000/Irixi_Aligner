using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irixi_Aligner_Common.Interfaces;

namespace Irixi_Aligner_Common.Equipments.BaseClass
{
    public class EquipmentCollection<T>: List<T>
        where T: IHashable
    {
        public T FindItemByHashString(string hash)
        {
            T obj;
            var ret = this.Where(item => item.HashString == hash);

            if (ret.Any())
                obj = ret.First();
            else
            {
                throw new Exception($"unable to find the element with hash string {hash}");
            }

            return obj;
        }
    }
}
