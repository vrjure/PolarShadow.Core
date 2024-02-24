using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace PolarShadow.Core
{
    public abstract class JsonSource : IPolarShadowSource
    {
        public abstract IPolarShadowProvider Build(IPolarShadowBuilder builder);

        public abstract void Save(Stream content);

        public abstract Task SaveAsync(Stream content);
    }
}
