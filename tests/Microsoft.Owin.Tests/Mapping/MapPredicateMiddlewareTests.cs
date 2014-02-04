// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin.Builder;
using Owin;
using Xunit;

namespace Microsoft.Owin.Mapping.Tests
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    using Predicate = Func<IOwinContext, bool>;
    using PredicateAsync = Func<IOwinContext, Task<bool>>;

    public class MapPredicateMiddlewareTests
    {
        private static readonly Predicate NotImplementedPredicate = new Predicate(envionment => { throw new NotImplementedException(); });
        private static readonly PredicateAsync NotImplementedPredicateAsync = new PredicateAsync(envionment => { throw new NotImplementedException(); });

        private static Task Success(IOwinContext context)
        {
            context.Response.StatusCode = 200;
            return Task.FromResult<object>(null);
        }

        private static void UseSuccess(IAppBuilder app)
        {
            app.Run(Success);
        }

        private static Task NotImplemented(IOwinContext context)
        {
            throw new NotImplementedException();
        }

        private static void UseNotImplemented(IAppBuilder app)
        {
            app.Run(NotImplemented);
        }

        private bool TruePredicate(IOwinContext context)
        {
            return true;
        }

        private bool FalsePredicate(IOwinContext context)
        {
            return false;
        }

        private Task<bool> TruePredicateAsync(IOwinContext context)
        {
            return Task.FromResult<bool>(true);
        }

        private Task<bool> FalsePredicateAsync(IOwinContext context)
        {
            return Task.FromResult<bool>(false);
        }

        [Fact]
        public void NullArguments_ArgumentNullException()
        {
            var builder = new AppBuilder();
            var noMiddleware = new AppBuilder().Build<AppFunc>();
            var noOptions = new MapWhenOptions();
            Assert.Throws<ArgumentNullException>(() => builder.MapWhen(null, UseNotImplemented));
            Assert.Throws<ArgumentNullException>(() => builder.MapWhen(NotImplementedPredicate, (Action<IAppBuilder>)null));
            Assert.Throws<ArgumentNullException>(() => new MapWhenMiddleware(null, noOptions));
            Assert.Throws<ArgumentNullException>(() => new MapWhenMiddleware(noMiddleware, null));

            Assert.Throws<ArgumentNullException>(() => builder.MapWhenAsync(null, UseNotImplemented));
            Assert.Throws<ArgumentNullException>(() => builder.MapWhenAsync(NotImplementedPredicateAsync, (Action<IAppBuilder>)null));
            Assert.Throws<ArgumentNullException>(() => new MapWhenMiddleware(null, noOptions));
            Assert.Throws<ArgumentNullException>(() => new MapWhenMiddleware(noMiddleware, null));
        }

        [Fact]
        public void PredicateTrue_BranchTaken()
        {
            IOwinContext context = new OwinContext();
            IAppBuilder builder = new AppBuilder();
            builder.MapWhen(TruePredicate, UseSuccess);
            var app = builder.Build<OwinMiddleware>();
            app.Invoke(context).Wait();

            Assert.Equal(200, context.Response.StatusCode);
        }

        [Fact]
        public void PredicateTrueAction_BranchTaken()
        {
            IOwinContext context = new OwinContext();
            IAppBuilder builder = new AppBuilder();
            builder.MapWhen(TruePredicate, UseSuccess);
            var app = builder.Build<OwinMiddleware>();
            app.Invoke(context).Wait();

            Assert.Equal(200, context.Response.StatusCode);
        }

        [Fact]
        public void PredicateFalseAction_PassThrough()
        {
            IOwinContext context = new OwinContext();
            IAppBuilder builder = new AppBuilder();
            builder.MapWhen(FalsePredicate, UseNotImplemented);
            builder.Run(Success);
            var app = builder.Build<OwinMiddleware>();
            app.Invoke(context).Wait();

            Assert.Equal(200, context.Response.StatusCode);
        }

        [Fact]
        public void PredicateAsyncTrueAction_BranchTaken()
        {
            IOwinContext context = new OwinContext();
            IAppBuilder builder = new AppBuilder();
            builder.MapWhenAsync(TruePredicateAsync, UseSuccess);
            var app = builder.Build<OwinMiddleware>();
            app.Invoke(context).Wait();

            Assert.Equal(200, context.Response.StatusCode);
        }

        [Fact]
        public void PredicateAsyncFalseAction_PassThrough()
        {
            IOwinContext context = new OwinContext();
            IAppBuilder builder = new AppBuilder();
            builder.MapWhenAsync(FalsePredicateAsync, UseNotImplemented);
            builder.Run(Success);
            var app = builder.Build<OwinMiddleware>();
            app.Invoke(context).Wait();

            Assert.Equal(200, context.Response.StatusCode);
        }

        [Fact]
        public void ChainedPredicates_Success()
        {
            IAppBuilder builder = new AppBuilder();
            builder.MapWhen(TruePredicate, map1 =>
            {
                map1.MapWhen((Predicate)FalsePredicate, UseNotImplemented);
                map1.MapWhen((Predicate)TruePredicate, map2 => map2.MapWhen((Predicate)TruePredicate, UseSuccess));
                map1.Run(NotImplemented);
            });
            var app = builder.Build<OwinMiddleware>();

            IOwinContext context = new OwinContext();
            app.Invoke(context).Wait();
            Assert.Equal(200, context.Response.StatusCode);
        }

        [Fact]
        public void ChainedPredicatesAsync_Success()
        {
            IAppBuilder builder = new AppBuilder();
            builder.MapWhenAsync(TruePredicateAsync, map1 =>
            {
                map1.MapWhenAsync((PredicateAsync)FalsePredicateAsync, UseNotImplemented);
                map1.MapWhenAsync((PredicateAsync)TruePredicateAsync, map2 => map2.MapWhenAsync((PredicateAsync)TruePredicateAsync, UseSuccess));
                map1.Run(NotImplemented);
            });
            var app = builder.Build<OwinMiddleware>();

            IOwinContext context = new OwinContext();
            app.Invoke(context).Wait();
            Assert.Equal(200, context.Response.StatusCode);
        }
    }
}
