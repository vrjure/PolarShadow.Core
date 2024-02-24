using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Linq;
using System.Text.Json;

namespace PolarShadow.Core
{
    public class Parameters : IParameterCollection
    {
        private IList<IParameter> _parameters;

        public Parameters() => Initialize(null);

        public Parameters(params IParameter[] parameters) => Initialize(parameters);

        public Parameters(IParameterCollection parameters) => Initialize(parameters);

        private void Initialize(IEnumerable<IParameter> parameters)
        {
            _parameters = new List<IParameter>();

            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    Add(parameter);
                }
            }
        }

        public IParameter this[int index]
        {
            get => _parameters[index];
            set => _parameters[index] = value;
        }

        public int Count => _parameters.Count;

        public bool IsReadOnly => false;

        public void Add(IParameter parameter)
        {
            if (parameter == null)
            {
                return;
            }
            _parameters.Add(parameter);
        }

        public void Clear()
        {
            _parameters.Clear();
        }

        public bool Contains(IParameter item)
        {
            return _parameters.Contains(item);
        }

        public void CopyTo(IParameter[] array, int arrayIndex)
        {
            _parameters.CopyTo(array, arrayIndex);
        }

        public IEnumerator<IParameter> GetEnumerator()
        {
             return _parameters.GetEnumerator();
        }

        public int IndexOf(IParameter item)
        {
            return _parameters.IndexOf(item);
        }

        public void Insert(int index, IParameter item)
        {
            _parameters.Insert(index, item);
        }

        public bool Remove(IParameter item)
        {
            return _parameters.Remove(item);
        }

        public void RemoveAt(int index)
        {
            _parameters.RemoveAt(index);
        }

        public bool TryGetValue(string key, out ParameterValue value)
        {
            value = default;
            var len = _parameters.Count;
            for (int i = len - 1; i >= 0; i--)
            {
                var parameter = _parameters[i];
                if (parameter.TryGetValue(key, out value))
                {
                    return true;
                }
            }

            return false;
        }

        public void WriteTo(Utf8JsonWriter writer)
        {
            writer.WriteStartArray();
            foreach (var item in _parameters)
            {
                item.WriteTo(writer);
            }
            writer.WriteEndArray();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
