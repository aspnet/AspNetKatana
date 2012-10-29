// <copyright file="KatanaSettings.cs" company="Katana contributors">
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
using System.IO;
using Owin;
using Owin.Builder;
using Owin.Loader;

namespace Katana.Engine.Settings
{
    public class KatanaSettings : IKatanaSettings
    {
        public KatanaSettings()
        {
            DefaultServer = "Microsoft.HttpListener.Owin";

            DefaultScheme = "http";
            DefaultHost = "+";
            DefaultPort = 8080;

            DefaultOutput = Console.Error;

            LoaderFactory = () => new DefaultLoader().Load;
            BuilderFactory = () => new AppBuilder();
        }

        public string DefaultServer { get; set; }

        public string DefaultScheme { get; set; }
        public string DefaultHost { get; set; }
        public int? DefaultPort { get; set; }

        public TextWriter DefaultOutput { get; set; }

        public string ServerAssemblyPrefix { get; set; }

        public Func<Func<string, Action<IAppBuilder>>> LoaderFactory { get; set; }
        public Func<IAppBuilder> BuilderFactory { get; set; }
    }
}
