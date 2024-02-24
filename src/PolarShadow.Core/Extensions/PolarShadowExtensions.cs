using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace PolarShadow.Core
{
    public static class PolarShadowExtensions
    {
        public static void SaveTo(this IPolarShadow polarShadow, IPolarShadowSource source)
        {
            using var ms = new MemoryStream();
            using var jsonWriter = new Utf8JsonWriter(ms, JsonOption.DefaultWriteOption);
            polarShadow.WriteTo(jsonWriter);
            jsonWriter.Flush();
            ms.Seek(0, SeekOrigin.Begin);
            source.Save(ms);
        }

        public static async Task SaveToAsync(this IPolarShadow polarShadow, IPolarShadowSource source)
        {
            using var ms = new MemoryStream();
            using var jsonWriter = new Utf8JsonWriter(ms, JsonOption.DefaultWriteOption);
            polarShadow.WriteTo(jsonWriter);
            jsonWriter.Flush();
            ms.Seek(0, SeekOrigin.Begin);
            await source.SaveAsync(ms);
        }

        public static IPolarShadow LoadJsonFileSource(this IPolarShadow polarShadow, string path, bool reload = false)
        {
            polarShadow.Load(new JsonFileSource { Path = path}, reload);
            return polarShadow;
        }

        public static IPolarShadow LoadJsonStreamSource(this IPolarShadow polarShadow, Stream stream, bool reload = false)
        {
            polarShadow.Load(new JsonStreamSource(stream), reload);
            return polarShadow;
        }

        public static IEnumerable<T> GetItems<T>(this IPolarShadow polarShadow) where T : IPolarShadowItem
        {
            return polarShadow.Items.Where(f => f is T).Cast<T>();
        }

        public static T GetItem<T>(this IPolarShadow polarShadow, string itemName) where T : IPolarShadowItem
        {
            return polarShadow.Items.Where(f => f.Name == itemName).Cast<T>().FirstOrDefault();
        }
    }
}
