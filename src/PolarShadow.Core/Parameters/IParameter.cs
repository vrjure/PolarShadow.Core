using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace PolarShadow.Core
{
    public interface IParameter
    {
        bool TryGetValue(string key, out ParameterValue value);
        void WriteTo(Utf8JsonWriter writer);
    }
}
