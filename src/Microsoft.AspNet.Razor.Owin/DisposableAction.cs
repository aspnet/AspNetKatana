// -----------------------------------------------------------------------
// <copyright file="DisposableAction.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNet.Razor.Owin;

namespace Microsoft.AspNet.Razor.Owin
{
    internal class DisposableAction : IDisposable
    {
        private Action _act;

        public DisposableAction(Action act)
        {
            Requires.NotNull(act, "act");

            _act = act;
        }

        public void Dispose()
        {
            _act();
        }
    }
}
