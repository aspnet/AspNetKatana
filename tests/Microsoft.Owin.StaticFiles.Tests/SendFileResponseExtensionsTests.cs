// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Owin.StaticFiles.Tests
{
    public class SendFileResponseExtensionsTests
    {
        [Fact]
        public void SendFileSupport()
        {
            IOwinResponse response = new OwinResponse();
            Assert.False(response.SupportsSendFile());
            // response.Set("sendfile.SendAsync", new Object());
            // Assert.False(response.SupportsSendFile()); // Get<type> throw for type mismatch.
            response.Set("sendfile.SendAsync", new Func<string, long, long?, CancellationToken, Task>((_, __, ___, ____) => Task.FromResult(0)));
            Assert.True(response.SupportsSendFile());
        }

        [Fact]
        public async Task SendFileWhenNotSupported()
        {
            IOwinResponse response = new OwinResponse();
            await Assert.ThrowsAsync<NotSupportedException>(() => response.SendFileAsync("foo"));
        }

        [Fact]
        public async Task SendFileWorks()
        {
            IOwinResponse response = new OwinResponse();
            string name = null;
            long offset = 0;
            long? length = null;
            CancellationToken token;
            Func<string, long, long?, CancellationToken, Task> func = (n, o, l, c) =>
                {
                    name = n;
                    offset = o;
                    length = l;
                    token = c;
                    return Task.FromResult(0);
                };

            response.Set("sendfile.SendAsync", func);

            await response.SendFileAsync("bob", 1, 3, CancellationToken.None);
            Assert.Equal("bob", name);
            Assert.Equal(1, offset);
            Assert.Equal(3, length);
            Assert.Equal(CancellationToken.None, token);
        }
    }
}
