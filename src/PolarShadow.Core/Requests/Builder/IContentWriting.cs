using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace PolarShadow.Core
{
    public interface IContentWriting
    {
        void BeforeWriteStartObject(Utf8JsonWriter writer, string propertyName, IParameter parameter);
        void AfterWriteStartObject(Utf8JsonWriter writer, string propertyName, IParameter parameter);

        void BeforeWriteEndObject(Utf8JsonWriter writer, string propertyName, IParameter parameter);
        void AfterWriteEndObject(Utf8JsonWriter writer, string propertyName, IParameter parameter);

        void BeforeWriteStartArray(Utf8JsonWriter writer, string propertyName, IParameter parameter);
        void AfterWriteStartArray(Utf8JsonWriter writer, string propertyName, IParameter parameter);

        void BeforeWriteEndArray(Utf8JsonWriter writer, string propertyName, IParameter parameter);
        void AfterWriteEndArray(Utf8JsonWriter writer, string propertyName, IParameter parameter);

        bool BeforeWriteProperty(Utf8JsonWriter writer, JsonProperty property, IParameter parameter);
        void AfterWriteProperty(Utf8JsonWriter writer, JsonProperty property, IParameter parameter);
    }
}
