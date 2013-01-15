// -----------------------------------------------------------------------
// <copyright file="NullTraceFactory.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Razor.Owin
{
    public class NullTraceFactory : ITraceFactory
    {
        public static readonly NullTraceFactory Instance = new NullTraceFactory();

        private NullTraceFactory()
        {
        }

        public ITrace ForRequest(Gate.Request req)
        {
            return NullTrace.Instance;
        }

        public ITrace ForApplication()
        {
            return NullTrace.Instance;
        }
    }
}
