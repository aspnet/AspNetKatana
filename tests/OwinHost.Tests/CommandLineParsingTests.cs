// <copyright file="CommandLineParsingTests.cs" company="Microsoft Open Technologies, Inc.">
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

using System.Collections.Generic;
using Microsoft.Owin.Hosting;
using OwinHost.Options;
using Shouldly;
using Xunit;

namespace OwinHost.Tests
{
    public class CommandLineParsingTests
    {
        [Fact]
        public void CommandModelWillParseIntegers()
        {
            var model = new CommandModel()
                .Option<int>("foo", string.Empty, (cmd, v) => cmd.Get<Dictionary<string, int>>()["foo"] = v)
                .Option<int>("bar", string.Empty, (cmd, v) => cmd.Get<Dictionary<string, int>>()["bar"] = v);

            var cmd1 = model.Parse(new[] { "/foo", "123", "/bar:456" });
            cmd1.Get<Dictionary<string, int>>()["foo"].ShouldBe(123);
            cmd1.Get<Dictionary<string, int>>()["bar"].ShouldBe(456);

            var cmd2 = model.Parse(new[] { "--foo", "123", "--bar:456" });
            cmd2.Get<Dictionary<string, int>>()["foo"].ShouldBe(123);
            cmd2.Get<Dictionary<string, int>>()["bar"].ShouldBe(456);
        }

        [Fact]
        public void CommandModelWillParseStrings()
        {
            var model = new CommandModel()
                .Option<string>("foo", string.Empty, (cmd, v) => cmd.Get<Dictionary<string, string>>()["foo"] = v)
                .Option<string>("bar", string.Empty, (cmd, v) => cmd.Get<Dictionary<string, string>>()["bar"] = v);

            var cmd1 = model.Parse("/foo", "123", "/bar:456");
            cmd1.Get<Dictionary<string, string>>()["foo"].ShouldBe("123");
            cmd1.Get<Dictionary<string, string>>()["bar"].ShouldBe("456");

            var cmd2 = model.Parse("--foo", "123", "--bar:456");
            cmd2.Get<Dictionary<string, string>>()["foo"].ShouldBe("123");
            cmd2.Get<Dictionary<string, string>>()["bar"].ShouldBe("456");
        }

        [Fact]
        public void ShortNameWorksAsOption()
        {
            var model = new CommandModel()
                .Option<string>("foo", "f", string.Empty, (cmd, v) => cmd.Get<Dictionary<string, string>>()["foo"] = v)
                .Option<string>("bar", "b", string.Empty, (cmd, v) => cmd.Get<Dictionary<string, string>>()["bar"] = v);

            var cmd1 = model.Parse("-f", "123", "-b:456");
            cmd1.Get<Dictionary<string, string>>()["foo"].ShouldBe("123");
            cmd1.Get<Dictionary<string, string>>()["bar"].ShouldBe("456");
        }

        [Fact]
        public void LongNameIsCaseInsensitive()
        {
            var model = new CommandModel()
                .Option<string>("foo", "f", string.Empty, (cmd, v) => cmd.Get<Dictionary<string, string>>()["foo"] = v)
                .Option<string>("bar", "b", string.Empty, (cmd, v) => cmd.Get<Dictionary<string, string>>()["bar"] = v);

            var cmd1 = model.Parse("--FoO", "123", "--BaR:456");
            cmd1.Get<Dictionary<string, string>>()["foo"].ShouldBe("123");
            cmd1.Get<Dictionary<string, string>>()["bar"].ShouldBe("456");

            var cmd2 = model.Parse("/FoO", "123", "/BaR:456");
            cmd2.Get<Dictionary<string, string>>()["foo"].ShouldBe("123");
            cmd2.Get<Dictionary<string, string>>()["bar"].ShouldBe("456");
        }

        [Fact]
        public void ShortNameIsCaseSensitive()
        {
            var model = new CommandModel()
                .Option<string>("foo", "f", string.Empty, (cmd, v) => cmd.Get<Dictionary<string, string>>()["foo"] = v)
                .Option<string>("bar", "F", string.Empty, (cmd, v) => cmd.Get<Dictionary<string, string>>()["bar"] = v);

            var cmd1 = model.Parse("-f", "123", "-F", "456");
            cmd1.Get<Dictionary<string, string>>()["foo"].ShouldBe("123");
            cmd1.Get<Dictionary<string, string>>()["bar"].ShouldBe("456");

            var cmd2 = model.Parse("-f:123", "-F:456");
            cmd2.Get<Dictionary<string, string>>()["foo"].ShouldBe("123");
            cmd2.Get<Dictionary<string, string>>()["bar"].ShouldBe("456");
        }

        [Fact]
        public void ProgramHelpIsRecognized()
        {
            var model = Program.CreateCommandModel();
            model.Parse("-?").Model.Name.ShouldBe("{show help}");
            model.Parse("/?").Model.Name.ShouldBe("{show help}");
            model.Parse("-h").Model.Name.ShouldBe("{show help}");
            model.Parse("/h").Model.Name.ShouldBe("{show help}");
            model.Parse("--help").Model.Name.ShouldBe("{show help}");
            model.Parse("/help").Model.Name.ShouldBe("{show help}");
            model.Parse("--HeLp").Model.Name.ShouldBe("{show help}");
            model.Parse("/HeLp").Model.Name.ShouldBe("{show help}");
        }

        [Fact]
        public void ProgamOptionsAreParsed()
        {
            var model = Program.CreateCommandModel();
            model.Parse("-u", "hello").Get<StartOptions>().Urls.ShouldBe(new[] { "hello" });
            model.Parse("-u:hello").Get<StartOptions>().Urls.ShouldBe(new[] { "hello" });
            model.Parse("--url:hello").Get<StartOptions>().Urls.ShouldBe(new[] { "hello" });
            model.Parse("--url", "hello").Get<StartOptions>().Urls.ShouldBe(new[] { "hello" });

            model.Parse("-u", "1", "-u", "2").Get<StartOptions>().Urls.ShouldBe(new[] { "1", "2" });

            model.Parse("--port", "8080").Get<StartOptions>().Port.ShouldBe(8080);
            model.Parse("--server", "alpha").Get<StartOptions>().ServerFactory.ShouldBe("alpha");

            model.Parse("MyProgram.Startup").Get<StartOptions>().AppStartup.ShouldBe("MyProgram.Startup");

            var options = model.Parse(
                "-s:Microsoft.Owin.Host.HttpListener",
                "-u:http://localhost:5000/path/",
                "-p:5000",
                "-o:\"c:\\file.log\"",
                "--settings:Microsoft.Owin.Hosting.settings",
                "-v:value?",
                "-b:Microsoft.Owin.Boot.AspNet",
                "My.Application, MyAssembly").Get<StartOptions>();

            options.ServerFactory.ShouldBe("Microsoft.Owin.Host.HttpListener");
            options.Urls.ShouldBe(new[] { "http://localhost:5000/path/" });
            options.Port.ShouldBe(5000);
            options.Settings.ShouldBe(new Dictionary<string, string>
            {
                { "boot", "Microsoft.Owin.Boot.AspNet" },
                { "alpha", "42" },
                { "traceoutput", "\"c:\\file.log\"" },
                { "traceverbosity", "value?" },
            });
            options.AppStartup.ShouldBe("My.Application, MyAssembly");
        }
    }
}
