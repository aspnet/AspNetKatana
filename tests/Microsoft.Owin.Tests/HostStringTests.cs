// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Xunit;
using Xunit.Extensions;

namespace Microsoft.Owin.Tests
{
    public class HostStringTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("lower.case")]
        [InlineData("Mixed.Case")]
        [InlineData("Mixed.Case:9090")]
        [InlineData("192.168.1.1")]
        [InlineData("192.168.1.1:9090")]
        [InlineData("::1")]
        [InlineData("[::1]")]
        [InlineData("[::1]:9090")]
        [InlineData("Uni말code")]
        [InlineData("Uni말code:9090")]
        [InlineData("xn--unicode-s232a")]
        [InlineData("xn--unicode-s232a:9090")]
        public void ValueRoundTrips(string input)
        {
            HostString host = new HostString(input);
            Assert.Equal(input, host.Value, StringComparer.Ordinal);
        }

        [Theory]
        [InlineData(null, "")]
        [InlineData("", "")]
        [InlineData("lower.case", "lower.case")]
        [InlineData("Mixed.Case", "Mixed.Case")]
        [InlineData("Mixed.Case:9090", "Mixed.Case:9090")]
        [InlineData("192.168.1.1", "192.168.1.1")]
        [InlineData("192.168.1.1:9090", "192.168.1.1:9090")]
        [InlineData("::1", "[::1]")]
        [InlineData("[::1]", "[::1]")]
        [InlineData("[::1]:9090", "[::1]:9090")]
        [InlineData("Uni말code", "xn--unicode-s232a")]
        [InlineData("Uni말code:9090", "xn--unicode-s232a:9090")]
        [InlineData("xn--unicode-s232a", "xn--unicode-s232a")]
        [InlineData("xn--unicode-s232a:9090", "xn--unicode-s232a:9090")]
        [InlineData("Uni말code.xn--unicode-s232a", "xn--unicode-s232a.xn--unicode-s232a")]
        [InlineData("Uni말code.xn--unicode-s232a:9090", "xn--unicode-s232a.xn--unicode-s232a:9090")]
        public void VerifyToUriComponent(string input, string expected)
        {
            HostString host = new HostString(input);
            Assert.Equal(expected, host.ToUriComponent(), StringComparer.Ordinal);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData("lower.case", "lower.case")]
        [InlineData("Mixed.Case", "Mixed.Case")]
        [InlineData("Mixed.Case:9090", "Mixed.Case:9090")]
        [InlineData("192.168.1.1", "192.168.1.1")]
        [InlineData("192.168.1.1:9090", "192.168.1.1:9090")]
        [InlineData("::1", "::1")]
        [InlineData("[::1]", "[::1]")]
        [InlineData("[::1]:9090", "[::1]:9090")]
        [InlineData("Uni말code", "Uni말code")]
        [InlineData("Uni말code:9090", "Uni말code:9090")]
        [InlineData("xn--unicode-s232a", "uni말code")]
        [InlineData("xn--unicode-s232a:9090", "uni말code:9090")]
        public void VerifyFromUriComponent(string input, string expected)
        {
            HostString host = HostString.FromUriComponent(input);
            Assert.Equal(expected, host.Value, StringComparer.Ordinal);
        }

        [Fact]
        public void FromUriComponentMixedUnicodeThrows()
        {
            // This is a known limitation of the IdnMapping library.
            Assert.Throws<ArgumentException>(() => HostString.FromUriComponent("Uni말code.xn--unicode-s232a"));
        }

        [Theory]
        [InlineData("lower.case", "lower.case:80")]
        [InlineData("Mixed.Case", "mixed.case:80")]
        [InlineData("Mixed.Case:9090", "mixed.case:9090")]
        [InlineData("192.168.1.1", "192.168.1.1:80")]
        [InlineData("192.168.1.1:9090", "192.168.1.1:9090")]
        [InlineData("[2001:1db8:85a3:1111:1111:8a2e:1370:7334]", "[2001:1db8:85a3:1111:1111:8a2e:1370:7334]:80")]
        [InlineData("[2001:1db8:85a3:1111:1111:8a2e:1370:7334]:9090", "[2001:1db8:85a3:1111:1111:8a2e:1370:7334]:9090")]
        [InlineData("[2001:1DB8:85A3:1111:1111:8A2E:1370:7334]", "[2001:1db8:85a3:1111:1111:8a2e:1370:7334]:80")]
        [InlineData("Uni말code", "uni말code:80")]
        [InlineData("Uni말code:9090", "uni말code:9090")]
        [InlineData("xn--unicode-s232a", "uni말code:80")]
        [InlineData("xn--unicode-s232a:9090", "uni말code:9090")]
        [InlineData("Uni말code.xn--unicode-s232a", "uni말code.uni말code:80")]
        [InlineData("Uni말code.xn--unicode-s232a:9090", "uni말code.uni말code:9090")]
        public void VerifyFromUriComponentUri(string input, string expected)
        {
            Uri uri = new Uri("http://" + input);
            HostString host = HostString.FromUriComponent(uri);
            Assert.Equal(expected, host.Value, StringComparer.Ordinal);
        }

        [Fact]
        public void FromUriComponentRelativeThrows()
        {
            Assert.Throws<InvalidOperationException>(() => HostString.FromUriComponent(new Uri("bob", UriKind.Relative)));
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData("lower.case", "lower.case")]
        [InlineData("mixed.case", "Mixed.Case")]
        [InlineData("mixed.case:9090", "Mixed.Case:9090")]
        [InlineData("192.168.1.1", "192.168.1.1")]
        [InlineData("192.168.1.1:9090", "192.168.1.1:9090")]
        [InlineData("[::1]", "[::1]")]
        [InlineData("[::1]:9090", "[::1]:9090")]
        [InlineData("[::a]", "[::A]")]
        [InlineData("Uni말code", "Uni말code")]
        [InlineData("Uni말code:9090", "Uni말code:9090")]
        [InlineData("xn--unicode-s232a", "xn--unicode-s232a")]
        [InlineData("xn--unicode-s232a:9090", "xn--unicode-s232a:9090")]
        [InlineData("Uni말code.xn--unicode-s232a", "Uni말code.xn--unicode-s232a")]
        [InlineData("Uni말code.xn--unicode-s232a:9090", "Uni말code.xn--unicode-s232a:9090")]
        public void AreEqual(string first, string second)
        {
            HostString firstHost = new HostString(first);
            HostString secondHost = new HostString(second);

            Assert.True(firstHost.Equals(secondHost));
            Assert.True(secondHost.Equals(firstHost));
            Assert.Equal(firstHost.GetHashCode(), secondHost.GetHashCode());
            Assert.True(firstHost == secondHost);
            Assert.True(secondHost == firstHost);
            Assert.False(firstHost != secondHost);
            Assert.False(secondHost != firstHost);
        }

        [Theory]
        [InlineData(null, "")]
        [InlineData("Default.Port", "Default.Port:80")]
        [InlineData("192.168.1.1", "192.168.1.1:80")]
        [InlineData("::1", "[::1]")]
        [InlineData("[::1]", "[::1]:80")]
        [InlineData("Uni말code", "xn--unicode-s232a")]
        [InlineData("Uni말code:9090", "xn--unicode-s232a:9090")]
        [InlineData("Uni말code.xn--unicode-s232a", "Uni말code.Uni말codea")]
        [InlineData("Uni말code.xn--unicode-s232a:9090", "Uni말code.Uni말code:9090")]
        public void NotEquals(string first, string second)
        {
            HostString firstHost = new HostString(first);
            HostString secondHost = new HostString(second);

            Assert.False(firstHost.Equals(secondHost));
            Assert.False(secondHost.Equals(firstHost));
            Assert.NotEqual(firstHost.GetHashCode(), secondHost.GetHashCode());
            Assert.False(firstHost == secondHost);
            Assert.False(secondHost == firstHost);
            Assert.True(firstHost != secondHost);
            Assert.True(secondHost != firstHost);
        }
    }
}
