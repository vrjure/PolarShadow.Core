using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace PolarShadow.Core
{
    public class KeyValueParameter : IKeyValueParameter
    {
        private readonly Dictionary<string, ParameterValue> _parameters;

        public KeyValueParameter()
        {
            _parameters = new Dictionary<string, ParameterValue>();
        }

        public ParameterValue this[string key]
        {
            get => _parameters[key];
            set => _parameters[key] = value;
        }

        public ICollection<string> Keys => _parameters.Keys;

        public ICollection<ParameterValue> Values => _parameters.Values;

        public int Count => _parameters.Count;

        public bool IsReadOnly => false;

        public void Add(string key, ParameterValue value)
        {
            _parameters[key] = value;
        }

        public void Add(KeyValuePair<string, ParameterValue> item)
        {
            _parameters[item.Key] = item.Value;
        }

        public void Clear()
        {
            _parameters.Clear();
        }

        public bool Contains(KeyValuePair<string, ParameterValue> item)
        {
            return _parameters.ContainsKey(item.Key);
        }

        public bool ContainsKey(string key)
        {
            return _parameters.ContainsKey(key);
        }

        void ICollection<KeyValuePair<string, ParameterValue>>.CopyTo(KeyValuePair<string, ParameterValue>[] array, int arrayIndex)
        {
            foreach (var item in _parameters)
            {
                array[arrayIndex] = item;
                arrayIndex++;
            }
        }

        public IEnumerator<KeyValuePair<string, ParameterValue>> GetEnumerator()
        {
            return _parameters.GetEnumerator();
        }

        public bool Remove(string key)
        {
            return _parameters.Remove(key);
        }

        bool ICollection<KeyValuePair<string, ParameterValue>>.Remove(KeyValuePair<string, ParameterValue> item)
        {
            return _parameters.Remove(item.Key);
        }

        public bool TryGetValue(string key, out ParameterValue value)
        {
            return _parameters.TryGetValue(key, out value);
        }

        public void WriteTo(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();
            foreach (var item in _parameters)
            {
                switch (item.Value.ValueKind)
                {
                    case ParameterValueKind.Number:
                        writer.WriteNumber(item.Key, item.Value.GetDecimal());
                        break;
                    case ParameterValueKind.String:
                        writer.WriteString(item.Key, item.Value.GetString());
                        break;
                    case ParameterValueKind.Json:
                        writer.WritePropertyName(item.Key);
                        item.Value.GetJson().WriteTo(writer);
                        break;
                    case ParameterValueKind.Boolean:
                        writer.WriteBoolean(item.Key, item.Value.GetBoolean());
                        break;
                    default:
                        break;
                }
            }
            writer.WriteEndObject();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
