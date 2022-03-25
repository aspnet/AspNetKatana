// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Xunit;

namespace Microsoft.Owin.Tests
{
    public class RequestCookieTests
    {
        [Theory]
        [InlineData("key=value", "key", "value")]
        [InlineData("__secure-key=value", "__secure-key", "value")]
        [InlineData("key%2C=%21value", "key,", "!value")]
        [InlineData("ke%23y%2C=val%5Eue", "ke#y,", "val^ue")]
        [InlineData("base64=QUI%2BREU%2FRw%3D%3D", "base64", "QUI+REU/Rw==")]
        [InlineData("base64=QUI+REU/Rw==", "base64", "QUI+REU/Rw==")]
        public void UnEscapesValues(string input, string expectedKey, string expectedValue)
        {
            var context = new OwinRequest();
            context.Headers["Cookie"] = input;
            var cookies = context.Cookies;

            var cookie = Assert.Single(cookies);
            Assert.Equal(Uri.EscapeDataString(expectedKey), cookie.Key);
            Assert.Equal(expectedValue, cookies[expectedKey]);
        }
    }
}
