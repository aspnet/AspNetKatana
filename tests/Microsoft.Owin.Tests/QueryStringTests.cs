using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shouldly;
using Xunit;
using Xunit.Extensions;

namespace Microsoft.Owin.Tests
{
    public class QueryStringTests
    {
        [Theory]
        [InlineData("one=two")]
        [InlineData("one=two&three&four=five")]
        [InlineData("/unusual?but#tolerable")]
        [InlineData("/")]
        [InlineData("")]
        [InlineData(null)]
        public void ConstructorAndValuePropertyArePassThrough(string value)
        {
            var queryString = new QueryString(value);
            queryString.Value.ShouldBe(value);
        }

        [Theory]
        [InlineData("one=two", "?one=two")]
        [InlineData("one=two&three&four=five", "?one=two&three&four=five")]
        [InlineData("/unusual?but#tolerable", "?/unusual?but%23tolerable")]
        [InlineData("/", "?/")]
        [InlineData("", "")]
        [InlineData(null, "")]
        public void ToUriComponentWillPrependQueryDelimiterAndEscapeFragmentDelimeter(string value, string uriComponent)
        {
            var queryString = new QueryString(value);
            queryString.ToUriComponent().ShouldBe(uriComponent);
            queryString.ToString().ShouldBe(uriComponent);
        }

        [Theory]
        [InlineData("?one=two", "one=two")]
        [InlineData("???one=two", "??one=two")]
        [InlineData("?one=two&thr$ee&four=five", "one=two&thr$ee&four=five")]
        [InlineData("?one=two&thr%24ee&four=five", "one=two&thr%24ee&four=five")]
        [InlineData("?/unusual?but#tolerable", "/unusual?but#tolerable")]
        [InlineData("?/unusual?but%23tolerable", "/unusual?but%23tolerable")]
        [InlineData("?", "")]
        [InlineData("???", "??")]
        [InlineData("", "")]
        [InlineData(null, "")]
        public void FromUriComponentWillRemoveQueryDelimiterAsAppropriate(string uriComponent, string value)
        {
            var queryString = QueryString.FromUriComponent(uriComponent);
            queryString.Value.ShouldBe(value);
        }

        [Theory]
        [InlineData("http://example.com/?one=two", "one=two")]
        [InlineData("http://example.com/???one=two", "??one=two")]
        [InlineData("http://example.com/?one=two&thr$ee&four=five", "one=two&thr$ee&four=five")]
        [InlineData("http://example.com/?one=two&thr%24ee&four=five", "one=two&thr$ee&four=five")]
        [InlineData("http://example.com/?/unusual?but#tolerable", "/unusual?but")]
        [InlineData("http://example.com/?/unusual?but%23tolerable", "/unusual?but#tolerable")]
        [InlineData("http://example.com/?", "")]
        [InlineData("http://example.com/???", "??")]
        [InlineData("http://example.com/", "")]
        [InlineData("http://example.com", "")]
        public void FromUriObjectTakesQueryStringValueAsAppropriate(string uriComponent, string value)
        {
            var queryString = QueryString.FromUriComponent(new Uri(uriComponent));
            queryString.Value.ShouldBe(value);
        }

        [Fact]
        public void LeadingDelimiterIsNotOptionalWhenDataPresent()
        {
            Should.Throw<ArgumentException>(() => QueryString.FromUriComponent("one=two"));
        }
    }
}
