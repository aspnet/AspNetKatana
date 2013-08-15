// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if NET40

using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Microsoft.Owin.Host.HttpListener
{
    internal static class HttpListenerExtensions
    {
        internal static Task<HttpListenerContext> GetContextAsync(this System.Net.HttpListener listener)
        {
            return Task.Factory.FromAsync<HttpListenerContext>(listener.BeginGetContext, listener.EndGetContext, null);
        }

        internal static Task<X509Certificate2> GetClientCertificateAsync(this HttpListenerRequest request)
        {
            return Task.Factory.FromAsync<X509Certificate2>(request.BeginGetClientCertificate, request.EndGetClientCertificate, null);
        }
    }
}

#else

using FormattingWorkaround = System.Object;

#endif
