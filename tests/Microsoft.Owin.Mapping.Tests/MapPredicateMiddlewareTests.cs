// <copyright file="MapPredicateMiddlewareTests.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
// </copyright>
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Owin;
using Owin.Builder;
using Xunit;
using Xunit.Extensions;

namespace Microsoft.Owin.Mapping.Tests
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    using Predicate = Func<IDictionary<string, object>, bool>;
    using PredicateAsync = Func<IDictionary<string, object>, Task<bool>>;

    public class MapPredicateMiddlewareTests
    {
        private static readonly AppFunc FuncNotImplemented = new AppFunc(_ => { throw new NotImplementedException(); });
        private static readonly Action<IAppBuilder> ActionNotImplemented = new Action<IAppBuilder>(_ => { throw new NotImplementedException(); });
        private static readonly AppFunc Success = new AppFunc(environment => { environment["owin.ResponseStatusCode"] = 200; return null; });
        private static readonly Predicate TruePredicate = new Predicate(envonment => true);
        private static readonly Predicate FalsePredicate = new Predicate(envonment => false);
        private static readonly Predicate NotImplementedPredicate = new Predicate(envonment => { throw new NotImplementedException(); });
        private static readonly PredicateAsync TruePredicateAsync = new PredicateAsync(envonment => TaskHelpers.FromResult<bool>(true));
        private static readonly PredicateAsync FalsePredicateAsync = new PredicateAsync(envonment => TaskHelpers.FromResult<bool>(false));
        private static readonly PredicateAsync NotImplementedPredicateAsync = new PredicateAsync(envonment => { throw new NotImplementedException(); });

        [Fact]
        public void NullArguments_ArgumentNullException()
        {
            AppBuilder builder = new AppBuilder();
            Assert.Throws<ArgumentNullException>(() => builder.MapPredicate((Predicate)null, FuncNotImplemented));
            Assert.Throws<ArgumentNullException>(() => builder.MapPredicate(NotImplementedPredicate, (AppFunc)null));
            Assert.Throws<ArgumentNullException>(() => builder.MapPredicate((Predicate)null, ActionNotImplemented));
            Assert.Throws<ArgumentNullException>(() => builder.MapPredicate(NotImplementedPredicate, (Action<IAppBuilder>)null));
            Assert.Throws<ArgumentNullException>(() => new MapPredicateMiddleware(null, FuncNotImplemented, NotImplementedPredicate));
            Assert.Throws<ArgumentNullException>(() => new MapPredicateMiddleware(FuncNotImplemented, null, NotImplementedPredicate));
            Assert.Throws<ArgumentNullException>(() => new MapPredicateMiddleware(FuncNotImplemented, FuncNotImplemented, null));

            Assert.Throws<ArgumentNullException>(() => builder.MapPredicate((PredicateAsync)null, FuncNotImplemented));
            Assert.Throws<ArgumentNullException>(() => builder.MapPredicate(NotImplementedPredicateAsync, (AppFunc)null));
            Assert.Throws<ArgumentNullException>(() => builder.MapPredicate((PredicateAsync)null, ActionNotImplemented));
            Assert.Throws<ArgumentNullException>(() => builder.MapPredicate(NotImplementedPredicateAsync, (Action<IAppBuilder>)null));
            Assert.Throws<ArgumentNullException>(() => new MapPredicateAsyncMiddleware(null, FuncNotImplemented, NotImplementedPredicateAsync));
            Assert.Throws<ArgumentNullException>(() => new MapPredicateAsyncMiddleware(FuncNotImplemented, null, NotImplementedPredicateAsync));
            Assert.Throws<ArgumentNullException>(() => new MapPredicateAsyncMiddleware(FuncNotImplemented, FuncNotImplemented, null));
        }

        [Fact]
        public void PredicateTrue_BranchTaken()
        {
            IDictionary<string, object> environment = CreateEmptyRequest();
            IAppBuilder builder = new AppBuilder();
            builder.MapPredicate(TruePredicate, Success);
            AppFunc app = builder.Build<AppFunc>();
            app(environment);

            Assert.Equal(200, environment["owin.ResponseStatusCode"]);
        }

        [Fact]
        public void PredicateTrueAction_BranchTaken()
        {
            IDictionary<string, object> environment = CreateEmptyRequest();
            IAppBuilder builder = new AppBuilder();
            builder.MapPredicate(TruePredicate, subBuilder => subBuilder.Run(Success));
            AppFunc app = builder.Build<AppFunc>();
            app(environment);

            Assert.Equal(200, environment["owin.ResponseStatusCode"]);
        }

        [Fact]
        public void PredicateFalse_PassThrough()
        {
            IDictionary<string, object> environment = CreateEmptyRequest();
            IAppBuilder builder = new AppBuilder();
            builder.MapPredicate(FalsePredicate, FuncNotImplemented);
            builder.Run(Success);
            AppFunc app = builder.Build<AppFunc>();
            app(environment);

            Assert.Equal(200, environment["owin.ResponseStatusCode"]);
        }

        [Fact]
        public void PredicateFalseAction_PassThrough()
        {
            IDictionary<string, object> environment = CreateEmptyRequest();
            IAppBuilder builder = new AppBuilder();
            builder.MapPredicate(FalsePredicate, subBuilder => subBuilder.Run(FuncNotImplemented));
            builder.Run(Success);
            AppFunc app = builder.Build<AppFunc>();
            app(environment);

            Assert.Equal(200, environment["owin.ResponseStatusCode"]);
        }

        [Fact]
        public void PredicateAsyncTrue_BranchTaken()
        {
            IDictionary<string, object> environment = CreateEmptyRequest();
            IAppBuilder builder = new AppBuilder();
            builder.MapPredicate(TruePredicateAsync, Success);
            AppFunc app = builder.Build<AppFunc>();
            app(environment);

            Assert.Equal(200, environment["owin.ResponseStatusCode"]);
        }

        [Fact]
        public void PredicateAsyncTrueAction_BranchTaken()
        {
            IDictionary<string, object> environment = CreateEmptyRequest();
            IAppBuilder builder = new AppBuilder();
            builder.MapPredicate(TruePredicateAsync, subBuilder => subBuilder.Run(Success));
            AppFunc app = builder.Build<AppFunc>();
            app(environment);

            Assert.Equal(200, environment["owin.ResponseStatusCode"]);
        }

        [Fact]
        public void PredicateAsyncFalse_PassThrough()
        {
            IDictionary<string, object> environment = CreateEmptyRequest();
            IAppBuilder builder = new AppBuilder();
            builder.MapPredicate(FalsePredicateAsync, FuncNotImplemented);
            builder.Run(Success);
            AppFunc app = builder.Build<AppFunc>();
            app(environment);

            Assert.Equal(200, environment["owin.ResponseStatusCode"]);
        }

        [Fact]
        public void PredicateAsyncFalseAction_PassThrough()
        {
            IDictionary<string, object> environment = CreateEmptyRequest();
            IAppBuilder builder = new AppBuilder();
            builder.MapPredicate(FalsePredicateAsync, subBuilder => subBuilder.Run(FuncNotImplemented));
            builder.Run(Success);
            AppFunc app = builder.Build<AppFunc>();
            app(environment);

            Assert.Equal(200, environment["owin.ResponseStatusCode"]);
        }

        [Fact]
        public void ChainedPredicates_Success()
        {
            IAppBuilder builder = new AppBuilder();
            builder.MapPredicate(TruePredicate, subBuilder =>
                {
                    subBuilder.MapPredicate(FalsePredicate, FuncNotImplemented);
                    subBuilder.MapPredicate(TruePredicate, subBuilder1 =>
                        {
                            subBuilder.MapPredicate(TruePredicate, Success);
                        });
                    subBuilder.Run(FuncNotImplemented);
                });
            AppFunc app = builder.Build<AppFunc>();

            IDictionary<string, object> environment = CreateEmptyRequest();
            app(environment);
            Assert.Equal(200, environment["owin.ResponseStatusCode"]);
        }

        [Fact]
        public void ChainedPredicatesAsync_Success()
        {
            IAppBuilder builder = new AppBuilder();
            builder.MapPredicate(TruePredicateAsync, subBuilder =>
            {
                subBuilder.MapPredicate(FalsePredicateAsync, FuncNotImplemented);
                subBuilder.MapPredicate(TruePredicateAsync, subBuilder1 =>
                {
                    subBuilder.MapPredicate(TruePredicateAsync, Success);
                });
                subBuilder.Run(FuncNotImplemented);
            });
            AppFunc app = builder.Build<AppFunc>();

            IDictionary<string, object> environment = CreateEmptyRequest();
            app(environment);
            Assert.Equal(200, environment["owin.ResponseStatusCode"]);
        }

        private IDictionary<string, object> CreateEmptyRequest()
        {
            IDictionary<string, object> environment = new Dictionary<string, object>();
            return environment;
        }
    }
}
