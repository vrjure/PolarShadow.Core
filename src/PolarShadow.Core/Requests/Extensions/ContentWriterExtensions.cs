using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace PolarShadow.Core
{
    public static class ContentWriterExtensions
    {
        /// <summary>
        /// 构建模板内容
        /// </summary>
        public static void Build(this IContentWriter writer, Stream output, JsonElement tempate, IParameter parameter)
        {
            using var jsonWriter = new Utf8JsonWriter(output, JsonOption.DefaultWriteOption);
            writer.Build(jsonWriter, tempate, parameter);
            jsonWriter.Flush();
        }
    }
}
