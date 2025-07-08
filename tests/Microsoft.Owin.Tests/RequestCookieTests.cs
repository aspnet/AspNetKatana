// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Xunit;

namespace Microsoft.Owin.Tests
{
    public class RequestCookieTests
    {
        [Theory]
        [InlineData("key=value", "key", "value")]
        [InlineData("__secure-key=value", "__secure-key", "value")]
        [InlineData("key%2C=%21value", "key%2C", "!value")]
        [InlineData("ke%23y%2C=val%5Eue", "ke%23y%2C", "val^ue")]
        [InlineData("base64=QUI%2BREU%2FRw%3D%3D", "base64", "QUI+REU/Rw==")]
        [InlineData("base64=QUI+REU/Rw==", "base64", "QUI+REU/Rw==")]
        public void UnEscapesValues(string input, string expectedKey, string expectedValue)
        {
            var context = new OwinRequest();
            context.Headers["Cookie"] = input;
            var cookies = context.Cookies;

            var cookie = Assert.Single(cookies);
            Assert.Equal(expectedKey, cookie.Key);
            Assert.Equal(expectedValue, cookies[expectedKey]);
        }

        [Theory]
        [InlineData("key", null, null)]
        [InlineData("=,key=value", new[] { "" }, new[] { ",key=value" })]
        [InlineData(",key=value", new[] { ",key" }, new[] { "value" })]
        [InlineData(",key=value; key=value2", new[] { ",key", "key" }, new[] { "value", "value2" })]
        [InlineData("key=value; ,key2=value2", new[] { "key", ",key2" }, new[] { "value", "value2" })]
        [InlineData("%6bey=value; key=value2", new[] { "%6bey", "key" }, new[] { "value", "value2" })]
        [InlineData("key=value; key2=value2", new[] { "key", "key2" }, new[] { "value", "value2" })]
        [InlineData("key=value; key=value2", new[] { "key" }, new[] { "value" })]
        public void ParseCookies(string input, string[] expectedKeys, string[] expectedValues)
        {
            var context = new OwinRequest();
            context.Headers["Cookie"] = input;
            var cookies = context.Cookies.ToArray();

            if (expectedKeys == null)
            {
                Assert.Empty(cookies);
            }
            else
            {
                Assert.Equal(expectedKeys.Length, cookies.Length);

                for (var i = 0; i < expectedKeys.Length; i++)
                {
                    Assert.Equal(expectedKeys[i], cookies[i].Key);
                    Assert.Equal(expectedValues[i], cookies[i].Value);
                }
            }
        }
    }
}
