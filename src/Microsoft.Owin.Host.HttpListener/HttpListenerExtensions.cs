// <copyright file="HttpListenerExtensions.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2011-2013 Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

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
#endif
