using System;
using Katana.Boot.AspNet;
using Katana.Engine;

[assembly: AspNetStarter]

namespace Katana.Boot.AspNet
{
    public class AspNetStarter : Attribute, IKatanaStarter
    {
        public IDisposable Start(StartParameters parameters)
        {
            return new AspNetStarterProxy().StartKatana(parameters);
        }
    }
}