// <copyright file="RoslynAppLoader.cs" company="Microsoft Open Technologies, Inc.">
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
using System.IO;
using Microsoft.Owin.Hosting.Loader;
using Owin;
using Roslyn.Scripting;
using Roslyn.Scripting.CSharp;

namespace Katana.Loader.Roslyn
{
    public class RoslynAppLoaderFactory : IAppLoaderFactory
    {
        public int Order
        {
            get { return -50; }
        }

        public Func<string, Action<IAppBuilder>> Create(Func<string, Action<IAppBuilder>> nextLoader)
        {
            return name =>
            {
                string extension = Path.GetExtension(name);

                if (string.Equals(".csx", extension, StringComparison.OrdinalIgnoreCase) && File.Exists(name))
                {
                    return app =>
                    {
                        var engine = new ScriptEngine();
                        Session session = engine.CreateSession(new HostObject(app));
                        session.AddReference(typeof(IAppBuilder).Assembly);
                        session.AddReference(typeof(HostObject).Assembly);
                        session.ExecuteFile(name);
                    };
                }
                return nextLoader(name);
            };
        }
    }
}
