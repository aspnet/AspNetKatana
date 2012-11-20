using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Owin;
using Shouldly;
using Xunit.Extensions;

namespace Microsoft.Owin.Host.IntegrationTests
{
    public class SimpleGetTests : TestBase
    {
        public void TextHtmlAlpha(IAppBuilder app)
        {
            app.UseFunc(next => env =>
            {
                var headers = (IDictionary<string, string[]>)env["owin.ResponseHeaders"];
                var body = (Stream)env["owin.ResponseBody"];

                headers["Content-Type"] = new string[] { "text/html" };
                
                using (var writer = new StreamWriter(body))
                {
                    writer.Write("<p>alpha</p>");
                }

                return TaskHelpers.Completed();
            });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task ResponseBodyShouldArrive(string serverName)
        {
            var port = RunWebServer(
                serverName,
                typeof(SimpleGetTests).FullName + ".TextHtmlAlpha");

            var client = new HttpClient();
            return client.GetStringAsync("http://localhost:" + port + "/text")
                .Then(body => body.ShouldBe("<p>alpha</p>"));
        }


        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task ContentTypeShouldBeSet(string serverName)
        {
            var port = RunWebServer(
                serverName,
                typeof(SimpleGetTests).FullName + ".TextHtmlAlpha");

            var client = new HttpClient();
            return client.GetAsync("http://localhost:" + port + "/text")
                .Then(message => message.Content.Headers.ContentType.MediaType.ShouldBe("text/html"));
        }

    }
}
