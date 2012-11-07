// <copyright file="AspNetEnvironmentTests.cs" company="Katana contributors">
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
using System.Web.Routing;
using FakeN.Web;
using Microsoft.Owin.Host.SystemWeb.CallEnvironment;
using Microsoft.Owin.Host.SystemWeb.Tests.FakeN;
using Owin;
using Shouldly;
using Xunit;

namespace Microsoft.Owin.Host.SystemWeb.Tests.CallEnvironment
{
    public class AspNetEnvironmentTests
    {
        private readonly AspNetDictionary _aspNetDictionary;
        private readonly IDictionary<string, object> _env;

        public AspNetEnvironmentTests()
        {
            OwinAppContext appContext = new OwinAppContext();
            appContext.Initialize(_ => { });
            RequestContext requestContext = new RequestContext(new FakeHttpContextEx(), new RouteData());
            OwinCallContext callContext = appContext.CreateCallContext(requestContext, string.Empty, string.Empty, null, null);
            callContext.Execute();
            _env = _aspNetDictionary = callContext.Environment;
        }

        [Fact]
        public void KnownPropertiesCanBeSetAndFetchedViaIndexerAndAppearAsProperties()
        {
            var requestContext = new RequestContext();

            _env["System.Web.Routing.RequestContext"] = requestContext;

            _env["System.Web.Routing.RequestContext"].ShouldBe(requestContext);
            _aspNetDictionary.RequestContext.ShouldBe(requestContext);
        }

        [Fact]
        public void UnknownPropertiesCanBeSetAndFetchedViaIndexerAndAreAddedToExtra()
        {
            var custom = new Object();

            _env["Custom"] = custom;
            _env["Custom"].ShouldBe(custom);
            _aspNetDictionary.Extra.ShouldContainKeyAndValue("Custom", custom);
        }

        [Fact]
        public void AddShouldHitPropertiesFirstAndExtraSecond()
        {
            var requestContext = new RequestContext();
            var custom = new Object();

            _env.Add("System.Web.Routing.RequestContext", requestContext);
            _env.Add("Custom", custom);

            _env["System.Web.Routing.RequestContext"].ShouldBe(requestContext);
            _env["Custom"].ShouldBe(custom);

            _aspNetDictionary.RequestContext.ShouldBe(requestContext);
            _aspNetDictionary.Extra.ShouldContainKeyAndValue("Custom", custom);
        }

        [Fact]
        public void KeysShouldContainAddedKnownPropertiesAndAddedExtras()
        {
            _env["System.Web.Routing.RequestContext"] = new RequestContext();
            _env["Custom"] = new object();

            _env.Keys.ShouldContain("System.Web.Routing.RequestContext");
            _env.Keys.ShouldContain("Custom");
        }

        [Fact]
        public void RemoveShouldNullKnownPropertiesAndRemoveAddedExtras()
        {
            var requestContext = new RequestContext();
            var custom = new Object();

            _env.Add("System.Web.Routing.RequestContext", requestContext);
            _env.Add("Custom", custom);

            _env.Remove("System.Web.Routing.RequestContext");
            _env.Remove("Custom");

            _aspNetDictionary.RequestContext.ShouldBe(null);
            _aspNetDictionary.Extra.ShouldNotContainKey("Custom");
        }

        [Fact]
        public void TryGetValueShouldReturnKnownPropertyOrAddedExtra()
        {
            var requestContext = new RequestContext();
            var custom = new Object();

            _env.Add("System.Web.Routing.RequestContext", requestContext);
            _env.Add("Custom", custom);

            object value1;
            bool bool1 = _env.TryGetValue("System.Web.Routing.RequestContext", out value1);
            object value2;
            bool bool2 = _env.TryGetValue("Custom", out value2);
            object value3;
            bool bool3 = _env.TryGetValue("NotKnown", out value3);

            bool1.ShouldBe(true);
            value1.ShouldBe(requestContext);
            bool2.ShouldBe(true);
            value2.ShouldBe(custom);
            bool3.ShouldBe(false);
            value3.ShouldBe(null);
        }

        [Fact]
        public void ValuesShouldContainKnownPropertiesAndAddedExtras()
        {
            var requestContext = new RequestContext();
            var custom = new Object();

            _env["System.Web.Routing.RequestContext"] = requestContext;
            _env["Custom"] = custom;

            _env.Values.ShouldContain(requestContext);
            _env.Values.ShouldContain(custom);
        }

        [Fact]
        public void ClearNullsOutPropertiesAndClearsExtra()
        {
            var requestContext = new RequestContext();
            var custom = new Object();

            _env["System.Web.Routing.RequestContext"] = requestContext;
            _env["Custom"] = custom;

            _env.Clear();

            _aspNetDictionary.RequestContext.ShouldBe(null);
            _aspNetDictionary.Extra.Count.ShouldBe(0);
        }

        [Fact]
        public void ContainsShouldWorkOnPropertiesOrAddedExtras()
        {
            var requestContext = new RequestContext();
            var custom = new Object();

            _env["System.Web.Routing.RequestContext"] = requestContext;
            _env["Custom"] = custom;

            _env.Contains(new KeyValuePair<string, object>("System.Web.Routing.RequestContext", requestContext)).ShouldBe(true);
            _env.Contains(new KeyValuePair<string, object>("System.Web.Routing.RequestContext", null)).ShouldBe(false);
            _env.Contains(new KeyValuePair<string, object>("Custom", custom)).ShouldBe(true);
            _env.Contains(new KeyValuePair<string, object>("Unknown", custom)).ShouldBe(false);
            _env.Contains(new KeyValuePair<string, object>("Custom", new Object())).ShouldBe(false);
        }

        [Fact]
        public void CountIsBasedOnAllKnownPropertiesAndExtraValues()
        {
            int count = _env.Count;

            _env["Custom"] = new object();

            _env.Count.ShouldNotBe(0);
            _env.Count.ShouldBe(count + 1);
        }

        [Fact]
        public void RemoveKeyValueWorksOnExactMatches()
        {
            var requestContext = new RequestContext();
            var custom = new Object();

            _env["System.Web.Routing.RequestContext"] = requestContext;
            _env["Custom"] = custom;

            _env.Remove(new KeyValuePair<string, object>("System.Web.Routing.RequestContext", null)).ShouldBe(false);
            _env.Remove(new KeyValuePair<string, object>("Custom", null)).ShouldBe(false);
            _env.Remove(new KeyValuePair<string, object>("System.Web.Routing.RequestContext", requestContext)).ShouldBe(true);
            _env.Remove(new KeyValuePair<string, object>("Custom", custom)).ShouldBe(true);
        }

        [Fact]
        public void IterationShouldWalkThroughAllKnownAndExtraKeyValuePairs()
        {
            var requestContext = new RequestContext();
            var custom = new Object();

            _env["System.Web.Routing.RequestContext"] = requestContext;
            _env["Custom"] = custom;

            var dict = new Dictionary<string, object>();
            foreach (var kv in _env)
            {
                dict.Add(kv.Key, kv.Value);
            }

            dict.ShouldContainKeyAndValue("System.Web.Routing.RequestContext", requestContext);
            dict.ShouldContainKeyAndValue("Custom", custom);
        }

        [Fact]
        public void EmptyEnvironmentShouldBeIterable()
        {
            foreach (var kv in _env)
            {
            }
        }

        [Fact]
        public void EmptyEnvironmentShouldBeClearable()
        {
            _env.Clear();
            _env.Count.ShouldBe(0);
        }
    }
}
