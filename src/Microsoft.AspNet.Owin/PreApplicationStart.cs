//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Katana Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using Microsoft.AspNet.Owin;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;

[assembly: PreApplicationStartMethod(typeof(PreApplicationStart), "Initialize")]

namespace Microsoft.AspNet.Owin
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
