using System;
using System.Collections.Generic;
using System.Web.Routing;
using Katana.Server.AspNet.CallEnvironment;
using Shouldly;
using Xunit;

namespace Katana.Server.AspNet.Tests.CallEnvironment
{
    public class AspNetEnvironmentTests
    {
        private AspNetEnvironment _aspNetEnvironment;
        private IDictionary<string, object> _env;

        public AspNetEnvironmentTests()
        {
            _env = _aspNetEnvironment = new AspNetEnvironment();
        }

        [Fact]
        public void KnownPropertiesCanBeSetAndFetchedViaIndexerAndAppearAsProperties()
        {
            var requestContext = new RequestContext();

            _env["System.Web.Routing.RequestContext"] = requestContext;

            _env["System.Web.Routing.RequestContext"].ShouldBe(requestContext);
            _aspNetEnvironment.RequestContext.ShouldBe(requestContext);
        }

        [Fact]
        public void UnknownPropertiesCanBeSetAndFetchedViaIndexerAndAreAddedToExtra()
        {
            var custom = new Object();

            _env["Custom"] = custom;
            _env["Custom"].ShouldBe(custom);
            _aspNetEnvironment.Extra.ShouldContainKeyAndValue("Custom", custom);
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

            _aspNetEnvironment.RequestContext.ShouldBe(requestContext);
            _aspNetEnvironment.Extra.ShouldContainKeyAndValue("Custom", custom);
        }

        [Fact]
        public void KeysShouldContainAddedKnownPropertiesAndAddedExtras()
        {
            _env["System.Web.Routing.RequestContext"] = new RequestContext();
            _env["Custom"] = new object();

            _env.Keys.ShouldBe(new[] { "System.Web.Routing.RequestContext", "Custom" });
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

            _aspNetEnvironment.RequestContext.ShouldBe(null);
            _aspNetEnvironment.Extra.ShouldNotContainKey("Custom");
        }

        [Fact]
        public void TryGetValueShouldReturnKnownPropertyOrAddedExtra()
        {
            var requestContext = new RequestContext();
            var custom = new Object();

            _env.Add("System.Web.Routing.RequestContext", requestContext);
            _env.Add("Custom", custom);

            object value1;
            var bool1 = _env.TryGetValue("System.Web.Routing.RequestContext", out value1);
            object value2;
            var bool2 = _env.TryGetValue("Custom", out value2);
            object value3;
            var bool3 = _env.TryGetValue("NotKnown", out value3);

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

            _aspNetEnvironment.RequestContext.ShouldBe(null);
            _aspNetEnvironment.Extra.Count.ShouldBe(0);
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
            var count = _env.Count;

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
    }
}
