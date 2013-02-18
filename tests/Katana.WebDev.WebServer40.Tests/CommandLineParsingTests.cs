using System.Collections.Generic;
using Katana.WebDev.WebServer40.Options;
using Shouldly;
using Xunit;

namespace Katana.WebDev.WebServer40.Tests
{
    public class CommandLineParsingTests
    {
        [Fact]
        public void CommandModelWillParseIntegers()
        {
            var model = new CommandModel()
                .Option<int>("foo", (cmd, v) => cmd.Get<Dictionary<string, int>>()["foo"] = v)
                .Option<int>("bar", (cmd, v) => cmd.Get<Dictionary<string, int>>()["bar"] = v);

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
                .Option<string>("foo", (cmd, v) => cmd.Get<Dictionary<string, string>>()["foo"] = v)
                .Option<string>("bar", (cmd, v) => cmd.Get<Dictionary<string, string>>()["bar"] = v);

            var cmd1 = model.Parse( "/foo", "123", "/bar:456" );
            cmd1.Get<Dictionary<string, string>>()["foo"].ShouldBe("123");
            cmd1.Get<Dictionary<string, string>>()["bar"].ShouldBe("456");

            var cmd2 = model.Parse( "--foo", "123", "--bar:456" );
            cmd2.Get<Dictionary<string, string>>()["foo"].ShouldBe("123");
            cmd2.Get<Dictionary<string, string>>()["bar"].ShouldBe("456");
        }

        [Fact] 
        public void InstallCommandIsRecognized()
        {
            var cmd = Program.CreateCommandModel().Parse("install");
            cmd.Model.Name.ShouldBe("install");
        }
    }
}
