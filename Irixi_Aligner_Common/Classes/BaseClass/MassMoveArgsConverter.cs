using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Irixi_Aligner_Common.Classes.BaseClass
{
    /// <summary>
    /// The customized converter to serialize and deserialize the MassMoveArgs
    /// <see cref="https://www.jerriepelser.com/blog/custom-converters-in-json-net-case-study-1/"/>
    /// </summary>
    public class MassMoveArgsConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(MassMoveArgs).GetTypeInfo().IsAssignableFrom(objectType.GetTypeInfo());
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var inst = value as MassMoveArgs;
            IEnumerable<AxisMoveArgs> array = (IEnumerable<AxisMoveArgs>)value;

            writer.WriteStartObject();
            writer.WritePropertyName("LogicalMotionComponent");
            writer.WriteValue(inst.LogicalMotionComponent);
            writer.WritePropertyName("HashString");
            writer.WriteValue(inst.HashString);
            writer.WritePropertyName("Args");
            writer.WriteStartArray();
            foreach (var item in array)
            {
                serializer.Serialize(writer, item);
            }
            writer.WriteEndArray();
            writer.WriteEndObject();

        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartObject)
            {
                JObject item = JObject.Load(reader);
                if (item["Args"] != null)
                {
                    var argsList = item["Args"].ToObject<IEnumerable<AxisMoveArgs>>(serializer);
                    var obj = new MassMoveArgs(argsList)
                    {
                        LogicalMotionComponent = item["LogicalMotionComponent"].Value<string>(),
                        HashString = item["HashString"].Value<string>()
                    };

                    return obj;
                }
                else
                {
                    throw new JsonReaderException("the move args array can not be null.");
                }
            }
            else
            {
                throw new JsonReaderException("the token type is not StartObject.");
            }
        }
    }
}
