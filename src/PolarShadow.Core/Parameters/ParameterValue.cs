using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Xml.XPath;

namespace PolarShadow.Core
{
    public struct ParameterValue
    {
        private decimal? _numberValue;
        private JsonElement? _jsonValue;
        private string _stringValue;
        private HtmlElement? _htmlValue;
        private bool? _booleanValue;

        private ParameterValueKind _valueKind;

        public ParameterValue(decimal value)
        {
            _numberValue = value;
            _valueKind = ParameterValueKind.Number;

            _jsonValue = default;
            _stringValue = default;
            _htmlValue = default;
            _booleanValue = default;
        }

        public ParameterValue(string value)
        {
            _stringValue= value;
            _valueKind = ParameterValueKind.String;
           
            _jsonValue = default;
            _numberValue = default;
            _htmlValue = default;
            _booleanValue = default;
        }

        public ParameterValue(bool value)
        {
            _booleanValue = value;
            _valueKind = ParameterValueKind.Boolean;

            _jsonValue = default;
            _stringValue = default;
            _htmlValue = default;
            _numberValue = default;
        }

        public ParameterValue(JsonElement value)
        {
            _jsonValue = value;
            _valueKind = ParameterValueKind.Json;

            _numberValue = default;
            _stringValue = default;
            _htmlValue = default;
            _booleanValue = default;
        }

        public ParameterValue(HtmlElement value)
        {
            _htmlValue = value;
            _valueKind = ParameterValueKind.Html;
           
            _stringValue = default;
            _numberValue = default;
            _jsonValue = default;
            _booleanValue = default;
        }

        public ParameterValueKind ValueKind => _valueKind;

        public static bool IsJsonPath(string path)
        {
            return path.StartsWith("$");
        }

        public static bool IsXPath(string path)
        {
            return path.StartsWith("/");
        }

        public string GetValue()
        {
            switch (_valueKind)
            {
                case ParameterValueKind.Number:
                    return _numberValue.ToString();
                case ParameterValueKind.String:
                    return _stringValue;
                case ParameterValueKind.Json:
                    return GetJsonValue();
                case ParameterValueKind.Html:
                    return GetHtmlValue();
                default:
                    break;
            }

            return null;
        }

        public decimal GetDecimal()
        {
            if (_valueKind == ParameterValueKind.Number)
                return _numberValue.Value;

            throw new InvalidOperationException();
        }

        public int GetInt32()
        {
            return (int)GetDecimal();
        }

        public short GetInt16()
        {
            return (short)GetDecimal();
        }

        public long GetInt64()
        {
            return (long)GetDecimal();
        }

        public float GetFloat()
        {
            return (float)GetDecimal();
        }

        public double GetDouble()
        {
            return (double)GetDecimal();
        }

        public string GetString()
        {
            if (_valueKind == ParameterValueKind.String)
                return _stringValue;

            throw new InvalidOperationException();
        }

        public bool GetBoolean()
        {
            if (_valueKind == ParameterValueKind.Boolean)
                return _booleanValue.Value;
            throw new InvalidOperationException();
        }

        public JsonElement GetJson()
        {
            if (_valueKind == ParameterValueKind.Json) return _jsonValue.Value;

            throw new InvalidOperationException();
        }

        public HtmlElement GetHtml()
        {
            if (_valueKind == ParameterValueKind.Html) return _htmlValue.Value;

            throw new InvalidOperationException();
        }

        private string GetJsonValue()
        {
            switch (_jsonValue.Value.ValueKind)
            {
                case JsonValueKind.String:
                    return _jsonValue.Value.GetString();
                case JsonValueKind.Number:
                    return _jsonValue.Value.GetDecimal().ToString();
                case JsonValueKind.True:
                case JsonValueKind.False:
                    return _jsonValue.Value.GetBoolean().ToString();
                default:
                    return _jsonValue.Value.GetRawText();
            }
        }

        private string GetHtmlValue()
        {
            return _htmlValue.Value.GetValue();
        }

        public override bool Equals(object obj)
        {
            var other = (ParameterValue)obj;
            if (this.ValueKind != other._valueKind)
            {
                return false;
            }
            return this.GetValue() == other.GetValue();
        }

        public override int GetHashCode()
        {
            return $"{_valueKind}{GetValue()}".GetHashCode();
        }

    }
}
