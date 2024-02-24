using System;
using System.Collections.Generic;
using System.Text;

namespace PolarShadow.Core
{
    public interface IKeyValueParameter : IParameter, IDictionary<string, ParameterValue>
    {

    }
}
