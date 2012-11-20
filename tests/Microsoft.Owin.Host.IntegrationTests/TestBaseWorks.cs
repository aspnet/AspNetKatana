using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Owin;
using Shouldly;
using Xunit;
using Xunit.Extensions;

namespace Microsoft.Owin.Host.IntegrationTests
{
    public class TestBaseWorks : TestBase
    {
        [Fact]
        public Task TestShouldRunWebServer()
        {
            var port = RunWebServer(
                serverName: "Microsoft.Owin.Host.SystemWeb",
                applicationName: typeof(TestBaseWorks).FullName + ".HelloWorld");

            var client = new HttpClient();

            return client.GetStringAsync("http://localhost:" + port)
                .Then(responseMessage => responseMessage.ShouldBe("Hello world!"));
        }

        public void HelloWorld(IAppBuilder app)
        {
            app.UseFunc(_ => env =>
            {
                var output = (Stream)env["owin.ResponseBody"];
                using (var writer = new StreamWriter(output))
                {
                    writer.Write("Hello world!");
                }
                return TaskHelpers.Completed();
            });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task ServerMayBeSystemWebOrHttpListener(string serverName)
        {
            var port = RunWebServer(
                serverName: serverName,
                applicationName: typeof(TestBaseWorks).FullName + ".HelloWorld");

            var client = new HttpClient();

            return client.GetStringAsync("http://localhost:" + port)
                .Then(responseMessage => responseMessage.ShouldBe("Hello world!"));
        }
    }
}
