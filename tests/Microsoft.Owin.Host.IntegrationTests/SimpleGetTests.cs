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
        public void Configuration(IAppBuilder app)
        {
            app.UseFunc(_ => Invoke);
        }

        public Task Invoke(IDictionary<string, object> env)
        {
            var headers = (IDictionary<string, string[]>)env["owin.ResponseHeaders"];
            headers["Content-Type"] = new string[] { "text/plain" };
            var body = (Stream)env["owin.ResponseBody"];
            using (var writer = new StreamWriter(body))
            {
                writer.Write("<p>alpha</p>");
            }
            return TaskHelpers.Completed();
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public void ResponseBodyShouldArrive(string serverName)
        {
            var port = base.RunWebServer(
                serverName,
                typeof(SimpleGetTests).FullName);

            var client = new HttpClient();
            client.GetStringAsync("http://localhost:" + port + "/text")
                .Then(body => body.ShouldBe("<p>alpha</p>"));
        }


        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public void ContentTypeShouldBeSet(string serverName)
        {
            var port = base.RunWebServer(
                serverName,
                typeof(SimpleGetTests).FullName);

            var client = new HttpClient();
            client.GetAsync("http://localhost:" + port + "/text")
                .Then(message => message.Content.Headers.ContentType.MediaType.ShouldBe("text/html"));
        }

    }
}
