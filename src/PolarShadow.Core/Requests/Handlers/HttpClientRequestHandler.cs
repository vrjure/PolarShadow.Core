using HtmlAgilityPack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PolarShadow.Core
{
    public class HttpClientRequestHandler : RequestHandlerBase
    {
        private static readonly string _applicationjson = "application/json";
        private static readonly string _textHtml = "text/html";
        private static readonly string _userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Safari/537.36 Edg/107.0.1418.62";

        protected override async Task<IObjectParameter> OnRequestAsync(IRequest request, IParameter parameter, CancellationToken cancellation)
        {
            var url = request.Request.Url.Format(parameter).Format(parameter);
            using var client = new HttpClient();

            if (!client.DefaultRequestHeaders.Contains("User-Agent"))
            {
                client.DefaultRequestHeaders.Add("User-Agent", _userAgent);
            }

            var method = request.Request.Method;
            if (string.IsNullOrEmpty(method))
            {
                method = HttpMethod.Get.Method;
            }

            using var requestMsg = new HttpRequestMessage(new HttpMethod(method), url);

            if (request.Request.Headers != null && request.Request.Headers.Count > 0)
            {
                foreach (var item in request.Request.Headers)
                {
                    requestMsg.Headers.Add(item.Key, item.Value);
                }
            }
            using var ms = new MemoryStream();
            if (request.Request.Body.HasValue)
            {
                this.Build(ms, request.Request.Body.Value, parameter);
                ms.Seek(0, SeekOrigin.Begin);
                requestMsg.Content = new StreamContent(ms);
            }

            using var responseMsg = await client.SendAsync(requestMsg, cancellation);
            responseMsg?.EnsureSuccessStatusCode();

            var contentType = responseMsg.Content.Headers.ContentType;
            var content = await responseMsg.Content.ReadAsStreamAsync();

            if (contentType.MediaType.Equals(_applicationjson, StringComparison.OrdinalIgnoreCase))
            {
                using var doc = JsonDocument.Parse(content);
                return new ObjectParameter(new ParameterValue(doc.RootElement.Clone()));
            }
            else if (contentType.MediaType.Equals(_textHtml, StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrEmpty(request.Response.Encoding))
                {
                    var doc = new HtmlDocument();
                    doc.Load(content);
                    return new ObjectParameter(new ParameterValue(new HtmlElement(doc.CreateNavigator())));
                }
                else
                {
                    var doc = new HtmlDocument();
                    doc.Load(content, Encoding.GetEncoding(request.Response.Encoding));
                    return new ObjectParameter(new ParameterValue(new HtmlElement(doc.CreateNavigator())));
                }
            }
            else
            {
                throw new InvalidOperationException($"Not supported content-type:{contentType}");
            }
        }
    }
}
