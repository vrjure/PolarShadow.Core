using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PolarShadow.Core
{
    public abstract class RequestHandlerBase : ContentWriter, IRequestHandler
    {
        public async Task<IObjectParameter> ExecuteAsync(IRequest request, IParameter parameter, CancellationToken cancellation = default)
        {
            if (request == null || request.Response == null)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(request.Request?.Url))
            {
                return await OnRequestAsync(request, parameter, cancellation);
            }

            return null;
        }

        protected abstract Task<IObjectParameter> OnRequestAsync(IRequest request, IParameter parameter, CancellationToken cancellation);


    }
}
