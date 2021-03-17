/**
 *
 * MIT License
 *
 * Copyright (c) 2018 Jagan Mohan Maddukuri
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 *
 * */

namespace ReverseProxy
{
    using Microsoft.AspNetCore.Http;
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Net.Security;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    using System.Collections.Concurrent;

    public class ReverseProxy
    {
        private readonly HttpClient InternalHttpClient = new HttpClient();

        private const string HeaderTransferEncoding = "transfer-encoding";

        private RequestDelegate nextMiddleWare;

        private ConcurrentDictionary<string, TargetService> primarySegmentDictionary = new ConcurrentDictionary<string, TargetService>();

        public ReverseProxy(RequestDelegate nextMiddleWare)
        {
            this.nextMiddleWare = nextMiddleWare;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string path = context.Request.Path;
            string primarySegment = path.Split('/')[1];

            Console.WriteLine("This is:" + primarySegment);

            TargetService s = new TargetService("weatherforecast");
            s.TargetUrl = "https://localhost:5001";

            primarySegmentDictionary["weatherforecast"] = s;

            TargetService service;
            if (!this.primarySegmentDictionary.TryGetValue(primarySegment, out service))
            {
                await nextMiddleWare(context);

                return;
            }

            Uri targetUri = new Uri(service.TargetUrl + context.Request.Path);
            HttpRequestMessage newRequest = this.GenerateNewRequest(context, targetUri);

            using (HttpResponseMessage proxiedResponse =
                await InternalHttpClient.SendAsync(newRequest, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted))
            {
                var response = context.Response;

                response.StatusCode = (int)proxiedResponse.StatusCode;
                foreach (var header in proxiedResponse.Headers)
                {
                    if (!header.Key.Equals(ReverseProxy.HeaderTransferEncoding, StringComparison.OrdinalIgnoreCase))
                    {
                        response.Headers[header.Key] = header.Value.ToArray();
                    }
                }

                foreach (var header in proxiedResponse.Content.Headers)
                {
                    if (!header.Key.Equals(ReverseProxy.HeaderTransferEncoding, StringComparison.OrdinalIgnoreCase))
                    {
                        response.Headers[header.Key] = header.Value.ToArray();
                    }
                }

                using (var responseStream = await proxiedResponse.Content.ReadAsStreamAsync())
                {
                    await responseStream.CopyToAsync(response.Body, 81920, context.RequestAborted);
                }

                Console.WriteLine("Response is:" + response.Body);
            }
        }

        public HttpRequestMessage GenerateNewRequest(HttpContext context, Uri targetUri)
        {
            HttpRequestMessage newRequest = new HttpRequestMessage();
            string method = context.Request.Method;

            if (!HttpMethods.IsDelete(method) &&
                !HttpMethods.IsGet(method) &&
                !HttpMethods.IsHead(method) &&
                !HttpMethods.IsTrace(method))
            {
                StreamContent content = new StreamContent(context.Request.Body);
                newRequest.Content = content;
            }

            foreach (var header in context.Request.Headers)
            {
                if (!newRequest.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()) && newRequest.Content != null)
                {
                    newRequest.Content.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                }
            }

            newRequest.Headers.Host = targetUri.Host;
            newRequest.RequestUri = targetUri;
            newRequest.Method = new HttpMethod(method);

            return newRequest;
        }
    }
}
