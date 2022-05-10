// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Owin.Infrastructure;
using Xunit;

namespace Microsoft.Owin.Tests
{
    public class CookieChunkingTests
    {
        [Fact]
        public void AppendLargeCookie_Appended()
        {
            IOwinContext context = new OwinContext();

            string testString = "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            new ChunkingCookieManager() { ChunkSize = null }.AppendResponseCookie(context, "TestCookie", testString, new CookieOptions());
            IList<string> values = context.Response.Headers.GetValues("Set-Cookie");
            Assert.Equal(1, values.Count);
            Assert.Equal("TestCookie=" + testString + "; path=/", values[0]);
        }

        [Fact]
        public void AppendLargeCookieWithLimit_Chunked()
        {
            IOwinContext context = new OwinContext();

            string testString = "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            new ChunkingCookieManager() { ChunkSize = 30 }.AppendResponseCookie(context, "TestCookie", testString, new CookieOptions());
            IList<string> values = context.Response.Headers.GetValues("Set-Cookie");
            Assert.Equal(9, values.Count);
            Assert.Equal(new[]
            {
                "TestCookie=chunks:8; path=/",
                "TestCookieC1=abcdefgh; path=/",
                "TestCookieC2=ijklmnop; path=/",
                "TestCookieC3=qrstuvwx; path=/",
                "TestCookieC4=yz012345; path=/",
                "TestCookieC5=6789ABCD; path=/",
                "TestCookieC6=EFGHIJKL; path=/",
                "TestCookieC7=MNOPQRST; path=/",
                "TestCookieC8=UVWXYZ; path=/",
            }, values);
        }

        [Fact]
        public void AppendLargeQuotedCookieWithLimit_QuotedChunked()
        {
            IOwinContext context = new OwinContext();

            string testString = "\"abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ\"";
            new ChunkingCookieManager() { ChunkSize = 32 }.AppendResponseCookie(context, "TestCookie", testString, new CookieOptions());
            IList<string> values = context.Response.Headers.GetValues("Set-Cookie");
            Assert.Equal(9, values.Count);
            Assert.Equal(new[]
            {
                "TestCookie=chunks:8; path=/",
                "TestCookieC1=\"abcdefgh\"; path=/",
                "TestCookieC2=\"ijklmnop\"; path=/",
                "TestCookieC3=\"qrstuvwx\"; path=/",
                "TestCookieC4=\"yz012345\"; path=/",
                "TestCookieC5=\"6789ABCD\"; path=/",
                "TestCookieC6=\"EFGHIJKL\"; path=/",
                "TestCookieC7=\"MNOPQRST\"; path=/",
                "TestCookieC8=\"UVWXYZ\"; path=/",
            }, values);
        }

        [Fact]
        public void GetLargeChunkedCookie_Reassembled()
        {
            IOwinContext context = new OwinContext();
            context.Request.Headers.AppendValues("Cookie",
                "TestCookie=chunks:7",
                "TestCookieC1=abcdefghi",
                "TestCookieC2=jklmnopqr",
                "TestCookieC3=stuvwxyz0",
                "TestCookieC4=123456789",
                "TestCookieC5=ABCDEFGHI",
                "TestCookieC6=JKLMNOPQR",
                "TestCookieC7=STUVWXYZ");

            string result = new ChunkingCookieManager().GetRequestCookie(context, "TestCookie");
            string testString = "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            Assert.Equal(testString, result);
        }

        [Fact]
        public void GetLargeChunkedCookieWithQuotes_Reassembled()
        {
            IOwinContext context = new OwinContext();
            context.Request.Headers.AppendValues("Cookie",
                "TestCookie=chunks:7",
                "TestCookieC1=\"abcdefghi\"",
                "TestCookieC2=\"jklmnopqr\"",
                "TestCookieC3=\"stuvwxyz0\"",
                "TestCookieC4=\"123456789\"",
                "TestCookieC5=\"ABCDEFGHI\"",
                "TestCookieC6=\"JKLMNOPQR\"",
                "TestCookieC7=\"STUVWXYZ\"");

            string result = new ChunkingCookieManager().GetRequestCookie(context, "TestCookie");
            string testString = "\"abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ\"";
            Assert.Equal(testString, result);
        }

        [Fact]
        public void GetLargeChunkedCookieWithMissingChunk_ThrowingEnabled_Throws()
        {
            IOwinContext context = new OwinContext();
            context.Request.Headers.AppendValues("Cookie",
                "TestCookie=chunks:7",
                "TestCookieC1=abcdefghi",
                // Missing chunk "TestCookieC2=jklmnopqr",
                "TestCookieC3=stuvwxyz0",
                "TestCookieC4=123456789",
                "TestCookieC5=ABCDEFGHI",
                "TestCookieC6=JKLMNOPQR",
                "TestCookieC7=STUVWXYZ");

            Assert.Throws<FormatException>(() => new ChunkingCookieManager().GetRequestCookie(context, "TestCookie"));
        }

