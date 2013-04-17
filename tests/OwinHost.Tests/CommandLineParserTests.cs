// <copyright file="CommandLineParserTests.cs" company="Microsoft Open Technologies, Inc.">
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
using OwinHost.CommandLine;
using Xunit;
using Xunit.Extensions;

namespace OwinHost.Tests
{
    public class CommandLineParserTests
    {
        [Fact]
        public void ParseEmptyParameters_Success()
        {
            CommandLineParser parser = new CommandLineParser();
            parser.Options.Add(new CommandLineOption(
                new[] { "p" }, "none", x => { throw new NotImplementedException(); }));

            IList<string> extra;
            extra = parser.Parse(new string[] { });
            Assert.Equal(0, extra.Count);
            extra = parser.Parse(new[] { string.Empty });
            Assert.Equal(1, extra.Count);
        }

        [Fact]
        public void ActionThrows_Throw()
        {
            CommandLineParser parser = new CommandLineParser();
            parser.Options.Add(new CommandLineOption(
                new[] { "p" }, "none", x => { throw new NotImplementedException(); }));

            Assert.Throws<NotImplementedException>(() => parser.Parse(new[] { "/p=v" }));
        }

        [Fact]
        public void UnknownParameter_Throw()
        {
            CommandLineParser parser = new CommandLineParser();
            parser.Options.Add(new CommandLineOption(
                new[] { "p" }, "none", x => { throw new NotImplementedException(); }));

            Assert.Throws<FormatException>(() => parser.Parse(new[] { "/x=v" }));
        }

        [Fact]
        public void MultiplyOcurringParameter_ActionInvokedMultipleTimes()
        {
            CommandLineParser parser = new CommandLineParser();
            int callbackCount = 0;
            string callbackValue = string.Empty;
            parser.Options.Add(new CommandLineOption(
                new[] { "known" }, "a known parameter",
                x => { callbackCount++; callbackValue += x; }));

            IList<string> extra = parser.Parse(new[] { "/known=a", "-known=b", "/known=\"c\"", "extra", "/known=d" });

            Assert.Equal(3, callbackCount);
            Assert.Equal("abc", callbackValue);
            Assert.Equal(2, extra.Count);
            Assert.Equal("extra", extra[0]);
            Assert.Equal("/known=d", extra[1]);
        }

        [Theory]
        [InlineData("/param", "param", "")]
        [InlineData("/paRaM", "param", "")]
        [InlineData("/param=value", "param", "value")]
        [InlineData("/param=vaLue", "param", "vaLue")]
        [InlineData("/p=value", "p", "value")]
        [InlineData("/param=", "param", "")]
        [InlineData("/param=\"value\"", "param", "value")]
        [InlineData("/param=\" value \"", "param", " value ")]
        [InlineData("/param=\"value", "param", "\"value")]
        [InlineData("/param=\"", "param", "\"")]
        [InlineData("-param", "param", "")]
        [InlineData("-paRaM", "param", "")]
        [InlineData("-param=value", "param", "value")]
        [InlineData("-param=vaLue", "param", "vaLue")]
        [InlineData("-p=value", "p", "value")]
        [InlineData("-param=", "param", "")]
        [InlineData("-param=\"value\"", "param", "value")]
        [InlineData("-param=\" value \"", "param", " value ")]
        [InlineData("-param=\"value", "param", "\"value")]
        [InlineData("-param=\"", "param", "\"")]
        public void KnownParameter_Success(string input, string param, string value)
        {
            CommandLineParser parser = new CommandLineParser();
            int callbackCount = 0;
            string callbackValue = null;
            parser.Options.Add(new CommandLineOption(
                new[] { param }, "a known parameter", 
                x => { callbackCount++; callbackValue = x; }));
            parser.Options.Add(new CommandLineOption(
                new[] { "unknown" }, "none", x => { throw new NotImplementedException(); }));

            IList<string> extra = parser.Parse(new[] { input });

            Assert.Equal(1, callbackCount);
            Assert.Equal(value, callbackValue);
            Assert.Equal(0, extra.Count);
        }

        [Theory]
        [InlineData("/")]
        [InlineData("-")]
        [InlineData("/=")]
        [InlineData("-=")]
        [InlineData("/=value")]
        [InlineData("-=value")]
        [InlineData("/ =")]
        [InlineData("- =")]
        public void BadData_ThrowFormatException(string input)
        {
            CommandLineParser parser = new CommandLineParser();
            parser.Options.Add(new CommandLineOption(
                new[] { "param" }, "none", x => { throw new NotImplementedException(); }));

            Assert.Throws<FormatException>(() => parser.Parse(new[] { input }));
        }

