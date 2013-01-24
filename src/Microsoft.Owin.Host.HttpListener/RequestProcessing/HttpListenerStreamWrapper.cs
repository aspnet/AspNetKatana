// <copyright file="HttpListenerStreamWrapper.cs" company="Katana contributors">
//   Copyright 2011-2013 Katana contributors
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

using System;
using System.IO;
using System.Net;

namespace Microsoft.Owin.Host.HttpListener.RequestProcessing
{
    internal class HttpListenerStreamWrapper : ExceptionFilterStream
    {
        internal HttpListenerStreamWrapper(Stream innerStream)
            : base(innerStream)
        {
        }

        // Convert HttpListenerExceptions to IOExceptions
        protected override bool TryWrapException(Exception ex, out Exception wrapped)
        {
            if (ex is HttpListenerException)
            {
                wrapped = new IOException(string.Empty, ex);
                return true;
            }

            wrapped = null;
            return false;
        }
    }
}
