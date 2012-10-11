using System;
using System.Net.Http;
using System.Threading.Tasks;
using Katana.Engine;
using Shouldly;
using Xunit;

namespace Katana.Performance.ReferenceApp.Tests
{
    public class CanonicalRequestPatternsTests : IDisposable
    {
        private readonly IDisposable _server;

        public CanonicalRequestPatternsTests()
        {
            _server = WebApplication.Start<Startup>(port: 8080);
        }

        public void Dispose()
        {
            _server.Dispose();
        }

        [Fact]
        public async Task ShouldReturnIndex()
        {
            var client = new HttpClient();
            var response = await client.GetAsync("http://localhost:8080/");
            response.Content.Headers.ContentType.MediaType.ShouldBe("text/html");
        }

        [Fact]
        public async Task ShouldReturnSmallUrl()
        {
            var client = new HttpClient();
            var response = await client.GetAsync("http://localhost:8080/small-immediate-syncwrite");
            response.Content.Headers.ContentType.MediaType.ShouldBe("text/plain");
            var text = await response.Content.ReadAsStringAsync();
            text.Length.ShouldBe(2 << 10);
        }

        [Fact]
        public async Task ShouldReturnLargeUrl()
        {
            var client = new HttpClient();
            var response = await client.GetAsync("http://localhost:8080/large-immediate-syncwrite");
            response.Content.Headers.ContentType.MediaType.ShouldBe("text/plain");
            var text = await response.Content.ReadAsStringAsync();
            text.Length.ShouldBe(1 << 20);
        }
    }
}
