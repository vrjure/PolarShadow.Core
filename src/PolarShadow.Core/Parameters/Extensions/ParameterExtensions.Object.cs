using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Xml.XPath;

namespace PolarShadow.Core
{
    public static partial class ParameterExtensions
    {
        public static void Add(this IObjectParameter parameter, XPathDocument doc) => Add(parameter, new HtmlElement(doc));
        public static void Add(this IObjectParameter parameter, XPathNavigator nav) => Add(parameter, new HtmlElement(nav));
        public static void Add(this IObjectParameter parameter, JsonElement value) => parameter.Add(new ParameterValue(value));

        public static void Add(this IObjectParameter parameter, HtmlElement value) => parameter.Add(new ParameterValue(value));

        public static void Add<T>(this IObjectParameter parameter, T value) where T : class
        {
            using var doc = JsonDocument.Parse(JsonSerializer.Serialize((object)value, JsonOption.DefaultSerializer));
            parameter.Add(new ParameterValue(doc.RootElement.Clone()));
        }
    }
}
