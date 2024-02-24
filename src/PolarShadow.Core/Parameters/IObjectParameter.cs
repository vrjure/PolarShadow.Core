using System;
using System.Collections.Generic;
using System.Text;

namespace PolarShadow.Core
{
    public interface IObjectParameter : IParameter
    {
        void Add(ParameterValue value);
    }
}
