// -----------------------------------------------------------------------
// <copyright file="GateTraceFactory.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
