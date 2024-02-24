using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PolarShadow.Core
{
    public class JsonFileSource : FileSource
    {
        
        public override IPolarShadowProvider Build(IPolarShadowBuilder builder)
        {
            return new JsonFileProvider(this);
        }
    }
}
