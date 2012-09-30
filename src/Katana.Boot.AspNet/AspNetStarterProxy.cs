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
        StartParameters _parameters;
        IDisposable _running;

        public IDisposable StartKatana(StartParameters parameters)
        {
            _parameters = parameters;
            Start();
            return new Disposable(StopKatana);
        }

        void StopKatana()
        {
            var running = Interlocked.Exchange(ref _running, null);
            if (running != null)
            {
                running.Dispose();
            }
        }

        void Start()
        {
            var starter = (AspNetStarterAgent)ApplicationHost.CreateApplicationHost(
                typeof(AspNetStarterAgent),
                _parameters.Path ?? "/",
                Directory.GetCurrentDirectory());

            var running = starter.Start(this, _parameters);
            var prior = Interlocked.Exchange(ref _running, running);
            if (prior != null)
            {
                // TODO: UNEXPECTED
            }
        }

        public void Stop(bool immediate)
        {
            var running = Interlocked.Exchange(ref _running, null);
            if (running != null)
            {
                running.Dispose();

                // ASP.NET has indicated a Stop when the starter 
                // believes it is still running. After the the old 
                // agent is disposed, Start is called to re-create a 
                // replacement app domain.
                Start();
            }
        }
    }
}