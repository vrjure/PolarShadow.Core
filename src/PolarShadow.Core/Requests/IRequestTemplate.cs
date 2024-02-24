using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace PolarShadow.Core
{
    public interface IRequestTemplate
    {
        string Url { get; set; }
        string Method { get; set; }
        Dictionary<string, string> Headers { get; set; }
        JsonElement? Body { get; set; }
    }
}
