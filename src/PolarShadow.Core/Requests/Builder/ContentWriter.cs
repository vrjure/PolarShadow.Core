using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace PolarShadow.Core
{
    public class ContentWriter : IContentWriter
    {
        protected virtual void BeforeWriteStartObject(Utf8JsonWriter writer, string propertyName, IParameter parameter) { }
        protected virtual void AfterWriteStartObject(Utf8JsonWriter writer, string propertyName, IParameter parameter) { }

        protected virtual void BeforeWriteEndObject(Utf8JsonWriter writer, string propertyName, IParameter parameter) { }
        protected virtual void AfterWriteEndObject(Utf8JsonWriter writer, string propertyName, IParameter parameter) { }

        protected virtual void BeforeWriteStartArray(Utf8JsonWriter writer, string property, IParameter parameter) { }
        protected virtual void AfterWriteStartArray(Utf8JsonWriter writer, string propertyName, IParameter parameter) { }

        protected virtual void BeforeWriteEndArray(Utf8JsonWriter writer, string propertyName, IParameter parameter) { }
        protected virtual void AfterWriteEndArray(Utf8JsonWriter writer, string propertyName, IParameter parameter) { }

        protected virtual bool BeforeWriteProperty(Utf8JsonWriter writer, JsonProperty property, IParameter parameter) => false;
        protected virtual void AfterWriteProperty(Utf8JsonWriter writer, JsonProperty property, IParameter parameter) { }

        public virtual void Build(Utf8JsonWriter writer, JsonElement template, IParameter parameter)
        {
            if (template.ValueKind == JsonValueKind.String)
            {
                Write(writer, template.GetString(), parameter);
            }
            else
            {
                Write(writer, template, parameter);
            }
        }

        private void Write(Utf8JsonWriter writer, string templateName, IParameter parameter)
        {
            if(parameter.TryReadValue(templateName, out JsonElement template))
            {
                Write(writer, template, parameter);
            }
        }

        private void Write(Utf8JsonWriter writer, JsonElement template, IParameter parameter)
        {
            switch (template.ValueKind)
            {
                case JsonValueKind.Object:
                    BuildObject(writer, template, parameter);
                    break;
                case JsonValueKind.Array:
                    BuildArray(writer, template, parameter);
                    break;
                case JsonValueKind.String:
                case JsonValueKind.Number:
                case JsonValueKind.True:
                case JsonValueKind.False:
                    BuildValue(writer, template, parameter);
                    break;
                default:
                    break;
            }
        }

        private void BuildObject(Utf8JsonWriter writer, JsonElement template, IParameter parameter, string propertyName = default)
        {
            BeforeWriteStartObject(writer, propertyName, parameter);

            if (string.IsNullOrEmpty(propertyName))
            {
                writer.WriteStartObject();
            }
            else
            {
                writer.WriteStartObject(propertyName);
            }

            AfterWriteStartObject(writer, propertyName, parameter);

            foreach (var property in template.EnumerateObject())
            {
                switch (property.Value.ValueKind)
                {
                    case JsonValueKind.Object:
                        BuildObject(writer, property.Value, parameter, property.Name);
                        break;
                    case JsonValueKind.Array:
                        BuildArray(writer, property.Value, parameter, property.Name);
                        break;
                    case JsonValueKind.String:
                    case JsonValueKind.Number:
                    case JsonValueKind.True:
                    case JsonValueKind.False:
                        BuildValue(writer, property, parameter);
                        break;
                    default:
                        break;
                }
            }

            BeforeWriteEndObject(writer, propertyName, parameter);

            writer.WriteEndObject();

            AfterWriteEndObject(writer, propertyName, parameter);
        }

        private void BuildArray(Utf8JsonWriter writer, JsonElement template, IParameter parameter, string propertyName = default)
        {
            if (template.ValueKind != JsonValueKind.Array)
            {
                return;
            }

            BeforeWriteStartArray(writer, propertyName, parameter);

            if (string.IsNullOrEmpty(propertyName))
            {
                writer.WriteStartArray();
            }
            else
            {
                writer.WriteStartArray(propertyName);
            }

            AfterWriteStartArray(writer, propertyName, parameter);

            BuildInArrayTemplate(writer, template, parameter);

            BeforeWriteEndArray(writer, propertyName, parameter);

            writer.WriteEndArray();

            AfterWriteEndArray(writer, propertyName, parameter);
        }

        private void BuildInArrayTemplate(Utf8JsonWriter writer, JsonElement template, IParameter parameter)
        {
            if (template.ValueKind != JsonValueKind.Array)
            {
                return;
            }

            foreach (var obj in template.EnumerateArray())
            {
                if (obj.ValueKind == JsonValueKind.Object)
                {
                    if (IsArrayTemplateObject(obj, out JsonElement path, out JsonElement pathTemplate))
                    {
                        if (pathTemplate.ValueKind == JsonValueKind.Object)
                        {
                            BuildArrayTemplate(writer, path, pathTemplate, parameter);
                        }
                        else if (pathTemplate.ValueKind == JsonValueKind.Array)
                        {
                            BuildInArrayTemplate(writer, pathTemplate, parameter);
                        }
                        else if (pathTemplate.ValueKind == JsonValueKind.String)//find template in parameter
                        {
                            var templateName = pathTemplate.GetString();
                            if (parameter.TryReadValue(templateName, out JsonElement namedTemplate))
                            {
                                BuildArrayTemplate(writer, path, namedTemplate, parameter);
                            }
                        }
                    }
                    else
                    {
                        Write(writer, obj, parameter);
                    }
                }
                else if (obj.ValueKind != JsonValueKind.Array)
                {
                    BuildValue(writer, obj, parameter);
                }
            }
        }
        
        private bool IsArrayTemplateObject(JsonElement obj, out JsonElement path, out JsonElement template)
        {
            path = default;
            template = default;
            return obj.ValueKind == JsonValueKind.Object
                && obj.TryGetProperty("path", out path)
                && obj.TryGetProperty("template", out template);
        }

        private void BuildValue(Utf8JsonWriter writer, JsonProperty property, IParameter parameter)
        {
            if (BeforeWriteProperty(writer, property, parameter))
            {
                AfterWriteProperty(writer, property, parameter);
                return;
            }

            writer.WritePropertyName(property.Name);
            BuildValue(writer, property.Value, parameter);

            AfterWriteProperty(writer, property, parameter);
        }

        private void BuildValue(Utf8JsonWriter writer, JsonElement value, IParameter parameter)
        {
            switch (value.ValueKind)
            {
                case JsonValueKind.String:
                    var str = value.GetString();
                    str = str.Format(parameter);
                    if (bool.TryParse(str, out bool boolean))
                    {
                        writer.WriteBooleanValue(boolean);
                    }
                    else
                    {
                        writer.WriteStringValue(str);
                    }
                    break;
                case JsonValueKind.Number:
                case JsonValueKind.True:
                case JsonValueKind.False:
                    value.WriteTo(writer);
                    break;
                default:
                    break;
            }
        }

        private void BuildArrayTemplate(Utf8JsonWriter jsonWriter, JsonElement path, JsonElement template, IParameter parameter)
        {
            if (path.ValueKind == JsonValueKind.String)
            {
                BuildArrayTemplateStringPath(jsonWriter, path, template, parameter);
            }
            else if (path.ValueKind == JsonValueKind.Array)
            {
                foreach (var obj in path.EnumerateArray())
                {
                    BuildArrayTemplateStringPath(jsonWriter, obj, template, parameter);
                }
            }
        }

        private void BuildArrayTemplateStringPath(Utf8JsonWriter jsonWriter, JsonElement path, JsonElement template, IParameter parameter)
        {
            if (path.ValueKind != JsonValueKind.String)
            {
                return;
            }

            if (!parameter.TryGetValue(path.GetString(), out ParameterValue pathValue))
            {
                return;
            }

            IParameterCollection childContent = null;
            if (parameter is IParameterCollection parameters)
            {
                childContent = new Parameters(parameters);
            }
            else
            {
                childContent = new Parameters(parameter);
            }

            var lastIndex = childContent.Count - 1;
            if (childContent[lastIndex] is IObjectParameter)
            {
                childContent.RemoveAt(lastIndex);
            }

            if (pathValue.ValueKind == ParameterValueKind.Json)
            {
                var jsonPathValue = pathValue.GetJson();

                foreach (var child in jsonPathValue.EnumerateArray())
                {
                    childContent.Add(new ObjectParameter(new ParameterValue(child)));
                    Write(jsonWriter, template, childContent);
                    childContent.RemoveAt(lastIndex);
                }
            }
            else if (pathValue.ValueKind == ParameterValueKind.Html)
            {
                var htmlPathValue = pathValue.GetHtml();
                if (htmlPathValue.ValueKind == HtmlValueKind.Nodes)
                {
                    foreach (var child in htmlPathValue.EnumerateNodes())
                    {
                        childContent.Add(new ObjectParameter(new ParameterValue(child)));
                        Write(jsonWriter, template, childContent);
                        childContent.RemoveAt(lastIndex);
                    }
                }
                else if (htmlPathValue.ValueKind == HtmlValueKind.Node)
                {
                    childContent.Add(new ObjectParameter(new ParameterValue(htmlPathValue)));
                    Write(jsonWriter, template, childContent);
                    childContent.RemoveAt(lastIndex);
                }

            }
        }
    }
}
