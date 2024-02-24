using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace PolarShadow.Core
{
    internal class RequestTemplate : IRequestTemplate
    {
        public string Url { get; set; }
        public string Method { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public JsonElement? Body { get; set; }
    }
}
