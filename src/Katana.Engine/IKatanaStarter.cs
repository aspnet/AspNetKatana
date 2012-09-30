using System;

namespace Katana.Engine
{
    public interface IKatanaStarter
    {
        IDisposable Start(StartParameters parameters);
    }
}