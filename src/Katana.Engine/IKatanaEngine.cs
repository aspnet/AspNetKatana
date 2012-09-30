using System;

namespace Katana.Engine
{
    public interface IKatanaEngine
    {
        IDisposable Start(StartContext context);
    }
}