using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace PolarShadow.Core
{
    public static partial class ParameterExtensions
    {
        public static void AddObjectValue<T>(this IParameterCollection p, T value) where T : class
        {
            var objectParameter = new ObjectParameter();
            objectParameter.Add(value);

            p.Add(objectParameter);
        }

        public static void AddKeyValue<T>(this IParameterCollection p, T value) where T:class
        {
            var keyValueParameter = new KeyValueParameter();
            keyValueParameter.Add(value);
            p.Add(keyValueParameter);
        }

        public static bool TryReadValue<TValue>(this IParameter parameter, string path, out TValue value)
        {
            if (parameter.TryGetValue(path, out ParameterValue val))
            {
                switch (val.ValueKind)
                {
                    case ParameterValueKind.Number:
                        if (val.GetDecimal() is TValue vd) { value = vd; return true; }
                        else if (val.GetInt16() is TValue v16) { value = v16; return true; }
                        else if (val.GetInt32() is TValue v32) { value = v32; return true; }
                        else if (val.GetInt64() is TValue v64) { value = v64; return true; }
                        else if (val.GetFloat() is TValue vfloat) { value = vfloat; return true; }
                        else if (val.GetDouble() is TValue vdouble) { value = vdouble; return true; }
                        break;
                    case ParameterValueKind.String:
                        if (val.GetString() is TValue v)
                        {
                            value = v;
                            return true;
                        }
                        break;
                    case ParameterValueKind.Boolean:
                        if (val.GetBoolean() is TValue vb)
                        {
                            value = vb;
                            return true;
                        }
                        break;
                    case ParameterValueKind.Json:
                        var jsonValue = val.GetJson();
                        if (jsonValue is TValue vj)
                        {
                            value = vj;
                            return true;
                        }
                        else if(TryGetJsonValue(jsonValue, out value))
                        {
                            return true;
                        }
                        break;
                    case ParameterValueKind.Html:
                        var htmlValue = val.GetHtml();
                        if (val.GetHtml() is TValue vh)
                        {         
                            value = vh;
                            return true;
                        }
                        else
                        {
                            var str = htmlValue.GetValue();
                            if (str is TValue _str)
                            {
                                value = _str;
                                return true;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }

            value = default;
            return false;
        }

        private static bool TryGetJsonValue<T>(JsonElement json, out T value)
        {
            switch (json.ValueKind)
            {
                case JsonValueKind.Object:
                case JsonValueKind.Array:
                    value = JsonSerializer.Deserialize<T>(json, JsonOption.DefaultSerializer);
                    return true;
                case JsonValueKind.String:
                    var str = json.GetString();
                    if (str is T _str)
                    {
                        value = _str;
                        return true;
                    }
                    break;
                case JsonValueKind.Number:
                    var num = json.GetDecimal();
                    if (num is T _num)
                    {
                        value = _num;
                        return true;
                    }
                    break;
                case JsonValueKind.True:
                case JsonValueKind.False:
                    var b = json.GetBoolean();
                    if (b is T _b)
                    {
                        value = _b;
                        return true;
                    }
                    break;
                case JsonValueKind.Null:
                    value = default;
                    return true;
            }

            value = default;
            return false;
        }
    }
}
