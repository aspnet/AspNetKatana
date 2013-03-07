// <copyright file="OwinHttpMessageExtensions.cs" company="Microsoft Open Technologies, Inc.">
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

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.WebApi.Owin;

namespace Owin
{
    /// <summary>
    /// 
    /// </summary>
    public static class OwinHttpMessageExtensions
    {
        private static readonly Func<OwinHttpMessageStep, Func<IDictionary<string, object>, Task>> Conversion1 =
            next => next.Invoke;

        private static readonly Func<Func<IDictionary<string, object>, Task>, OwinHttpMessageStep> Conversion2 =
            next => new OwinHttpMessageStep.CallAppFunc(next);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Not out of scope")]
        public static IAppBuilder UseWebApi(this IAppBuilder builder, HttpConfiguration configuration)
        {
            return Add(builder, new HttpMessageInvoker(new HttpServer(configuration)));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="initialize"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Not out of scope")]
        public static IAppBuilder UseWebApi(this IAppBuilder builder, Action<HttpConfiguration> initialize)
        {
            if (initialize == null)
            {
                throw new ArgumentNullException("initialize");
            }
            var configuration = new HttpConfiguration();
            initialize(configuration);
            return Add(builder, new HttpMessageInvoker(new HttpServer(configuration)));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configuration"></param>
        /// <param name="dispatcher"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Not out of scope")]
        public static IAppBuilder UseWebApi(this IAppBuilder builder, HttpConfiguration configuration, HttpMessageHandler dispatcher)
        {
            return Add(builder, new HttpMessageInvoker(new HttpServer(configuration, dispatcher)));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="server"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Not out of scope")]
        public static IAppBuilder UseWebApi(this IAppBuilder builder, HttpMessageHandler server)
        {
            return Add(builder, new HttpMessageInvoker(server));
        }

        private static IAppBuilder Add(IAppBuilder builder, HttpMessageInvoker invoker)
        {
            builder.AddSignatureConversion(Conversion1);
            builder.AddSignatureConversion(Conversion2);
            return builder.Use(Middleware(invoker));
        }

        private static Func<OwinHttpMessageStep, OwinHttpMessageStep> Middleware(HttpMessageInvoker invoker)
        {
            return next => new OwinHttpMessageStep.CallHttpMessageInvoker(next, invoker);
        }

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Justification = "False positive")]
        private static void AddSignatureConversion(this IAppBuilder builder, Delegate conversion)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            object value;
            if (builder.Properties.TryGetValue("builder.AddSignatureConversion", out value) &&
                value is Action<Delegate>)
            {
                ((Action<Delegate>)value).Invoke(conversion);
            }
            else
            {
                throw new MissingMethodException(builder.GetType().FullName, "AddSignatureConversion");
            }
        }
    }
}
