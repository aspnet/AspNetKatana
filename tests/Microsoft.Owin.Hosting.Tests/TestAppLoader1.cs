// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Owin.Hosting.Loader;
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

        public Action<IAppBuilder> Load(string appName, IList<string> errors)
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
