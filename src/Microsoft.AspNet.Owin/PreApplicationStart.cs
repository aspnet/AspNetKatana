// <copyright file="PreApplicationStart.cs" company="Katana contributors">
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
using System.Configuration;
using System.IO;
using System.Web;
using System.Web.Hosting;
using Microsoft.Owin.Host.SystemWeb;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;

[assembly: PreApplicationStartMethod(typeof(PreApplicationStart), "Initialize")]

namespace Microsoft.Owin.Host.SystemWeb
{
    public static class PreApplicationStart
    {
        public static void Initialize()
        {
            try
            {
                DynamicModuleUtility.RegisterModule(typeof(OwinHttpModule));

                var appSetting = ConfigurationManager.AppSettings["owin:SetCurrentDirectory"];
                if (string.Equals("True", appSetting, StringComparison.InvariantCultureIgnoreCase))
                {
                    var physicalPath = HostingEnvironment.MapPath("~");
                    if (physicalPath != null)
                    {
                        Directory.SetCurrentDirectory(physicalPath);
                    }
                }
            }
                // ReSharper disable EmptyGeneralCatchClause
            catch
            {
            }
            // ReSharper restore EmptyGeneralCatchClause
        }
    }
}
