// <copyright file="RazorResponse.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2011-2013 Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Owin;

namespace Microsoft.AspNet.Razor.Owin.Execution
{
    public class RazorResponse : IRazorResponse
    {
        private OwinResponse _response;

        public RazorResponse(IDictionary<string, object> environment)
        {
            _response = new OwinResponse(environment);
        }

        public int StatusCode
        {
            get { return _response.StatusCode; }
            set { _response.StatusCode = value; }
        }

        public string ReasonPhrase
        {
            get { return _response.ReasonPhrase; }
            set { _response.ReasonPhrase = value; }
        }

        public Encoding Encoding { get; set; }

        public Stream Body
        {
            get { return _response.Body; }
            set { _response.Body = value; }
        }
    }
}
