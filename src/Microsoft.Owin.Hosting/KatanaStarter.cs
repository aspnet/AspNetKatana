// <copyright file="KatanaStarter.cs" company="Katana contributors">
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Owin.Hosting.Services;
using Microsoft.Owin.Hosting.Settings;
using Microsoft.Owin.Hosting.Starter;

namespace Microsoft.Owin.Hosting
{
    public class KatanaStarter : IKatanaStarter
    {
        public IDisposable Start(StartParameters parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }

            return String.IsNullOrWhiteSpace(parameters.Boot)
                ? DirectStart(parameters)
                : IndirectStart(parameters);
        }

        private static IDisposable IndirectStart(StartParameters parameters)
        {
            IKatanaStarter starter = BuildStarter(parameters.Boot);
            parameters.Boot = null;
            return starter.Start(parameters);
        }

        private static Assembly LoadProvider(params string[] names)
        {
            var innerExceptions = new List<Exception>();
            foreach (var name in names)
            {
                try
                {
                    return Assembly.Load(name);
                }
                catch (FileNotFoundException ex)
                {
                    innerExceptions.Add(ex);
                }
                catch (FileLoadException ex)
                {
                    innerExceptions.Add(ex);
                }
                catch (BadImageFormatException ex)
                {
                    innerExceptions.Add(ex);
                }
            }
            throw new AggregateException(innerExceptions);
        }

        private static IKatanaStarter BuildStarter(string boot)
        {
            if (boot == "Domain")
            {
                return new DomainStarterProxy();
            }
            return LoadProvider("Katana.Boot." + boot, boot)
                .GetCustomAttributes(inherit: false)
                .OfType<IKatanaStarter>()
                .SingleOrDefault();
        }

        private static IDisposable DirectStart(StartParameters parameters)
        {
            IKatanaEngine engine = BuildEngine();

            return engine.Start(new StartContext { Parameters = parameters });
        }

        private static IKatanaEngine BuildEngine()
        {
            return DefaultServices.Create().GetService<IKatanaEngine>();
        }
    }
}
