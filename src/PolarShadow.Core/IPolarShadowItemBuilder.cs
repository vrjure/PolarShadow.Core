using System;
using System.Collections.Generic;
using System.Text;

namespace PolarShadow.Core
{
    public interface IPolarShadowItemBuilder
    {
        IPolarShadowItem Build(string name, IPolarShadowBuilder builder);
    }
}
