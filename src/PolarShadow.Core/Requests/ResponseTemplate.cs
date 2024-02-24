using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace PolarShadow.Core
{
    internal class ResponseTemplate : IResponseTemplate
    {
        public string Encoding { get; set; }
        public JsonElement? Template { get; set; }
    }
}
