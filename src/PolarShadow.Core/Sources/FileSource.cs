using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace PolarShadow.Core
{
    public abstract class FileSource : JsonSource
    {
        public string Path { get; set; }

        public override void Save(Stream content)
        {
            if (string.IsNullOrEmpty(Path)) return;
            if (!File.Exists(Path)) return;
            using var fs = new FileStream(Path, FileMode.Truncate, FileAccess.Write, FileShare.Read);
            content.CopyTo(fs);
            fs.Flush();
        }

        public override async Task SaveAsync(Stream content)
        {
            if (string.IsNullOrEmpty(Path)) return;
            if (!File.Exists(Path)) return;
            using var fs = new FileStream(Path, FileMode.Truncate, FileAccess.Write, FileShare.Read);
            await content.CopyToAsync(fs);
            await fs.FlushAsync();
        }

        public override string ToString()
        {
            return Path ?? "null";
        }
    }
}
