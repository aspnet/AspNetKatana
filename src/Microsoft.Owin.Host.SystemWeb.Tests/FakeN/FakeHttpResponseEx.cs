// <copyright file="FakeHttpResponseEx.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
// </copyright>
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

using System.IO;
using FakeN.Web;

namespace Microsoft.Owin.Host.SystemWeb.Tests.FakeN
{
    public class FakeHttpResponseEx : FakeHttpResponse
    {
        private readonly Stream _outputStream = Stream.Null;
        private int _status;

        public override int StatusCode
        {
            get { return _status; }
            set { _status = value; }
        }

        public override Stream OutputStream
        {
            get { return _outputStream; }
        }
    }
}
