using System;
using Microsoft.Owin.Hosting.Services;
using Owin;

namespace Microsoft.Owin.Hosting.Tests
{
    public class TestAppLoader2 : IAppLoader
    {
        public Action<IAppBuilder> Load(string appName)
        {
            if (appName == "World")
            {
                return Result;
            }
            return null;
        }

        public static Action<IAppBuilder> Result = _ => { };
    }
}