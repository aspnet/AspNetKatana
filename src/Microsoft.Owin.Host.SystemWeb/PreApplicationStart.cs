// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Web;
using Microsoft.Owin.Host.SystemWeb;
using Microsoft.Owin.Host.SystemWeb.Infrastructure;

[assembly: PreApplicationStartMethod(typeof(PreApplicationStart), "Initialize")]

namespace Microsoft.Owin.Host.SystemWeb
{
    /// <summary>
    /// Registers the OWIN request processing module at application startup.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class PreApplicationStart
    {
        private const string TraceName = "Microsoft.Owin.Host.SystemWeb.PreApplicationStart";

        /// <summary>
        /// Registers the OWIN request processing module.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "Initialize must never throw on server startup path")]
        public static void Initialize()
        {
            try
            {
                if (OwinBuilder.IsAutomaticAppStartupEnabled)
                {
                    HttpApplication.RegisterModule(typeof(OwinHttpModule));
                }
            }
            catch (Exception ex)
            {
                ITrace trace = TraceFactory.Create(TraceName);
                trace.WriteError(Resources.Trace_RegisterModuleException, ex);
                throw;
            }
        }
    }
}
