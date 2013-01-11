// -----------------------------------------------------------------------
// <copyright file="TestData.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gate;
using Owin;

namespace Microsoft.AspNet.Razor.Owin.Tests
{
    internal static class TestData
    {
        public static IDictionary<string, object> CreateCallParams(
            string method = "GET",
            string path = "/",
            string pathBase = "",
            string queryString = "",
            string scheme = "http",
            string version = "1.0")
        {
            var cp = new Dictionary<string, object>() 
            {
                { "owin.RequestMethod", method },
                { "owin.RequestPath", path },
                { "owin.RequestPathBase", pathBase },
                { "owin.RequestQueryString", queryString },
                { "owin.RequestScheme", scheme },
                { "owin.Version", version }
            };
            return cp;
        }

        public static Request CreateRequest(
            string method = "GET",
            string path = "/",
            string pathBase = "",
            string queryString = "",
            string scheme = "http",
            string version = "1.0")
        {
            return new Request(CreateCallParams(
                method, path, pathBase, queryString, scheme, version));
        }

        public static TestFile CreateDummyFile()
        {
            return new TestFile("Irrel", "evan", "t");
        }
    }
}
