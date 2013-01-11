// -----------------------------------------------------------------------
// <copyright file="NullTraceFactory.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.AspNet.Razor.Owin
{
    public class NullTrace : ITrace
    {
        public static readonly NullTrace Instance = new NullTrace();

        private NullTrace()
        {
        }

        public void WriteLine(string format, params object[] args)
        {
        }

        public IDisposable StartTrace()
        {
            return new DisposableAction(() => { });
        }
    }
}
