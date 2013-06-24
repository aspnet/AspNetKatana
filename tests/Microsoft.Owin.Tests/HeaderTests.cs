// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Microsoft.Owin.Tests
{
    public class HeaderTests
    {
        private const string CustomHeaderKey = "Custom-Header";
        private readonly string[] CustomHeaderRawValues = new[] { "v1", "v2, v3", "\"v4, b\"", "v5, v6", "v7", };
        private const string CustomHeaderJoinedValues = "v1,v2, v3,\"v4, b\",v5, v6,v7";
        private readonly IEnumerable<string> CustomHeaderSplitValues = new[] { "v1", "v2", "v3", "v4, b", "v5", "v6", "v7", };

        [Fact]
        public void GetMissing_null()
        {
            IHeaderDictionary headers = CreateHeaders(null);
            Assert.Null(headers[CustomHeaderKey]);
            Assert.Null(headers.Get(CustomHeaderKey));
            Assert.Null(headers.GetValues(CustomHeaderKey));
            Assert.Null(headers.GetCommaSeparatedValues(CustomHeaderKey));
        }

        [Fact]
        public void GetIndex_Merged()
        {
            IHeaderDictionary headers = CreateHeaders(CustomHeaderRawValues);
            string values = headers[CustomHeaderKey];
            Assert.Equal(CustomHeaderJoinedValues, values);
        }

        [Fact]
        public void Get_Merged()
        {
            IHeaderDictionary headers = CreateHeaders(CustomHeaderRawValues);
            string values = headers.Get(CustomHeaderKey);
            Assert.Equal(CustomHeaderJoinedValues, values);
        }

        [Fact]
        public void GetValues_Unchanged()
        {
            IHeaderDictionary headers = CreateHeaders(CustomHeaderRawValues);
            IList<string> values = headers.GetValues(CustomHeaderKey);
            Assert.Equal(CustomHeaderRawValues, values);
        }

        [Fact]
        public void GetComaSeperatedValues_SplitAndUnquoted()
        {
            IHeaderDictionary headers = CreateHeaders(CustomHeaderRawValues);
            IList<string> values = headers.GetCommaSeparatedValues(CustomHeaderKey);
            Assert.Equal(CustomHeaderSplitValues, values);
        }

        [Fact]
        public void SetNullOrEmpty_Remove()
        {
            IHeaderDictionary headers;

            headers = CreateHeaders(CustomHeaderRawValues);
            headers[CustomHeaderKey] = null;
            Assert.Null(headers[CustomHeaderKey]);

            headers = CreateHeaders(CustomHeaderRawValues);
            headers[CustomHeaderKey] = string.Empty;
            Assert.Null(headers[CustomHeaderKey]);

            headers = CreateHeaders(CustomHeaderRawValues);
            headers.Set(CustomHeaderKey, null);
            Assert.Null(headers.Get(CustomHeaderKey));

            headers = CreateHeaders(CustomHeaderRawValues);
            headers.Set(CustomHeaderKey, string.Empty);
            Assert.Null(headers.Get(CustomHeaderKey));

            headers = CreateHeaders(CustomHeaderRawValues);
            headers.SetValues(CustomHeaderKey, (string[])null);
            Assert.Null(headers.GetValues(CustomHeaderKey));

            headers = CreateHeaders(CustomHeaderRawValues);
            headers.SetValues(CustomHeaderKey, new string[0]);
            Assert.Null(headers.GetValues(CustomHeaderKey));

            headers = CreateHeaders(CustomHeaderRawValues);
            headers.SetCommaSeparatedValues(CustomHeaderKey, (string[])null);
            Assert.Null(headers.GetCommaSeparatedValues(CustomHeaderKey));

            headers = CreateHeaders(CustomHeaderRawValues);
            headers.SetCommaSeparatedValues(CustomHeaderKey, new string[0]);
            Assert.Null(headers.GetCommaSeparatedValues(CustomHeaderKey));
        }

        [Fact]
        public void SetIndex_Overwrites()
        {
            IHeaderDictionary headers = CreateHeaders(CustomHeaderRawValues);
            headers[CustomHeaderKey] = "vA, vB";
            IList<string> values = headers.GetValues(CustomHeaderKey);
            Assert.Equal(new[] { "vA, vB" }, values);
        }

        [Fact]
        public void Set_Overwrites()
        {
            IHeaderDictionary headers = CreateHeaders(CustomHeaderRawValues);
            headers.Set(CustomHeaderKey, "vA, vB");
            IList<string> values = headers.GetValues(CustomHeaderKey);
            Assert.Equal(new[] { "vA, vB" }, values);
        }

        [Fact]
        public void SetValues_Overwrites()
        {
            IHeaderDictionary headers = CreateHeaders(CustomHeaderRawValues);
            headers.SetValues(CustomHeaderKey, "vA, vB", "vC");
            IList<string> values = headers.GetValues(CustomHeaderKey);
            Assert.Equal(new[] { "vA, vB", "vC" }, values);
        }

        [Fact]
        public void SetComaSeperatedValues_QuotesJoinsOverwrites()
        {
            IHeaderDictionary headers = CreateHeaders(CustomHeaderRawValues);
            headers.SetCommaSeparatedValues(CustomHeaderKey, "vA, vB", "vC");
            IList<string> values = headers.GetValues(CustomHeaderKey);
            Assert.Equal(new[] { "\"vA, vB\",vC" }, values);
        }

        [Fact]
        public void AppendNullOrEmpty_Unchanged()
        {
            IHeaderDictionary headers;

            headers = CreateHeaders(CustomHeaderRawValues);
            headers.Append(CustomHeaderKey, null);
            Assert.Equal(CustomHeaderRawValues, headers.GetValues(CustomHeaderKey));

            headers = CreateHeaders(CustomHeaderRawValues);
            headers.Append(CustomHeaderKey, string.Empty);
            Assert.Equal(CustomHeaderRawValues, headers.GetValues(CustomHeaderKey));

            headers = CreateHeaders(CustomHeaderRawValues);
            headers.AppendValues(CustomHeaderKey, (string[])null);
            Assert.Equal(CustomHeaderRawValues, headers.GetValues(CustomHeaderKey));

            headers = CreateHeaders(CustomHeaderRawValues);
            headers.AppendValues(CustomHeaderKey, new string[0]);
            Assert.Equal(CustomHeaderRawValues, headers.GetValues(CustomHeaderKey));

            headers = CreateHeaders(CustomHeaderRawValues);
            headers.AppendCommaSeparatedValues(CustomHeaderKey, (string[])null);
            Assert.Equal(CustomHeaderRawValues, headers.GetValues(CustomHeaderKey));

            headers = CreateHeaders(CustomHeaderRawValues);
            headers.AppendCommaSeparatedValues(CustomHeaderKey, new string[0]);
            Assert.Equal(CustomHeaderRawValues, headers.GetValues(CustomHeaderKey));
        }

        [Fact]
        public void Append_MergesAndAppends()
        {
            IHeaderDictionary headers = CreateHeaders(CustomHeaderRawValues);
            headers.Append(CustomHeaderKey, "vA, vB");
            IList<string> values = headers.GetValues(CustomHeaderKey);
            Assert.Equal(new[] { CustomHeaderJoinedValues + ",vA, vB" }, values);
        }

        [Fact]
        public void AppendValues_AppendsToList()
        {
            IHeaderDictionary headers = CreateHeaders(CustomHeaderRawValues);
            headers.AppendValues(CustomHeaderKey, "vA, vB", "vC");
            IList<string> values = headers.GetValues(CustomHeaderKey);
            Assert.Equal(CustomHeaderRawValues.Concat(new[] { "vA, vB", "vC" }), values);
        }

        [Fact]
        public void AppendComaSeperatedValues_QuotesJoinsAppends()
        {
            IHeaderDictionary headers = CreateHeaders(CustomHeaderRawValues);
            headers.AppendCommaSeparatedValues(CustomHeaderKey, "vA, vB", "vC");
            IList<string> values = headers.GetValues(CustomHeaderKey);
            Assert.Equal(new[] { CustomHeaderJoinedValues + ",\"vA, vB\",vC" }, values);
        }

        private IHeaderDictionary CreateHeaders(string[] orriginalValues)
        {
            IDictionary<string, string[]> dictionary = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            if (orriginalValues != null)
            {
                dictionary[CustomHeaderKey] = orriginalValues;
            }
            return new HeaderDictionary(dictionary);
        }
    }
}
