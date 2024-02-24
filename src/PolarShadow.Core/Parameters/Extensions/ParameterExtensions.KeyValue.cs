using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace PolarShadow.Core
{
    public static partial class ParameterExtensions
    {
        public static void Add(this IKeyValueParameter parameter, string name, decimal value) => parameter.Add(name, new ParameterValue(value));
        public static void Add(this IKeyValueParameter parameter, string name, string value) => parameter.Add(name, new ParameterValue(value));
        public static void Add(this IKeyValueParameter parameter, string name, bool value) => parameter.Add(name, new ParameterValue(value));
        public static void Add(this IKeyValueParameter parameter, string name, JsonElement value) => parameter.Add(name, new ParameterValue(value));
        public static void Add(this IKeyValueParameter parameter, string name, HtmlElement value) => parameter.Add(name, new ParameterValue(value));
        public static void Add(this IKeyValueParameter parameter, JsonElement value)
        {
            if (value.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidOperationException("JsonValueKind must be a object");
            }

            foreach (var item in value.EnumerateObject())
            {
                switch (item.Value.ValueKind)
                {
                    case JsonValueKind.Object:
                    case JsonValueKind.Array:
                        parameter.Add(item.Name, item.Value);
                        break;
                    case JsonValueKind.String:
                        parameter.Add(item.Name, item.Value.GetString());
                        break;
                    case JsonValueKind.Number:
                        parameter.Add(item.Name, item.Value.GetDecimal());
                        break;
                    case JsonValueKind.True:
                    case JsonValueKind.False:
                        parameter.Add(item.Name, item.Value.GetBoolean());
                        break;
                    default:
                        break;
                }
            }
        }
        public static void Add<T>(this IKeyValueParameter parameter, T value) where T :class
        {
            using var doc = JsonDocument.Parse(JsonSerializer.Serialize(value, JsonOption.DefaultSerializer));
            parameter.Add(doc.RootElement.Clone());
        }
    }
}
