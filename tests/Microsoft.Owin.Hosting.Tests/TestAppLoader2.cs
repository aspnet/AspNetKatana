using System;
using Microsoft.Owin.Hosting.Loader;
using Owin;

namespace Microsoft.Owin.Hosting.Tests
{
    public class TestAppLoader2 : IAppLoaderProvider
    {
        public Func<string, Action<IAppBuilder>> GetAppLoader()
        {
            return Load;
        }

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