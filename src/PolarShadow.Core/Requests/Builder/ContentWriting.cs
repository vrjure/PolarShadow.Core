using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace PolarShadow.Core
{
    public abstract class ContentWriting : IContentWriting
    {

        public virtual void BeforeWriteStartObject(Utf8JsonWriter writer, string propertyName, IParameter parameter)
        {
            
        }

        public virtual void AfterWriteStartObject(Utf8JsonWriter writer, string propertyName, IParameter parameter)
        {

        }


        public virtual void BeforeWriteEndObject(Utf8JsonWriter writer, string propertyName, IParameter parameter)
        {

        }

        public virtual void AfterWriteEndObject(Utf8JsonWriter writer, string propertyName, IParameter parameter)
        {

        }


        public virtual void BeforeWriteStartArray(Utf8JsonWriter writer, string propertyName, IParameter parameter)
        {

        }

        public virtual void AfterWriteStartArray(Utf8JsonWriter writer, string propertyName, IParameter parameter)
        {

        }


        public virtual void BeforeWriteEndArray(Utf8JsonWriter writer, string propertyName, IParameter parameter)
        {

        }

        public virtual void AfterWriteEndArray(Utf8JsonWriter writer, string propertyName, IParameter parameter)
        {
            
        }


        public virtual void AfterWriteProperty(Utf8JsonWriter writer, JsonProperty property, IParameter parameter)
        {
            
        }

        public virtual bool BeforeWriteProperty(Utf8JsonWriter writer, JsonProperty property, IParameter parameter)
        {
            return false;
        }
    }
}
