// <copyright file="CallConnected.cs" company="Katana contributors">
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

using System;

namespace Microsoft.AspNet.Owin.CallEnvironment
{
    /// <summary>
    /// Object for use in server.CallConnected key
    /// </summary>
    public class CallConnected
    {
        private readonly CallConnectedSource _source;

        public CallConnected(CallConnectedSource source)
        {
            _source = source;
        }

        public bool IsConnected
        {
            get { return _source.IsConnected; }
        }

        public void Register(Action continuation)
        {
            _source.Register(continuation);
        }
    }
}
