using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PolarShadow.Core
{
    public interface IPolarShadowSource
    {
        void Save(Stream content);
        Task SaveAsync(Stream content);
        IPolarShadowProvider Build(IPolarShadowBuilder builder);
    }
}
