using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace PolarShadow.Core
{
    public static class PolarShadowBuilderExtensions
    {
        public static IPolarShadowBuilder ConfigureItem<T>(this IPolarShadowBuilder builder, Action<T> itemBuilderBuilder) where T : IPolarShadowItemBuilder
        {
            if (builder.TryGetItemBuilder(out T itemBuilder))
            {
                itemBuilderBuilder(itemBuilder);
            }

            return builder;
        }
    }
}
