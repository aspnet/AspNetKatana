// <copyright file="PreApplicationStart.cs" company="Microsoft Open Technologies, Inc.">
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
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Web;
using System.Web.Hosting;
using Microsoft.Owin.Host.SystemWeb;
using Microsoft.Owin.Host.SystemWeb.Infrastructure;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;

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
                DynamicModuleUtility.RegisterModule(typeof(OwinHttpModule));
            }
            catch (Exception ex)
            {
                ITrace trace = TraceFactory.Create(TraceName);
                trace.WriteError(Resources.Trace_RegisterModuleException, ex);
                throw;
            }

            try
            {
                string appSetting = ConfigurationManager.AppSettings[Constants.OwinSetCurrentDirectory];
                if (string.Equals("true", appSetting, StringComparison.OrdinalIgnoreCase))
                {
                    string physicalPath = HostingEnvironment.MapPath("~");
                    if (physicalPath != null)
                    {
                        Directory.SetCurrentDirectory(physicalPath);
                    }
                }
            }
            catch (Exception ex)
            {
                ITrace trace = TraceFactory.Create(TraceName);
                trace.WriteError(Resources.Trace_SetCurrentDirectoryException, ex);
                throw;
            }
        }
    }
}