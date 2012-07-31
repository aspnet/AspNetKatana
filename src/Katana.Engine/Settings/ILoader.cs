using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Owin;

namespace Katana.Engine.Settings
{
    public interface ILoader
    {
        Action<IAppBuilder> Load(string startupName);
    }
}
