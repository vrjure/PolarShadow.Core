using PolarShadow.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PolarShadow.Core
{
    public class PolarShadowBuilder : IPolarShadowBuilder
    {
        private readonly Dictionary<Type, HashSet<string>> _itemTypeNames;
        private readonly Dictionary<Type, IPolarShadowItemBuilder> _itemBuilders;
        public PolarShadowBuilder()
        {
            _itemTypeNames = new Dictionary<Type, HashSet<string>>();
            _itemBuilders = new Dictionary<Type, IPolarShadowItemBuilder>();
        }

        public IPolarShadowBuilder AddItemBuilder<T>(IPolarShadowItemBuilder itemBuilder) where T : IPolarShadowItemBuilder
        {
            var type = typeof(T);
            if (!_itemBuilders.ContainsKey(type))
            {
                _itemBuilders.Add(type, itemBuilder);
            }

            return this;
        }

        public IPolarShadowBuilder AddItemName<T>(string name) where T : IPolarShadowItemBuilder
        {
            var type = typeof(T);
            if (!_itemTypeNames.TryGetValue(type, out HashSet<string> names))
            {
                names = new HashSet<string>();
                _itemTypeNames.Add(type, names);
            }

            if (!names.Contains(name))
            {
                names.Add(name);
            }

            return this;
        }

        public IPolarShadow Build()
        {
            var items = new List<IPolarShadowItem>();
            foreach (var item in _itemTypeNames)
            {
                var itemBuilder = _itemBuilders[item.Key];
                foreach (var name in item.Value)
                {
                    items.Add(itemBuilder.Build(name, this));
                }
            }
            return new PolarShadowDefault(this, items);
        }

        public bool TryGetItemBuilder<T>(out T itemBuilder) where T : IPolarShadowItemBuilder
        {
            if( _itemBuilders.TryGetValue(typeof(T), out IPolarShadowItemBuilder b))
            {
                itemBuilder = (T)b;
                return true;
            }
            itemBuilder = default;
            return false;
        }
    }
}
