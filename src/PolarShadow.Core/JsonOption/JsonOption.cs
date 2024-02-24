using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace PolarShadow.Core
{
    public static class JsonOption
    {
        static JsonOption()
        {
            DefaultSerializer = new JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Converters =
                {
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
                    new KeyValueParameterConverter(),
                    new TypeMappingConverter<IRequest, RequestInternal>(),
                    new TypeMappingConverter<IRequestTemplate, RequestTemplate>(),
                    new TypeMappingConverter<IResponseTemplate, ResponseTemplate>()
                },
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            };

            FormatSerializer = new JsonSerializerOptions(DefaultSerializer)
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.Never,
                WriteIndented = true,
            };

            DefaultWriteOption = new JsonWriterOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            FormatWriteOption = new JsonWriterOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                Indented = true,
            };
        }

        public static JsonSerializerOptions DefaultSerializer { get; }
        
        public static JsonSerializerOptions FormatSerializer { get; }

        public static JsonWriterOptions DefaultWriteOption { get; }

        public static JsonWriterOptions FormatWriteOption { get; }

        public static JsonNodeOptions DefaultNodeOption => new JsonNodeOptions { PropertyNameCaseInsensitive = true };


        public static void Default(JsonSerializerOptions option)
        {
            option.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            option.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            option.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
            option.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        }

    }
}
