using System;
using Owin;

namespace Katana.Engine.Settings
{
    public interface ILoader
    {
        Action<IAppBuilder> Load(string startupName);
    }
}
