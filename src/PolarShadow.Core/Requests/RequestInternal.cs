using System;
using System.Collections.Generic;
using System.Text;

namespace PolarShadow.Core
{
    internal class RequestInternal : IRequest
    {
        public IRequestTemplate Request { get; set; }

        public IResponseTemplate Response { get; set; }
    }
}
