using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PolarShadow.Core
{
    public interface IPolarShadowProvider
    {
        JsonElement Root { get; }
        void Load();
        Task LoadAsync();
    }
}
