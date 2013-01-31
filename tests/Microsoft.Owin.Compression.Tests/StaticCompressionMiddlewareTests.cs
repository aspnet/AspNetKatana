using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Owin.Testing;
using Owin;
using Shouldly;
using Xunit;

namespace Microsoft.Owin.Compression.Tests
{
    public class StaticCompressionMiddlewareTests
    {
        [Fact]
        public async Task StaticCompressionNoEffectOnSimpleRequest()
        {
            var server = TestServer.Create(app => app
                .UseStaticCompression()
                .UseFilter(async (request, response, next) =>
                {
                    response.StatusCode = 200;
                    response.SetHeader("Content-Type", "text/plain");
                    await response.Body.WriteAsync(System.Text.Encoding.UTF8.GetBytes("Hello"), 0, 5);
                }));

            var resp = await server.Path("/hello").SendAsync("GET");

            resp.Content.Headers.ContentEncoding.ShouldBeEmpty();

            server.Close();
        }

        [Fact]
        public async Task StaticCompressionWorksWithAcceptEncodingAndETag()
        {
            var server = TestServer.Create(app => app
                .UseStaticCompression()
                .UseFilter(async (request, response, next) =>
                {
                    response.StatusCode = 200;
                    response.SetHeader("Content-Type", "text/plain");
                    response.SetHeader("ETag", "\"test-etag\"");
                    await response.Body.WriteAsync(System.Text.Encoding.UTF8.GetBytes("Hello"), 0, 5);
                }));

            var resp = await server
                .Path("/hello")
                .And(req => req.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip")))
                .SendAsync("GET");

            resp.Content.Headers.ContentEncoding.ShouldBe(new[] { "gzip" });

            resp.Headers.ETag.Tag.ShouldBe("\"test-etag^gzip\"");

            server.Close();
        }
    }
}
