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

        static IAppBuilder Add(IAppBuilder builder, HttpMessageInvoker invoker)
        {
            return builder
                .AddSignatureConversion(Conversion1)
                .AddSignatureConversion(Conversion2)
                .Use(Middleware(invoker));
        }

        static Func<OwinHttpMessageStep, OwinHttpMessageStep> Middleware(HttpMessageInvoker invoker)
        {
            return next => new OwinHttpMessageStep.CallHttpMessageInvoker(next, invoker);
        }

        static readonly Func<OwinHttpMessageStep, Func<IDictionary<string, object>, Task>> Conversion1 =
            next => next.Invoke;

        static readonly Func<Func<IDictionary<string, object>, Task>, OwinHttpMessageStep> Conversion2 =
            next => new OwinHttpMessageStep.CallAppFunc(next);
    }
}