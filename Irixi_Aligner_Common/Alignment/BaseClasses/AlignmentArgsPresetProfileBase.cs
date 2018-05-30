using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Irixi_Aligner_Common.Alignment.Interfaces;
using Irixi_Aligner_Common.Classes.BaseClass;

namespace Irixi_Aligner_Common.Alignment.BaseClasses
{
    public class AlignmentArgsPresetProfileBase : IAlignmentArgsProfile
    {
        [HashIgnore]
        public string HashString { set; get; }

        public virtual void FromArgsInstance(AlignmentArgsBase arg) => throw new NotImplementedException();

        public virtual void ToArgsInstance(AlignmentArgsBase arg) => throw new NotImplementedException();

        public virtual bool Validate()
        {
            return this.HashString == this.GetHashString();
        }

        /// <summary>
        /// Calculate hash string by all the properties except the ones marked with HashIgnore
        /// </summary>
        /// <returns></returns>
        public virtual string GetHashString()
        {
            // get the list of my properties those do not marked as [HashIgnore]
            Type mytype = this.GetType();
            List<PropertyInfo> properties = new List<PropertyInfo>(mytype.GetProperties());
            var validprop = properties.Where(item => item.GetCustomAttribute<HashIgnoreAttribute>() == null).Select((pi, str) =>
            {
                return pi.GetValue(this);
            }).ToArray();
            return HashGenerator.GetHashSHA256(String.Join(",", validprop));

        }

        public override string ToString()
        {
            return HashString;
        }
    }
}
