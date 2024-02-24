using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PolarShadow.Core
{
    public class JsonFileProvider : JsonProvider
    {
        private FileSource _fileSource;
        public JsonFileProvider(FileSource source) : base(source)
        {
            _fileSource = source;
            if (_fileSource == null) { throw new ArgumentNullException(nameof(source)); }
        }

        protected override JsonElement Parse()
        {
            if (string.IsNullOrEmpty(_fileSource.Path)) return default;
            if (!File.Exists(_fileSource.Path)) return default;
            using var fs = new FileStream(_fileSource.Path, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var doc = JsonDocument.Parse(fs);
            return doc.RootElement.Clone();
        }

        protected override async Task<JsonElement> ParseAsync()
        {
            if (string.IsNullOrEmpty(_fileSource.Path)) return default;
            if (!File.Exists(_fileSource.Path)) return default;
            using var fs = new FileStream(_fileSource.Path, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var doc = await JsonDocument.ParseAsync(fs);
            return doc.RootElement.Clone();
        }
    }
}
