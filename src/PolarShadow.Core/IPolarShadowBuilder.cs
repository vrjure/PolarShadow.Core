using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolarShadow.Core
{
    public interface IPolarShadowBuilder
    {
        IPolarShadowBuilder AddItemBuilder<T>(IPolarShadowItemBuilder itemBuilderCreator) where T : IPolarShadowItemBuilder;
        IPolarShadowBuilder AddItemName<T>(string name) where T : IPolarShadowItemBuilder;
        bool TryGetItemBuilder<T>(out T itemBuilder) where T : IPolarShadowItemBuilder;
        IPolarShadow Build();
    }
}
