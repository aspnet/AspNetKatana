//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Katana Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;

namespace Katana.Engine
{
    public interface IKatanaStarter
    {
        IDisposable Start(StartParameters parameters);
    }
}