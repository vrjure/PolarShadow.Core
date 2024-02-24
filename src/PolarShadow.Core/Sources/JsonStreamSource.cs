using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace PolarShadow.Core
{
    public class JsonStreamSource : JsonSource
    {
        public JsonStreamSource(Stream stream)
        {
            Stream = stream;
        }

        public Stream Stream { get; set; }

        public override IPolarShadowProvider Build(IPolarShadowBuilder builder)
        {
            return new JsonStreamProvider(this);
        }

        public override void Save(Stream content)
        {
            if (Stream == null)
            {
                return;
            }

            Stream.SetLength(0);

            content.CopyTo(Stream);
            Stream.Flush();
        }

        public override async Task SaveAsync(Stream content)
        {
            if (Stream == null)
            {
                return;
            }

            Stream.SetLength(0);

            await content.CopyToAsync(Stream);
            await Stream.FlushAsync();
        }
    }
}
