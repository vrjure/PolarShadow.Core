using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PolarShadow.Core
{
    public abstract class JsonProvider : IPolarShadowProvider
    {
        private JsonElement _root;
        private JsonSource _source;
        public JsonProvider(JsonSource source)
        {
            _source = source;
        }

        public JsonElement Root => _root;

        public void Load()
        {
            _root = Parse();
        }

        public async Task LoadAsync()
        {
            _root = await ParseAsync();
        }

        protected abstract JsonElement Parse();
        protected abstract Task<JsonElement> ParseAsync();
    }
}
