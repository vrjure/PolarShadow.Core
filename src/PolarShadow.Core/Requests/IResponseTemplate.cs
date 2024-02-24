using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace PolarShadow.Core
{
    public interface IResponseTemplate
    {
        string Encoding { get; set; }
        JsonElement? Template { get; set; }
    }
}
