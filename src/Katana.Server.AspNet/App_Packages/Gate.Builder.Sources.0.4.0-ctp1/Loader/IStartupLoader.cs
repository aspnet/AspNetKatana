using System;
using Owin;

namespace Gate.Builder.Loader
{
    public interface IStartupLoader
    {
        Action<IAppBuilder> Load(string startupName);
    }
}