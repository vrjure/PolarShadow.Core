using System;
using System.Collections.Generic;
using System.Text;

namespace PolarShadow.Core
{
    public interface IRequest
    {
        IRequestTemplate Request { get; }
        IResponseTemplate Response { get; }
    }
}
