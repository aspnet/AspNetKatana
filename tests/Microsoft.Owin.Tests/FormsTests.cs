// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace Microsoft.Owin.Tests
{
    public class FormsTests
    {
        private const string FormsItemKey = "FormsItem";
        private static readonly string[] RawValues = new[] { "v1", "v2, v3", "\"v4, b\"", "v5, v6", "v7", };
        private const string JoinedValues = "v1,v2, v3,\"v4, b\",v5, v6,v7";

        private const string OriginalFormsString = "q1=v1&q2=v2,b&q3=v3&q3=v4&q4&q5=v5&q5=v5&q+6=v+6";

        [Fact]
        public void ParseForm()
        {
            IDictionary<string, object> environment = new Dictionary<string, object>();
            MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(OriginalFormsString));
            environment["owin.RequestBody"] = stream;

            IOwinRequest request = new OwinRequest(environment);

            IFormCollection form = request.ReadFormAsync().Result;
            Assert.Equal("v1", form.Get("q1"));
            Assert.Equal("v2,b", form.Get("Q2"));
            Assert.Equal("v3,v4", form.Get("q3"));
            Assert.Null(form.Get("q4"));
            Assert.Equal("v5,v5", form.Get("Q5"));
            Assert.Equal("v 6", form.Get("Q 6"));
            Assert.True(stream.CanRead);
        }

        [Fact]
        public void GetMissing_null()
        {
            IFormCollection form = CreateForm(null);
            Assert.Null(form[FormsItemKey]);
            Assert.Null(form.Get(FormsItemKey));
            Assert.Null(form.GetValues(FormsItemKey));
        }

        [Fact]
        public void GetIndex_Merged()
        {
            IFormCollection form = CreateForm(RawValues);
            string values = form[FormsItemKey];
            Assert.Equal(JoinedValues, values);
        }

        [Fact]
        public void Get_Merged()
        {
            IFormCollection form = CreateForm(RawValues);
            string values = form.Get(FormsItemKey);
            Assert.Equal(JoinedValues, values);
        }

        [Fact]
        public void GetValues_Unchanged()
        {
            IFormCollection form = CreateForm(RawValues);
            IList<string> values = form.GetValues(FormsItemKey);
            Assert.Equal(RawValues, values);
        }

        private IFormCollection CreateForm(string[] values)
        {
            IDictionary<string, string[]> formValues = new Dictionary<string, string[]>();
            formValues[FormsItemKey] = values;
            return new FormCollection(formValues);
        }

        [Fact]
        public void ReadFromStream()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(OriginalFormsString);
            writer.Flush();
            stream.Seek(0, SeekOrigin.Begin);
            IOwinRequest request = new OwinRequest();
            request.Body = stream;
            IFormCollection form = request.ReadFormAsync().Result;
            Assert.Equal("v1", form.Get("q1"));
            Assert.Equal("v2,b", form.Get("Q2"));
            Assert.Equal("v3,v4", form.Get("q3"));
            Assert.Null(form.Get("q4"));
            Assert.Equal("v5,v5", form.Get("Q5"));
            Assert.Equal("v 6", form.Get("Q 6"));
        }

        [Fact]
        public void ReadFromStreamTwice()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(OriginalFormsString);
            writer.Flush();
            stream.Seek(0, SeekOrigin.Begin);
            IOwinRequest request = new OwinRequest();
            request.Body = stream;
            IFormCollection form = request.ReadFormAsync().Result;
            Assert.Equal("v1", form.Get("q1"));
            Assert.Equal("v2,b", form.Get("Q2"));
            Assert.Equal("v3,v4", form.Get("q3"));
            Assert.Null(form.Get("q4"));
            Assert.Equal("v5,v5", form.Get("Q5"));
            Assert.Equal("v 6", form.Get("Q 6"));

            form = request.ReadFormAsync().Result;
            Assert.Equal("v1", form.Get("q1"));
            Assert.Equal("v2,b", form.Get("Q2"));
            Assert.Equal("v3,v4", form.Get("q3"));
            Assert.Null(form.Get("q4"));
            Assert.Equal("v5,v5", form.Get("Q5"));
            Assert.Equal("v 6", form.Get("Q 6"));
        }
    }
}
