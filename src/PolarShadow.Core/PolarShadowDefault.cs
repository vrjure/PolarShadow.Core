using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PolarShadow.Core
{
    internal class PolarShadowDefault : IPolarShadow
    {
        private readonly IPolarShadowBuilder _builder;
        private readonly ICollection<IPolarShadowItem> _items;

        public PolarShadowDefault(IPolarShadowBuilder builder, IEnumerable<IPolarShadowItem> items)
        {
            _builder = builder;
            _items = new List<IPolarShadowItem>(items);
        }

        public IEnumerable<IPolarShadowItem> Items => _items;

        public void Load(IPolarShadowSource source, bool reLoad = false)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var provider = source.Build(_builder);
            if (provider == null)
            {
                return;
            }

            provider.Load();

            Load(provider, reLoad);
        }

        public async Task LoadAsync(IPolarShadowSource source, bool reLoad = false)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var provider = source.Build(_builder);
            if (provider == null)
            {
                return;
            }

            await provider.LoadAsync().ConfigureAwait(false);

            Load(provider, reLoad);
        }

        private void Load(IPolarShadowProvider provider, bool reLoad = false)
        {
            if (provider == null || provider.Root.ValueKind == JsonValueKind.Undefined) return;

            foreach (var item in _items)
            {
                item.Load(provider, reLoad);
            }
        }

        public void ReadFrom(string json)
        {
            var source = new JsonStringSource { Json = json };
            Load(source, true);
        }

        public void LoadFrom(IPolarShadowSource source)
        {
            Load(source, true);
        }

        public void WriteTo(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();

            foreach (var item in _items)
            {
                writer.WritePropertyName(item.Name);
                item.WriteTo(writer);
            }

            writer.WriteEndObject();
        }
    }
}
