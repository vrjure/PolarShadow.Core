using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PolarShadow.Core
{
    public interface IRequestHandler
    {
        Task<IObjectParameter> ExecuteAsync(IRequest request, IParameter parameter, CancellationToken cancellation = default);
    }
}
