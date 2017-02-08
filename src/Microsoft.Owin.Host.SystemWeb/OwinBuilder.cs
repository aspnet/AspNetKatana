// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.Owin.Host.SystemWeb.Infrastructure;
using Owin;
using Owin.Loader;

namespace Microsoft.Owin.Host.SystemWeb
{
    internal static class OwinBuilder
    {
        internal static bool IsAutomaticAppStartupEnabled
        {
            get
            {
                string autoAppStartup = ConfigurationManager.AppSettings[Constants.OwinAutomaticAppStartup];
                return string.IsNullOrWhiteSpace(autoAppStartup)
                    || string.Equals("true", autoAppStartup, StringComparison.OrdinalIgnoreCase);
            }
        }

        internal static Action<IAppBuilder> GetAppStartup()
        {
            string appStartup = ConfigurationManager.AppSettings[Constants.OwinAppStartup];

            var loader = new DefaultLoader(new ReferencedAssembliesWrapper());
            IList<string> errors = new List<string>();
            Action<IAppBuilder> startup = loader.Load(appStartup ?? string.Empty, errors);

            if (startup == null)
            {
                throw new EntryPointNotFoundException(Resources.Exception_AppLoderFailure
                    + Environment.NewLine + " - " + string.Join(Environment.NewLine + " - ", errors)
                    + (IsAutomaticAppStartupEnabled ? Environment.NewLine + Resources.Exception_HowToDisableAutoAppStartup : string.Empty)
                    + Environment.NewLine + Resources.Exception_HowToSpecifyAppStartup);
            }
            return startup;
        }

        internal static OwinAppContext Build()
        {
            Action<IAppBuilder> startup = GetAppStartup();
            return Build(startup);
        }

        internal static OwinAppContext Build(Func<IDictionary<string, object>, Task> app)
        {
            return Build(builder => builder.Use(new Func<object, object>(_ => app)));
        }

        internal static OwinAppContext Build(Action<IAppBuilder> startup)
        {
            if (startup == null)
            {
                throw new ArgumentNullException("startup");
            }

            var appContext = new OwinAppContext();
            appContext.Initialize(startup);
            return appContext;
        }
    }
}
