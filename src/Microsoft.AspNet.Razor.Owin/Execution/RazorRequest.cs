// <copyright file="RazorRequest.cs" company="Microsoft Open Technologies, Inc.">
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
using Microsoft.Owin;

namespace Microsoft.AspNet.Razor.Owin.Execution
{
    public class RazorRequest : IRazorRequest
    {
        private readonly OwinRequest _request;

        public RazorRequest(IDictionary<string, object> environment)
        {
            _request = new OwinRequest(environment);
        }

        public IDictionary<string, object> Environment
        {
            get { return _request.Environment; }
        }

        public string Method
        {
            get { return _request.Method; }
            set { _request.Method = value; }
        }

        public string Path
        {
            get { return _request.Path.Value; }
        }

        public TextWriter TraceOutput
        {
            get { return _request.Get<TextWriter>("host.TraceOutput"); }
        }
    }
}
