using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace PolarShadow.Core
{
    public interface IWriterJson
    {
        void WriteTo(Utf8JsonWriter writer);
        void LoadFrom(IPolarShadowSource source);
    }
}
