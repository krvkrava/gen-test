using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Smooth.Algebraics;

namespace Helpers
{
    public class JsonOptionNullConverter<T> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
            => objectType.IsGenericType
               && objectType.GetGenericTypeDefinition() == typeof(Option<>)
               && objectType.GetGenericArguments().Single() == typeof(T);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            try
            {
                var option = (Option<T>) value;
                if (option.isSome)
                    JToken.FromObject(option.value).WriteTo(writer);
                else writer.WriteNull();
            }
            catch (Exception e)
            {
                writer.WriteNull();
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var isReadable = reader.TokenType != JsonToken.Null;
            return isReadable
                ? serializer.Deserialize<T>(reader).ToSome()
                : Option<T>.None;
        }
    }
}