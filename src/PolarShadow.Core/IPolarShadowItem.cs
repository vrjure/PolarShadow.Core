using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace PolarShadow.Core
{
    public interface IPolarShadowItem : IWriterJson
    {
        string Name { get; }
        void Load(IPolarShadowProvider provider, bool reload = false);
    }
}
