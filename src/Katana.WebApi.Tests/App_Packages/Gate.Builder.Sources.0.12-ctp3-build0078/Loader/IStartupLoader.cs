using System;
using Owin;

namespace Gate.Builder.Loader
{
    internal interface IStartupLoader
    {
        Action<IAppBuilder> Load(string startupName);
    }
}