        [Fact]
        public void ExpectedShortUsage_Success()
        {
            string server = null, url = null, port = null, output = null,
                settings = null, verbose = null, help = null, boot = null;
            CommandLineParser parser = new CommandLineParser();
            parser.Options.Add(new CommandLineOption(new[] { "s", "server" }, string.Empty, x => server = x));
            parser.Options.Add(new CommandLineOption(new[] { "u", "url" }, string.Empty, x => url = x));
            parser.Options.Add(new CommandLineOption(new[] { "p", "port" }, string.Empty, x => port = x));
            parser.Options.Add(new CommandLineOption(new[] { "o", "output" }, string.Empty, x => output = x));
            parser.Options.Add(new CommandLineOption(new[] { "settings" }, string.Empty, x => settings = x));
            parser.Options.Add(new CommandLineOption(new[] { "v", "verbose" }, string.Empty, x => verbose = x));
            parser.Options.Add(new CommandLineOption(new[] { "?", "help" }, string.Empty, x => help = x));
            parser.Options.Add(new CommandLineOption(new[] { "b", "boot" }, string.Empty, x => boot = x));

            IList<string> extra = parser.Parse(new[] 
            {
                "-s=Microsoft.Owin.Host.HttpListener",
                "/u=http://localhost:5000/path/",
                "/p=5000",
                "/o=\"c:\\file.log\"",
                "/settings=Microsoft.Owin.Hosting.config",
                "/v",
                "/?",
                "/b=Microsoft.Owin.Boot.AspNet",
                "My.Application"
            });

            Assert.Equal("Microsoft.Owin.Host.HttpListener", server);
            Assert.Equal("http://localhost:5000/path/", url);
            Assert.Equal("5000", port);
            Assert.Equal("c:\\file.log", output);
            Assert.Equal("Microsoft.Owin.Hosting.config", settings);
            Assert.Equal(string.Empty, verbose);
            Assert.Equal(string.Empty, help);
            Assert.Equal("Microsoft.Owin.Boot.AspNet", boot);
            Assert.Equal(1, extra.Count);
            Assert.Equal("My.Application", extra[0]);
        }

        [Fact]
        public void ExpectedLongUsage_Success()
        {
            string server = null, url = null, port = null, output = null,
                settings = null, verbose = null, help = null, boot = null;
            CommandLineParser parser = new CommandLineParser();
            parser.Options.Add(new CommandLineOption(new[] { "s", "server" }, string.Empty, x => server = x));
            parser.Options.Add(new CommandLineOption(new[] { "u", "url" }, string.Empty, x => url = x));
            parser.Options.Add(new CommandLineOption(new[] { "p", "port" }, string.Empty, x => port = x));
            parser.Options.Add(new CommandLineOption(new[] { "o", "output" }, string.Empty, x => output = x));
            parser.Options.Add(new CommandLineOption(new[] { "settings" }, string.Empty, x => settings = x));
            parser.Options.Add(new CommandLineOption(new[] { "v", "verbose" }, string.Empty, x => verbose = x));
            parser.Options.Add(new CommandLineOption(new[] { "?", "help" }, string.Empty, x => help = x));
            parser.Options.Add(new CommandLineOption(new[] { "b", "boot" }, string.Empty, x => boot = x));

            IList<string> extra = parser.Parse(new[] 
            {
                "-server=Microsoft.Owin.Host.HttpListener",
                "/url=http://localhost:5000/path/",
                "/port=5000",
                "/output=\"c:\\file.log\"",
                "/settings=Microsoft.Owin.Hosting.config",
                "/verbose",
                "/help",
                "/boot=Microsoft.Owin.Boot.AspNet",
                "My.Application"
            });

            Assert.Equal("Microsoft.Owin.Host.HttpListener", server);
            Assert.Equal("http://localhost:5000/path/", url);
            Assert.Equal("5000", port);
            Assert.Equal("c:\\file.log", output);
            Assert.Equal("Microsoft.Owin.Hosting.config", settings);
            Assert.Equal(string.Empty, verbose);
            Assert.Equal(string.Empty, help);
            Assert.Equal("Microsoft.Owin.Boot.AspNet", boot);
            Assert.Equal(1, extra.Count);
            Assert.Equal("My.Application", extra[0]);
        }
    }
}
