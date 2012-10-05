// Copyright 2011-2012 Katana contributors
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

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.WebApi.Owin;

namespace Owin
{
    public static class OwinHttpMessageExtensions
    {
        private static readonly Func<OwinHttpMessageStep, Func<IDictionary<string, object>, Task>> _conversion1 =
            next => next.Invoke;

        private static readonly Func<Func<IDictionary<string, object>, Task>, OwinHttpMessageStep> _conversion2 =
            next => new OwinHttpMessageStep.CallAppFunc(next);

        public static IAppBuilder UseHttpServer(this IAppBuilder builder, HttpConfiguration configuration)
        {
            return Add(builder, new HttpMessageInvoker(new HttpServer(configuration)));
        }

        public static IAppBuilder UseHttpServer(this IAppBuilder builder, HttpConfiguration configuration, HttpMessageHandler dispatcher)
        {
            return Add(builder, new HttpMessageInvoker(new HttpServer(configuration, dispatcher)));
        }

        public static IAppBuilder UseHttpServer(this IAppBuilder builder, HttpServer server)
        {
            return Add(builder, new HttpMessageInvoker(server));
        }

        private static IAppBuilder Add(IAppBuilder builder, HttpMessageInvoker invoker)
        {
            return builder
                .AddSignatureConversion(_conversion1)
                .AddSignatureConversion(_conversion2)
                .Use(Middleware(invoker));
        }

        private static Func<OwinHttpMessageStep, OwinHttpMessageStep> Middleware(HttpMessageInvoker invoker)
        {
            return next => new OwinHttpMessageStep.CallHttpMessageInvoker(next, invoker);
        }
    }
}
