// <copyright file="ResponseWriter.cs" company="Microsoft Open Technologies, Inc.">
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

using System.IO;
using System.Text;
using Microsoft.AspNet.Razor.Owin.Execution;

namespace Microsoft.AspNet.Razor.Owin
{
    public class ResponseWriter : TextWriter
    {
        public ResponseWriter(IRazorResponse response)
        {
            Response = response;
        }

        public IRazorResponse Response { get; private set; }

        public override void Write(string value)
        {
            base.Write(value);
        }

        public override void Write(char[] buffer, int index, int count)
        {
            byte[] bytes = Encoding.GetBytes(buffer, index, count);
            Response.Body.Write(bytes, 0, bytes.Length);
        }

        public override Encoding Encoding
        {
            get { return Response.Encoding; }
        }

    }
}
