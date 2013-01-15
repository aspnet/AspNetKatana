// -----------------------------------------------------------------------
// <copyright file="ITraceFactory.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gate;

namespace Microsoft.AspNet.Razor.Owin
{
    public interface ITraceFactory
    {
        ITrace ForRequest(Request req);
        ITrace ForApplication();
    }

    public interface ITrace
    {
        void WriteLine(string format, params object[] args);
        IDisposable StartTrace();
    }
}
