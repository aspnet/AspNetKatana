//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Katana Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.IO;
using System.Threading;
using System.Web.Hosting;
using Katana.Engine;
using Katana.Engine.Utils;

namespace Katana.Boot.AspNet
{
    public class AspNetStarterProxy : MarshalByRefObject
    {
        private StartParameters _parameters;
        private IDisposable _running;

        public IDisposable StartKatana(StartParameters parameters)
        {
            this._parameters = parameters;
            this.Start();
            return new Disposable(this.StopKatana);
        }

        private void StopKatana()
        {
            var running = Interlocked.Exchange(ref this._running, null);
            if (running != null)
            {
                running.Dispose();
            }
        }

        private void Start()
        {
            var starter = (AspNetStarterAgent)ApplicationHost.CreateApplicationHost(
                typeof(AspNetStarterAgent),
                this._parameters.Path ?? "/",
                Directory.GetCurrentDirectory());

            var running = starter.Start(this, this._parameters);
            var prior = Interlocked.Exchange(ref this._running, running);
            if (prior != null)
            {
                // TODO: UNEXPECTED
            }
        }

        public void Stop(bool immediate)
        {
            var running = Interlocked.Exchange(ref this._running, null);
            if (running != null)
            {
                running.Dispose();

                // ASP.NET has indicated a Stop when the starter 
                // believes it is still running. After the the old 
                // agent is disposed, Start is called to re-create a 
                // replacement app domain.
                this.Start();
            }
        }
    }
}