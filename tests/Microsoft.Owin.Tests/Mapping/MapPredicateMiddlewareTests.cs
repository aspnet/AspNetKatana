// <copyright file="MapPredicateMiddlewareTests.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Threading.Tasks;
using Microsoft.Owin.Builder;
using Owin;
using Xunit;

namespace Microsoft.Owin.Mapping.Tests
{
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
            var noMiddleware = new AppBuilder().Build<OwinMiddleware>();
            Assert.Throws<ArgumentNullException>(() => builder.MapWhen(null, UseNotImplemented));
            Assert.Throws<ArgumentNullException>(() => builder.MapWhen(NotImplementedPredicate, (Action<IAppBuilder>)null));
            Assert.Throws<ArgumentNullException>(() => new MapWhenMiddleware(null, NotImplementedPredicate, noMiddleware));
            Assert.Throws<ArgumentNullException>(() => new MapWhenMiddleware(noMiddleware, NotImplementedPredicate, null));
            Assert.Throws<ArgumentNullException>(() => new MapWhenMiddleware(noMiddleware, (Predicate)null, noMiddleware));

            Assert.Throws<ArgumentNullException>(() => builder.MapWhenAsync(null, UseNotImplemented));
            Assert.Throws<ArgumentNullException>(() => builder.MapWhenAsync(NotImplementedPredicateAsync, (Action<IAppBuilder>)null));
            Assert.Throws<ArgumentNullException>(() => new MapWhenMiddleware(null, NotImplementedPredicateAsync, noMiddleware));
            Assert.Throws<ArgumentNullException>(() => new MapWhenMiddleware(noMiddleware, NotImplementedPredicateAsync, null));
            Assert.Throws<ArgumentNullException>(() => new MapWhenMiddleware(noMiddleware, (PredicateAsync)null, noMiddleware));
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
