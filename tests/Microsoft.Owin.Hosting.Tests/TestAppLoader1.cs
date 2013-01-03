// <copyright file="TestAppLoader1.cs" company="Katana contributors">
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
using Microsoft.Owin.Hosting.Loader;
using Owin;

namespace Microsoft.Owin.Hosting.Tests
{
    public class TestAppLoader1 : IAppLoaderProvider
    {
        public Func<string, Action<IAppBuilder>> CreateAppLoader(Func<string, Action<IAppBuilder>> next)
        {
            return appName => Load(appName) ?? next(appName);
        }

        public Action<IAppBuilder> Load(string appName)
        {
            if (appName == "Hello")
            {
                return Result;
            }
            return null;
        }

        public static Action<IAppBuilder> Result = _ => { };
    }
}
