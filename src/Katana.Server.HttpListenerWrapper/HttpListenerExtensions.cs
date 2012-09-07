using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.HttpListener.Owin
{
    public static class HttpListenerExtensions
    {
        public static Task<HttpListenerContext> GetContextAsync(this System.Net.HttpListener listener)
        {
            return Task.Factory.FromAsync<HttpListenerContext>(listener.BeginGetContext, listener.EndGetContext, null);
        }

        public static Task<X509Certificate2> GetClientCertificateAsync(this HttpListenerRequest request)
        {
            return Task.Factory.FromAsync<X509Certificate2>(request.BeginGetClientCertificate, request.EndGetClientCertificate, null);
        }
    }
}
