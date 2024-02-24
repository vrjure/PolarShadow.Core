using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace PolarShadow.Core
{

    internal readonly struct JsonPathValue
    {
        private readonly decimal _numberValue;
        private readonly string _stringValue;
        private readonly JsonElement _jsonValue;
        private readonly IEnumerable<string> _stringValues;
        private readonly IEnumerable<decimal> _numberValues;

        private readonly JsonPathValueKind _valueKind;

        public JsonPathValueKind ValueKind => _valueKind;

        internal JsonPathValue(decimal number)
        {
            _numberValue = number;
            _valueKind = JsonPathValueKind.Number;

            _stringValue = string.Empty;
            _jsonValue = default;
            _stringValues = null;
            _numberValues = null;
        }

        internal JsonPathValue(string str)
        {
            _stringValue = str;
            _valueKind = JsonPathValueKind.String;

            _numberValue = default;
            _jsonValue = default;
            _stringValues = null;
            _numberValues = null;
        }

        internal JsonPathValue(JsonElement json)
        {
            _jsonValue = json;
            _valueKind = JsonPathValueKind.Json;

            _numberValue = default;
            _stringValue = null;
            _numberValues = null;
            _stringValues = null;
        }

        internal JsonPathValue(IEnumerable<string> stringValues)
        {
            _stringValues = stringValues;
            _valueKind = JsonPathValueKind.Strings;

            _numberValue = default;
            _stringValue = null;
            _jsonValue= default;
            _numberValues = null;
        }

        internal JsonPathValue(IEnumerable<decimal> numberValues)
        {
            _numberValues = numberValues;
            _valueKind = JsonPathValueKind.Numbers;

            _numberValue = default;
            _stringValue = null;
            _stringValues = null;
            _jsonValue = default;
        }

        public bool In(JsonPathValue other)
        {
            if (_valueKind == JsonPathValueKind.String
                && other._valueKind == JsonPathValueKind.Strings)
            {
                var str = _stringValue;
                return other._stringValues.Any(f => f == str);
            }
            else if (_valueKind == JsonPathValueKind.Number
                && other._valueKind == JsonPathValueKind.Numbers)
            {
                var number = _numberValue;
                return other._numberValues.Any(f => f == number);
            }
            else if (_valueKind == JsonPathValueKind.Json)
            {
                if (_jsonValue.ValueKind == JsonValueKind.String
                    && other._valueKind == JsonPathValueKind.Strings)
                {
                    var str = _jsonValue.GetString();
                    return other._stringValues.Any(f=> f == str);

                }
                else if (_jsonValue.ValueKind == JsonValueKind.Number 
                    && other._valueKind == JsonPathValueKind.Numbers)
                {
                    var number = _jsonValue.GetDecimal();
                    return other._numberValues.Any(f => f == number);
                }
            }

            return false;
        }

        public bool SubsetOf(JsonPathValue other)
        {
            if (_valueKind != JsonPathValueKind.Json 
                || _jsonValue.ValueKind != JsonValueKind.Array
                || other._valueKind != JsonPathValueKind.Numbers && other._valueKind != JsonPathValueKind.Strings)
            {
                return false;
            }

            var first = _jsonValue.EnumerateArray().FirstOrDefault();
            if (first.ValueKind == JsonValueKind.String
                && other._valueKind == JsonPathValueKind.Strings)
            {
                var set = new HashSet<string>();
                foreach (var item in _jsonValue.EnumerateArray())
                {
                    set.Add(item.GetString());
                }

                return set.IsSubsetOf(other._stringValues);
            }
            else if (first.ValueKind == JsonValueKind.Number
                && other._valueKind == JsonPathValueKind.Numbers)
            {
                var set = new HashSet<decimal>();
                foreach (var item in _jsonValue.EnumerateArray())
                {
                    set.Add(item.GetDecimal());
                }
                return set.IsSubsetOf(_numberValues);
            }
            return false;
        }

        public bool AnyOf(JsonPathValue other)
        {
            if (_valueKind != JsonPathValueKind.Json
                || _jsonValue.ValueKind != JsonValueKind.Array
                || other._valueKind != JsonPathValueKind.Numbers && other._valueKind != JsonPathValueKind.Strings)
            {
                return false;
            }

            var first = _jsonValue.EnumerateArray().FirstOrDefault();
            if (first.ValueKind == JsonValueKind.String)
            {
                var set = new HashSet<string>();
                foreach (var item in _jsonValue.EnumerateArray())
                {
                    set.Add(item.GetString());
                }
                return other._stringValues.Any(f => set.Contains(f));
            }
            else if (first.ValueKind == JsonValueKind.Number)
            {
                var set = new HashSet<decimal>();
                foreach (var item in _jsonValue.EnumerateArray())
                {
                    set.Add(item.GetDecimal());
                }
                return other._numberValues.Any(f => set.Contains(f));
            }

            return false;
        }

        public bool Size(JsonPathValue other)
        {
            var len = GetLength(this);
            return len != -1 && len == GetLength(other);
        }

        public bool Empty()
        {
            if (_valueKind != JsonPathValueKind.Json 
                || _jsonValue.ValueKind != JsonValueKind.Array
                 && _jsonValue.ValueKind != JsonValueKind.String
                 && _jsonValue.ValueKind != JsonValueKind.Undefined)
            {
                return false;
            }

            return _jsonValue.ValueKind switch
            {
                JsonValueKind.String => string.IsNullOrEmpty(_jsonValue.GetString()),
                JsonValueKind.Array => _jsonValue.GetArrayLength() == 0,
                JsonValueKind.Undefined => true,
                _ => false
            };
        }

        public bool Matches(JsonPathValue regex)
        {
            if (_valueKind == JsonPathValueKind.Json 
                && _jsonValue.ValueKind == JsonValueKind.String 
                && regex._valueKind == JsonPathValueKind.String)
            {
                if (regex._stringValue.EndsWith("/i"))
                {
                    return Regex.IsMatch(_jsonValue.GetString(), regex._stringValue[1..^2], RegexOptions.IgnoreCase);
                }
                else if (regex._stringValue.EndsWith("/m"))
                {
                    return Regex.IsMatch(_jsonValue.GetString(), regex._stringValue[1..^2], RegexOptions.Multiline);
                }
                else if (regex._stringValue.EndsWith("/s"))
                {
                    return Regex.IsMatch(_jsonValue.GetString(), regex._stringValue[1..^2], RegexOptions.Singleline);
                }
                else if (regex._stringValue.EndsWith("/g"))
                {
                    return Regex.IsMatch(_jsonValue.GetString(), regex._stringValue[1..^2]);
                }
                
                return Regex.IsMatch(_jsonValue.GetString(), regex._stringValue[1..^1]);
            }

            return false;
        }

        public bool IsTrue()
        {
            return _valueKind switch
            {
                JsonPathValueKind.Number => _numberValue != 0,
                JsonPathValueKind.String => !string.IsNullOrEmpty(_stringValue),
                JsonPathValueKind.Strings => _stringValues != null && _stringValues.Count() > 0,
                JsonPathValueKind.Numbers => _numberValues != null && _numberValues.Count() > 0,
                JsonPathValueKind.Json => _jsonValue.ValueKind switch
                {
                    JsonValueKind.String => string.IsNullOrEmpty(_jsonValue.GetString()),
                    JsonValueKind.Number => _jsonValue.GetDecimal() != 0,
                    JsonValueKind.False => false,
                    JsonValueKind.True => true,
                    _ => false
                },
                _ => false
            };
        }

        private int GetLength(JsonPathValue value)
        {
            return value._valueKind switch
            {
                JsonPathValueKind.String => _stringValue.Length,
                JsonPathValueKind.Strings => _stringValues.Count(),
                JsonPathValueKind.Numbers => _numberValues.Count(),
                JsonPathValueKind.Json => _jsonValue.ValueKind == JsonValueKind.Array ? _jsonValue.GetArrayLength() : -1,
                _ => -1
            };
        }

        public static bool operator ==(JsonPathValue left, JsonPathValue right)
        {
            if (left._valueKind == JsonPathValueKind.Json 
                && right._valueKind == JsonPathValueKind.Json
                && left._jsonValue.ValueKind == right._jsonValue.ValueKind)
            {
                return left._jsonValue.ValueKind switch
                {
                    JsonValueKind.String => left._jsonValue.ValueEquals(right._jsonValue.GetString()),
                    JsonValueKind.Number => left._jsonValue.GetDecimal() == right._jsonValue.GetDecimal(),
                    JsonValueKind.False => left._jsonValue.GetBoolean() == right._jsonValue.GetBoolean(),
                    JsonValueKind.True => left._jsonValue.GetBoolean() == right._jsonValue.GetBoolean(),
                    _ => false
                };
            }
            else if (left._valueKind == JsonPathValueKind.Json
                && left._jsonValue.ValueKind == JsonValueKind.String
                && right._valueKind == JsonPathValueKind.String)
            {
                return left._jsonValue.ValueEquals(right._stringValue);
            }
            else if (left._valueKind == JsonPathValueKind.Json
                && left._jsonValue.ValueKind == JsonValueKind.Number
                && right._valueKind == JsonPathValueKind.Number)
            {
                return left._jsonValue.GetDecimal() == right._numberValue;
            }

            return false;
        }

        public static bool operator !=(JsonPathValue left, JsonPathValue right)
        {
            return !(left == right);
        }

        public static bool operator >(JsonPathValue left, JsonPathValue right)
        {
            if (left._valueKind == JsonPathValueKind.Json
                && left._jsonValue.ValueKind == JsonValueKind.Number)
            {
                if (right._valueKind == JsonPathValueKind.Json && right._jsonValue.ValueKind == JsonValueKind.Number)
                {
                    return left._jsonValue.GetDecimal() > right._jsonValue.GetDecimal();
                }
                else if (right._valueKind == JsonPathValueKind.Number)
                {
                    return left._jsonValue.GetDecimal() > right._numberValue;
                }
            }

            return false;
        }

        public static bool operator <(JsonPathValue left, JsonPathValue right)
        {
            if (left._valueKind == JsonPathValueKind.Json
                && left._jsonValue.ValueKind == JsonValueKind.Number)
            {
                if (right._valueKind == JsonPathValueKind.Json && right._jsonValue.ValueKind == JsonValueKind.Number)
                {
                    return left._jsonValue.GetDecimal() < right._jsonValue.GetDecimal();
                }
                else if (right._valueKind == JsonPathValueKind.Number)
                {
                    return left._jsonValue.GetDecimal() < right._numberValue;
                }
            }

            return false;
        }

        public static bool operator >=(JsonPathValue left, JsonPathValue right)
        {
            if (left._valueKind == JsonPathValueKind.Json
                && left._jsonValue.ValueKind == JsonValueKind.Number)
            {
                if (right._valueKind == JsonPathValueKind.Json && right._jsonValue.ValueKind == JsonValueKind.Number)
                {
                    return left._jsonValue.GetDecimal() >= right._jsonValue.GetDecimal();
                }
                else if (right._valueKind == JsonPathValueKind.Number)
                {
                    return left._jsonValue.GetDecimal() >= right._numberValue;
                }
            }

            return false;
        }

        public static bool operator <=(JsonPathValue left, JsonPathValue right)
        {
            if (left._valueKind == JsonPathValueKind.Json
                && left._jsonValue.ValueKind == JsonValueKind.Number)
            {
                if (right._valueKind == JsonPathValueKind.Json && right._jsonValue.ValueKind == JsonValueKind.Number)
                {
                    return left._jsonValue.GetDecimal() <= right._jsonValue.GetDecimal();
                }
                else if (right._valueKind == JsonPathValueKind.Number)
                {
                    return left._jsonValue.GetDecimal() <= right._numberValue;
                }
            }

            return false;
        }

        public override bool Equals(object obj)
        {
            return this == (JsonPathValue)obj;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
