// <copyright file="WebApplication.cs" company="Katana contributors">
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
using System.Diagnostics.CodeAnalysis;

namespace Katana.Engine
{
    public static class WebApplication
    {
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Would require too many overloads")]
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "By design")]
        public static IDisposable Start<TStartup>(
            string url = null,
            string server = null,
            string scheme = null,
            string host = null,
            int? port = null,
            string path = null,
            string boot = null,
            string outputFile = null,
            int verbosity = 0)
        {
            return Start(
                new StartParameters
                {
                    Boot = boot,
                    Server = server,
                    App = typeof(TStartup).AssemblyQualifiedName,
                    OutputFile = outputFile,
                    Verbosity = verbosity,
                    Url = url,
                    Scheme = scheme,
                    Host = host,
                    Port = port,
                    Path = path,
                });
        }

        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Would require too many overloads")]
        public static IDisposable Start(
            string app = null,
            string url = null,
            string server = null,
            string scheme = null,
            string host = null,
            int? port = null,
            string path = null,
            string boot = null,
            string outputFile = null,
            int verbosity = 0)
        {
            return Start(
                new StartParameters
                {
                    Boot = boot,
                    Server = server,
                    App = app,
                    OutputFile = outputFile,
                    Verbosity = verbosity,
                    Url = url,
                    Scheme = scheme,
                    Host = host,
                    Port = port,
                    Path = path,
                });
        }

        public static IDisposable Start(StartParameters parameters)
        {
            return new KatanaStarter().Start(parameters);
        }
    }
}
