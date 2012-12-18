using System;
using Microsoft.Owin.Hosting.Services;
using Owin;

namespace Microsoft.Owin.Hosting.Tests
{
    public class TestAppLoader1 : IAppLoader
    {
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