// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Xunit;

namespace Microsoft.Owin.Tests
{
    public class QueryTests
    {
        private const string QueryItemKey = "QueryItem";
        private static readonly string[] RawValues = new[] { "v1", "v2, v3", "\"v4, b\"", "v5, v6", "v7", };
        private const string JoinedValues = "v1,v2, v3,\"v4, b\",v5, v6,v7";

        private const string OriginalQueryString = "q1=v1;q2=v2,b;q3=v3;q3=v4;q4;q5=v5;q5=v5;q+6=v+6";

        [Fact]
        public void ParseQuery()
        {
            IDictionary<string, object> environment = new Dictionary<string, object>();
            environment["owin.RequestQueryString"] = OriginalQueryString;
            IOwinRequest request = new OwinRequest(environment);
            Assert.Equal(OriginalQueryString, request.QueryString.Value);

            IReadableStringCollection query = request.Query;
            Assert.Equal("v1", query.Get("q1"));
            Assert.Equal("v2,b", query.Get("Q2"));
            Assert.Equal("v3,v4", query.Get("q3"));
            Assert.Null(query.Get("q4"));
            Assert.Equal("v5,v5", query.Get("Q5"));
            Assert.Equal("v 6", query.Get("Q 6"));
        }

        [Fact]
        public void GetMissing_null()
        {
            IReadableStringCollection query = CreateQuery(null);
            Assert.Null(query[QueryItemKey]);
            Assert.Null(query.Get(QueryItemKey));
            Assert.Null(query.GetValues(QueryItemKey));
        }

        [Fact]
        public void GetIndex_Merged()
        {
            IReadableStringCollection query = CreateQuery(RawValues);
            string values = query[QueryItemKey];
            Assert.Equal(JoinedValues, values);
        }

        [Fact]
        public void Get_Merged()
        {
            IReadableStringCollection query = CreateQuery(RawValues);
            string values = query.Get(QueryItemKey);
            Assert.Equal(JoinedValues, values);
        }

        [Fact]
        public void GetValues_Unchanged()
        {
            IReadableStringCollection query = CreateQuery(RawValues);
            IList<string> values = query.GetValues(QueryItemKey);
            Assert.Equal(RawValues, values);
        }

        private IReadableStringCollection CreateQuery(string[] values)
        {
            IDictionary<string, string[]> queryValues = new Dictionary<string, string[]>();
            queryValues[QueryItemKey] = values;
            return new ReadableStringCollection(queryValues);
        }
    }
}