        [Fact]
        public void GetLargeChunkedCookieWithMissingChunk_ThrowingDisabled_NotReassembled()
        {
            IOwinContext context = new OwinContext();
            context.Request.Headers.AppendValues("Cookie",
                "TestCookie=chunks:7",
                "TestCookieC1=abcdefghi",
                // Missing chunk "TestCookieC2=jklmnopqr",
                "TestCookieC3=stuvwxyz0",
                "TestCookieC4=123456789",
                "TestCookieC5=ABCDEFGHI",
                "TestCookieC6=JKLMNOPQR",
                "TestCookieC7=STUVWXYZ");

            string result = new ChunkingCookieManager() { ThrowForPartialCookies = false }.GetRequestCookie(context, "TestCookie");
            string testString = "chunks:7";
            Assert.Equal(testString, result);
        }

        [Fact]
        public void DeleteChunkedCookieWithOptions_AllDeleted()
        {
            IOwinContext context = new OwinContext();
            context.Request.Headers.AppendValues("Cookie", "TestCookie=chunks:7;TestCookieC1=1;TestCookieC2=2;TestCookieC3=3;TestCookieC4=4;TestCookieC5=5;TestCookieC6=6;TestCookieC7=7");

            new ChunkingCookieManager().DeleteCookie(context, "TestCookie", new CookieOptions() { Domain = "foo.com" });
            var cookies = context.Response.Headers.GetValues("Set-Cookie");
            Assert.Equal(8, cookies.Count);
            Assert.Equal(new[]
            {
                "TestCookie=; domain=foo.com; path=/; expires=Thu, 01-Jan-1970 00:00:00 GMT",
                "TestCookieC1=; domain=foo.com; path=/; expires=Thu, 01-Jan-1970 00:00:00 GMT",
                "TestCookieC2=; domain=foo.com; path=/; expires=Thu, 01-Jan-1970 00:00:00 GMT",
                "TestCookieC3=; domain=foo.com; path=/; expires=Thu, 01-Jan-1970 00:00:00 GMT",
                "TestCookieC4=; domain=foo.com; path=/; expires=Thu, 01-Jan-1970 00:00:00 GMT",
                "TestCookieC5=; domain=foo.com; path=/; expires=Thu, 01-Jan-1970 00:00:00 GMT",
                "TestCookieC6=; domain=foo.com; path=/; expires=Thu, 01-Jan-1970 00:00:00 GMT",
                "TestCookieC7=; domain=foo.com; path=/; expires=Thu, 01-Jan-1970 00:00:00 GMT",
            }, cookies);
        }

        [Fact]
        public void DeleteChunkedCookieWithMissingRequestCookies_OnlyPresentCookiesDeleted()
        {
            IOwinContext context = new OwinContext();
            context.Request.Headers.Append("Cookie", "TestCookie=chunks:7;TestCookieC1=1;TestCookieC2=2");
            new ChunkingCookieManager().DeleteCookie(context, "TestCookie", new CookieOptions() { Domain = "foo.com", Secure = true });
            var cookies = context.Response.Headers.GetValues("Set-Cookie");
            Assert.Equal(3, cookies.Count);
            Assert.Equal(new[]
            {
                "TestCookie=; domain=foo.com; path=/; expires=Thu, 01-Jan-1970 00:00:00 GMT; secure",
                "TestCookieC1=; domain=foo.com; path=/; expires=Thu, 01-Jan-1970 00:00:00 GMT; secure",
                "TestCookieC2=; domain=foo.com; path=/; expires=Thu, 01-Jan-1970 00:00:00 GMT; secure",
            }, cookies);
        }

        [Fact]
        public void DeleteChunkedCookieWithMissingRequestCookies_StopsAtMissingChunk()
        {
            IOwinContext context = new OwinContext();
            // C3 is missing so we don't try to delete C4 either.
            context.Request.Headers.Append("Cookie", "TestCookie=chunks:7;TestCookieC1=1;TestCookieC2=2;TestCookieC4=4");
            new ChunkingCookieManager().DeleteCookie(context, "TestCookie", new CookieOptions() { Domain = "foo.com", Secure = true });
            var cookies = context.Response.Headers.GetValues("Set-Cookie");
            Assert.Equal(3, cookies.Count);
            Assert.Equal(new[]
            {
                "TestCookie=; domain=foo.com; path=/; expires=Thu, 01-Jan-1970 00:00:00 GMT; secure",
                "TestCookieC1=; domain=foo.com; path=/; expires=Thu, 01-Jan-1970 00:00:00 GMT; secure",
                "TestCookieC2=; domain=foo.com; path=/; expires=Thu, 01-Jan-1970 00:00:00 GMT; secure",
            }, cookies);
        }
    }
}
