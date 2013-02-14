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
using Owin;
using Owin.Builder;
using Xunit;

namespace Microsoft.Owin.Mapping.Tests
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    using Predicate = Func<IDictionary<string, object>, bool>;
    using PredicateAsync = Func<IDictionary<string, object>, Task<bool>>;

    public class MapPredicateMiddlewareTests
    {
        private static readonly AppFunc FuncNotImplemented = new AppFunc(_ => { throw new NotImplementedException(); });
        private static readonly Action<IAppBuilder> ActionNotImplemented = new Action<IAppBuilder>(_ => { throw new NotImplementedException(); });

        private static readonly AppFunc Success = new AppFunc(environment =>
        {
            environment["owin.ResponseStatusCode"] = 200;
            return TaskHelpers.FromResult<object>(null);
        });

        private static readonly Predicate NotImplementedPredicate = new Predicate(envionment => { throw new NotImplementedException(); });
        private static readonly PredicateAsync NotImplementedPredicateAsync = new PredicateAsync(envionment => { throw new NotImplementedException(); });

        private bool TruePredicate(IDictionary<string, object> environment)
        {
            return true;
        }

        private bool FalsePredicate(IDictionary<string, object> environment)
        {
            return false;
        }

        private Task<bool> TruePredicateAsync(IDictionary<string, object> environment)
        {
            return TaskHelpers.FromResult<bool>(true);
        }

        private Task<bool> FalsePredicateAsync(IDictionary<string, object> environment)
        {
            return TaskHelpers.FromResult<bool>(false);
        }

        [Fact]
        public void NullArguments_ArgumentNullException()
        {
            var builder = new AppBuilder();
            Assert.Throws<ArgumentNullException>(() => builder.MapPredicate(null, FuncNotImplemented));
            Assert.Throws<ArgumentNullException>(() => builder.MapPredicate(NotImplementedPredicate, (AppFunc)null));
            Assert.Throws<ArgumentNullException>(() => builder.MapPredicate(null, ActionNotImplemented));
            Assert.Throws<ArgumentNullException>(() => builder.MapPredicate(NotImplementedPredicate, (Action<IAppBuilder>)null));
            Assert.Throws<ArgumentNullException>(() => new MapPredicateMiddleware(null, FuncNotImplemented, NotImplementedPredicate));
            Assert.Throws<ArgumentNullException>(() => new MapPredicateMiddleware(FuncNotImplemented, null, NotImplementedPredicate));
            Assert.Throws<ArgumentNullException>(() => new MapPredicateMiddleware(FuncNotImplemented, FuncNotImplemented, (Predicate)null));

            Assert.Throws<ArgumentNullException>(() => builder.MapPredicateAsync(null, FuncNotImplemented));
            Assert.Throws<ArgumentNullException>(() => builder.MapPredicateAsync(NotImplementedPredicateAsync, (AppFunc)null));
            Assert.Throws<ArgumentNullException>(() => builder.MapPredicateAsync(null, ActionNotImplemented));
            Assert.Throws<ArgumentNullException>(() => builder.MapPredicateAsync(NotImplementedPredicateAsync, (Action<IAppBuilder>)null));
            Assert.Throws<ArgumentNullException>(() => new MapPredicateMiddleware(null, FuncNotImplemented, NotImplementedPredicateAsync));
            Assert.Throws<ArgumentNullException>(() => new MapPredicateMiddleware(FuncNotImplemented, null, NotImplementedPredicateAsync));
            Assert.Throws<ArgumentNullException>(() => new MapPredicateMiddleware(FuncNotImplemented, FuncNotImplemented, (PredicateAsync)null));
        }

        [Fact]
        public void PredicateTrue_BranchTaken()
        {
            IDictionary<string, object> environment = CreateEmptyRequest();
            IAppBuilder builder = new AppBuilder();
            builder.MapPredicate(TruePredicate, Success);
            var app = builder.Build<AppFunc>();
            app(environment).Wait();

            Assert.Equal(200, environment["owin.ResponseStatusCode"]);
        }

        [Fact]
        public void PredicateTrueAction_BranchTaken()
        {
            IDictionary<string, object> environment = CreateEmptyRequest();
            IAppBuilder builder = new AppBuilder();
            builder.MapPredicate(TruePredicate, subBuilder => subBuilder.Run(Success));
            var app = builder.Build<AppFunc>();
            app(environment).Wait();

            Assert.Equal(200, environment["owin.ResponseStatusCode"]);
        }

        [Fact]
        public void PredicateFalse_PassThrough()
        {
            IDictionary<string, object> environment = CreateEmptyRequest();
            IAppBuilder builder = new AppBuilder();
            builder.MapPredicate(FalsePredicate, FuncNotImplemented);
            builder.Run(Success);
            var app = builder.Build<AppFunc>();
            app(environment).Wait();

            Assert.Equal(200, environment["owin.ResponseStatusCode"]);
        }

        [Fact]
        public void PredicateFalseAction_PassThrough()
        {
            IDictionary<string, object> environment = CreateEmptyRequest();
            IAppBuilder builder = new AppBuilder();
            builder.MapPredicate(FalsePredicate, subBuilder => subBuilder.Run(FuncNotImplemented));
            builder.Run(Success);
            var app = builder.Build<AppFunc>();
            app(environment).Wait();

            Assert.Equal(200, environment["owin.ResponseStatusCode"]);
        }

        [Fact]
        public void PredicateAsyncTrue_BranchTaken()
        {
            IDictionary<string, object> environment = CreateEmptyRequest();
            IAppBuilder builder = new AppBuilder();
            builder.MapPredicateAsync(TruePredicateAsync, Success);
            var app = builder.Build<AppFunc>();
            app(environment).Wait();

            Assert.Equal(200, environment["owin.ResponseStatusCode"]);
        }

        [Fact]
        public void PredicateAsyncTrueAction_BranchTaken()
        {
            IDictionary<string, object> environment = CreateEmptyRequest();
            IAppBuilder builder = new AppBuilder();
            builder.MapPredicateAsync(TruePredicateAsync, subBuilder => subBuilder.Run(Success));
            var app = builder.Build<AppFunc>();
            app(environment).Wait();

            Assert.Equal(200, environment["owin.ResponseStatusCode"]);
        }

        [Fact]
        public void PredicateAsyncFalse_PassThrough()
        {
            IDictionary<string, object> environment = CreateEmptyRequest();
            IAppBuilder builder = new AppBuilder();
            builder.MapPredicateAsync(FalsePredicateAsync, FuncNotImplemented);
            builder.Run(Success);
            var app = builder.Build<AppFunc>();
            app(environment).Wait();

            Assert.Equal(200, environment["owin.ResponseStatusCode"]);
        }

        [Fact]
        public void PredicateAsyncFalseAction_PassThrough()
        {
            IDictionary<string, object> environment = CreateEmptyRequest();
            IAppBuilder builder = new AppBuilder();
            builder.MapPredicateAsync(FalsePredicateAsync, subBuilder => subBuilder.Run(FuncNotImplemented));
            builder.Run(Success);
            var app = builder.Build<AppFunc>();
            app(environment).Wait();

            Assert.Equal(200, environment["owin.ResponseStatusCode"]);
        }

        [Fact]
        public void ChainedPredicates_Success()
        {
            IAppBuilder builder = new AppBuilder();
            builder.MapPredicate(TruePredicate, subBuilder =>
            {
                subBuilder.MapPredicate(FalsePredicate, FuncNotImplemented);
                subBuilder.MapPredicate(TruePredicate, subBuilder1 => { subBuilder.MapPredicate(TruePredicate, Success); });
                subBuilder.Run(FuncNotImplemented);
            });
            var app = builder.Build<AppFunc>();

            IDictionary<string, object> environment = CreateEmptyRequest();
            app(environment).Wait();
            Assert.Equal(200, environment["owin.ResponseStatusCode"]);
        }

        [Fact]
        public void ChainedPredicatesAsync_Success()
        {
            IAppBuilder builder = new AppBuilder();
            builder.MapPredicateAsync(TruePredicateAsync, subBuilder =>
            {
                subBuilder.MapPredicateAsync(FalsePredicateAsync, FuncNotImplemented);
                subBuilder.MapPredicateAsync(TruePredicateAsync, subBuilder1 => { subBuilder.MapPredicateAsync(TruePredicateAsync, Success); });
                subBuilder.Run(FuncNotImplemented);
            });
            var app = builder.Build<AppFunc>();

            IDictionary<string, object> environment = CreateEmptyRequest();
            app(environment).Wait();
            Assert.Equal(200, environment["owin.ResponseStatusCode"]);
        }

        private IDictionary<string, object> CreateEmptyRequest()
        {
            IDictionary<string, object> environment = new Dictionary<string, object>();
            return environment;
        }
    }
}
