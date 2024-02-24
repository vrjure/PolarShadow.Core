using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PolarShadow.Core
{
    public class JsonStreamProvider : JsonProvider
    {
        private readonly JsonStreamSource _source;

        public JsonStreamProvider(JsonStreamSource source) : base(source)
        {
            _source = source;
        }

        protected override JsonElement Parse()
        {
            if (_source.Stream == null)
            {
                return default;
            }

            using var doc = JsonDocument.Parse(_source.Stream);
            return doc.RootElement.Clone();
        }

        protected override async Task<JsonElement> ParseAsync()
        {
            if (_source.Stream == null)
            {
                return default;
            }

            using var doc = await JsonDocument.ParseAsync(_source.Stream);
            return doc.RootElement.Clone();
        }
    }
}
