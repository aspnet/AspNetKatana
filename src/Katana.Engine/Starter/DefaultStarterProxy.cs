//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Katana Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.IO;

namespace Katana.Engine.Starter
{
    public class DefaultStarterProxy : IKatanaStarter
    {
        public IDisposable Start(StartParameters parameters)
        {
            var directory = Directory.GetCurrentDirectory();
            var info = new AppDomainSetup
                       {
                           ApplicationBase = directory,
                           PrivateBinPath = "bin",
                           PrivateBinPathProbe = "*",
                           ConfigurationFile = Path.Combine(directory, "web.config")
                       };

            var domain = AppDomain.CreateDomain("OWIN", null, info);

            var agent = (DefaultStarterAgent)domain.CreateInstanceFromAndUnwrap(
                typeof(DefaultStarterAgent).Assembly.Location,
                typeof(DefaultStarterAgent).FullName);

            agent.ResolveAssembliesFromDirectory(AppDomain.CurrentDomain.SetupInformation.ApplicationBase);
            
            return agent.Start(parameters);
        }
    }
}