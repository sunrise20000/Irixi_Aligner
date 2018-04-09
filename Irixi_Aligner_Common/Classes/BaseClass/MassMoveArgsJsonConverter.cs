using System;
using System.Linq;
using Newtonsoft.Json;

namespace Irixi_Aligner_Common.Classes.BaseClass
{
    /// <summary>
    /// Customized converter to convert the object inherited from collection to json string
    /// <see cref="https://stackoverflow.com/questions/14383736/how-to-serialize-deserialize-a-custom-collection-with-additional-properties-usin"/>
    /// </summary>
    public class MassMoveArgsJsonConverter : JsonConverter
    {
        //public override bool CanConvert(Type objectType)
        //{
        //    return objectType == typeof(MassMoveArgs);
        //}

        //public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        //{
        //    // N.B. null handling is missing
        //    var args = serializer.Deserialize<MassMoveArgs>(reader);
        //    var fooElements = args;
        //    var fooColl = new FooCollection { Bar = surrogate.Bar };
        //    foreach (var el in fooElements)
        //        fooColl.Add(el);
        //    return fooColl;
        //}

        //public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        //{
        //    // N.B. null handling is missing
        //    var args = (MassMoveArgs)value;
        //    // create the surrogate and serialize it instead 
        //    // of the collection itself
        //    var surrogate = new MassMoveArgs()
        //    {
        //        Collection = fooColl.ToList(),
        //        Bar = fooColl.Bar
        //    };
        //    serializer.Serialize(writer, surrogate);
        //}
    }
}
