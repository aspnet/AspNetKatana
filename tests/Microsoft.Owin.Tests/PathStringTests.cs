// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
    public class PathStringTests
    {
        [Theory]
        [InlineData("/path")]
        [InlineData("/path/two")]
        [InlineData("/path?two")]
        [InlineData("/")]
        [InlineData("")]
        [InlineData(null)]
        public void ConstructorAndValuePropertyArePassThrough(string value)
        {
            var path = new PathString(value);
            path.Value.ShouldBe(value);
        }

        [Theory]
        [InlineData("/path", "/path")]
        [InlineData("/path/two", "/path/two")]
        [InlineData("/path?two", "/path%3Ftwo")]
        [InlineData("/path#two", "/path%23two")]
        // pchar = unreserved / pct-encoded / sub-delims / ":" / "@"
        [InlineData("/abcd1234%-._~!$&'()*+,;=:@?#[]", "/abcd1234%25-._~!$&'()*+,;=:@%3F%23%5B%5D")]
        public void ToUriComponentWillEscapeAsAppropriate(string value, string uriComponent)
        {
            var path = new PathString(value);
            path.ToUriComponent().ShouldBe(uriComponent);
            path.ToString().ShouldBe(uriComponent);
        }

        [Theory]
        [InlineData("/path", "/path")]
        [InlineData("/path/two", "/path/two")]
        [InlineData("/path%2Ftwo", "/path/two")]
        [InlineData("/path%3Ftwo", "/path?two")]
        [InlineData("/path?two", "/path?two")]
        [InlineData("/path#two", "/path#two")]
        public void FromUriComponentWillUnescapeAsAppropriate(string uriComponent, string value)
        {
            var path = PathString.FromUriComponent(uriComponent);
            path.Value.ShouldBe(value);
        }

        [Theory]
        [InlineData("http://example.com/path", "/path")]
        [InlineData("http://example.com/path/two", "/path/two")]
        [InlineData("http://example.com/path%3Ftwo?three#four", "/path?two")]
        [InlineData("http://example.com/path%23two?three#four", "/path#two")]
        [InlineData("http://example.com/", "/")]
        [InlineData("http://example.com", "/")]
        [InlineData("http://example.com/?three#four", "/")]
        [InlineData("http://example.com?three#four", "/")]
        public void UriMayBeUsedAsFromUriComponentParameter(string uriString, string value)
        {
            var path = PathString.FromUriComponent(new Uri(uriString));
            path.Value.ShouldBe(value);
        }

        [Theory]
        [InlineData("/path", "/Path", true)]
        [InlineData("/path/two", "/path/TWo", true)]
        [InlineData("/PATH", "/path", true)]
        [InlineData("/path/", "/path", false)]
        [InlineData("/path-one", "/path_one", false)]
        public void EqualityIsImplementedAndOrdinalIgnoreCaseByDefault(string value1, string value2, bool equal)
        {
            var path1 = new PathString(value1);
            var path2 = new PathString(value2);
            var object1 = (object)path1;
            var object2 = (object)path2;
            var object1B = object1;

            // value equality
            (path1 == path2).ShouldBe(equal);
            (path1 != path2).ShouldBe(!equal);
            (path1.Equals(path2)).ShouldBe(equal);
            (Equals(path1, path2)).ShouldBe(equal);

            // reference equality
            ReferenceEquals(object1, object2).ShouldBe(false);
            ReferenceEquals(object1, object1B).ShouldBe(true);
            (object1 == object2).ShouldBe(false);
            (object1 != object2).ShouldBe(true);
            (object1 == object1B).ShouldBe(true);
            (object1 != object1B).ShouldBe(false);

            // object equality
            (object1.Equals(object2)).ShouldBe(equal);
            (Equals(object1, object2)).ShouldBe(equal);

            // Compile error: (path1 == object2).ShouldBe(equal);
            // Compile error: (path1 != object2).ShouldBe(!equal);
            (path1.Equals(object2)).ShouldBe(equal);
            (Equals(path1, object2)).ShouldBe(equal);

            // Compile error: (object1 == path2).ShouldBe(equal);
            // Compile error: (object1 != path2).ShouldBe(!equal);
            (object1.Equals(path2)).ShouldBe(equal);
            (Equals(object1, path2)).ShouldBe(equal);

            // hash code equality
            (path1.GetHashCode() == path2.GetHashCode()).ShouldBe(equal);
            (object1.GetHashCode() == object2.GetHashCode()).ShouldBe(equal);
            (object1.GetHashCode() == object2.GetHashCode()).ShouldBe(equal);
            (path1.GetHashCode() == object2.GetHashCode()).ShouldBe(equal);

            // dictionary equality
            var dictionaryPathString = new Dictionary<PathString, object>();
            dictionaryPathString[path1] = "first";
            dictionaryPathString[path2] = "second";
            (dictionaryPathString.Count == 1).ShouldBe(equal);

            var dictionaryObject = new Dictionary<object, object>();
            dictionaryObject[path1] = "first";
            dictionaryObject[path2] = "second";
            (dictionaryObject.Count == 1).ShouldBe(equal);
        }

        [Theory]
        [InlineData("/one/two", "/three/four", "/one/two/three/four")]
        [InlineData("/one?two", "/three#four", "/one%3Ftwo/three%23four")]
        public void PathStringMayBeCombined(string value1, string value2, string escapedCombination)
        {
            var path1 = new PathString(value1);
            var path2 = new PathString(value2);
            var object1 = path1;
            var object2 = path2;

            (path1 + path2).Value.ShouldBe(value1 + value2);
            (path1 + path2).ToString().ShouldBe(escapedCombination);
            (path1 + path2).ToUriComponent().ShouldBe(escapedCombination);

            (object1 + object2).Value.ShouldBe(value1 + value2);
            (object1 + object2).ToString().ShouldBe(escapedCombination);
            (object1 + object2).ToUriComponent().ShouldBe(escapedCombination);

            (path1 + object2).Value.ShouldBe(value1 + value2);
            (path1 + object2).ToString().ShouldBe(escapedCombination);
            (path1 + object2).ToUriComponent().ShouldBe(escapedCombination);

            (object1 + path2).Value.ShouldBe(value1 + value2);
            (object1 + path2).ToString().ShouldBe(escapedCombination);
            (object1 + path2).ToUriComponent().ShouldBe(escapedCombination);
        }

        [Fact]
        public void PathMustStartWithSlash()
        {
            Should.Throw<ArgumentException>(() => new PathString("hello")).ParamName.ShouldBe("value");
            Should.Throw<ArgumentException>(() => new PathString("../hello")).ParamName.ShouldBe("value");
        }

        [Fact]
        public void EscapingIsCorrectWhenUserDefinedPathHasValueWhichHappensToBeAnEscapeSequence()
        {
            var singleEscapedPath = new PathString("/one%2Ftwo");
            singleEscapedPath.Value.ShouldBe("/one%2Ftwo");

            var doubleEscapedString = singleEscapedPath.ToUriComponent();
            doubleEscapedString.ShouldBe("/one%2Ftwo");

            var recreatedPath = PathString.FromUriComponent(doubleEscapedString);
            recreatedPath.Value.ShouldBe("/one/two");
            recreatedPath.ToUriComponent().ShouldBe("/one/two");
        }

        [Theory]
        [InlineData("/oNe/two", "/one", true, "/two")]
        [InlineData("/oNe", "/one", true, "")]
        [InlineData("/oNe-two", "/one", false, null)]
        [InlineData("/oNe", "/one/two", false, null)]
        [InlineData("/oNe", "/one-two", false, null)]
        [InlineData("/oNe/", "/one", true, "/")]
        [InlineData("/oNe/", "", true, "/oNe/")]
        [InlineData("/", "", true, "/")]
        [InlineData("/", "/", true, "")]
        [InlineData("", "", true, "")]
        [InlineData("", "/", false, null)]
        [InlineData(null, "", true, "")]
        [InlineData(null, null, true, "")]
        [InlineData("", null, true, "")]
        public void StartsWithPerformsIgnoreCaseLeadingPathMatch(
            string pathValue,
            string matchValue,
            bool startsWith,
            string remainingValue)
        {
            var path = new PathString(pathValue);
            var match = new PathString(matchValue);
            path.StartsWithSegments(match).ShouldBe(startsWith);

            PathString remaining;
            path.StartsWithSegments(match, out remaining).ShouldBe(startsWith);
            if (startsWith)
            {
                remaining.Value.ShouldBe(remainingValue);
            }
            else
            {
                remaining.ShouldBe(PathString.Empty);
                remaining.ShouldBe(new PathString(String.Empty));
                remaining.Value.ShouldBe(String.Empty);
                remaining.HasValue.ShouldBe(false);
            }
        }

        [Theory]
        [InlineData("/one", "two=three", "/one?two=three")]
        [InlineData("/one/", "two=three", "/one/?two=three")]
        [InlineData("/", "two=three", "/?two=three")]
        [InlineData("", "two=three", "?two=three")]
        [InlineData(null, "two=three", "?two=three")]
        [InlineData("/one", "", "/one")]
        [InlineData("/one/", "", "/one/")]
        [InlineData("/", "", "/")]
        [InlineData("", "", "")]
        [InlineData(null, "", "")]
        [InlineData("", null, "")]
        [InlineData(null, null, "")]
        [InlineData("/", null, "/")]
        public void PathAndQueryStringAreCombinable(
            string pathValue,
            string queryValue,
            string combinedValue)
        {
            var path = new PathString(pathValue);
            var query = new QueryString(queryValue);
            path.Add(query).ShouldBe(combinedValue);
            (path + query).ShouldBe(combinedValue);
        }
    }
}
