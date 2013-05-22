// <copyright file="TestAppLoader1.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Collections.Generic;
using Microsoft.Owin.Hosting.Loader;
using Microsoft.Owin.Hosting.Services;
using Owin;

namespace Microsoft.Owin.Hosting.Tests
{
    public class TestAppLoader1 : IAppLoaderFactory
    {
        public static Action<IAppBuilder> Result = _ => { };

        public int Order
        {
            get { return 0; }
        }

        public Func<string, IList<string>, Action<IAppBuilder>> Create(Func<string, IList<string>, Action<IAppBuilder>> next)
        {
            return (appName, err) => Load(appName, err) ?? next(appName, err);
        }

        public Action<IAppBuilder> Load(string appName,  IList<string> errors)
        {
            if (appName == "Hello")
            {
                return Result;
            }
            errors.Add("Hello");
            return null;
        }
    }
}
