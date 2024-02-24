using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PolarShadow.Core
{
    public class JsonStringProvider : JsonProvider
    {
        private JsonStringSource _source;
        public JsonStringProvider(JsonStringSource source) : base(source)
        {
            if (_source == null) throw new ArgumentNullException(nameof(source));
            _source = source;
        }

        protected override JsonElement Parse()
        {
            if (string.IsNullOrEmpty(_source.Json))
            {
                return default;
            }

            using var doc = JsonDocument.Parse(_source.Json);
            return doc.RootElement.Clone();
        }

        protected override Task<JsonElement> ParseAsync()
        {
            return Task.FromResult(Parse());
        }
    }
}
