// <copyright file="GateTraceFactory.cs" company="Microsoft Open Technologies, Inc.">
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

using System;
using System.Threading;
using Gate;

namespace Microsoft.AspNet.Razor.Owin
{
    public class GateTraceFactory : ITraceFactory
    {
        private long _nextId = 0;

        public ITrace ForRequest(Request req)
        {
            // Just loop around if we reach the end of the request id space.
            Interlocked.CompareExchange(ref _nextId, 0, Int64.MaxValue);

            return new GateTrace(req, Interlocked.Increment(ref _nextId));
        }

        public ITrace ForApplication()
        {
            return GateTrace.Global;
        }

        internal void SetCurrentId(long id)
        {
            _nextId = id;
        }
    }
}
