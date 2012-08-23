using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Katana.Engine.Utils
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class Encapsulate
    {
        readonly AppFunc _app;
        readonly TextWriter _output;

        public Encapsulate(AppFunc app, TextWriter output)
        {
            _app = app;
            _output = output;
        }

        public Task Invoke(IDictionary<string, object> env)
        {
            object hostTraceOutput;
            if (!env.TryGetValue("host.TraceOutput", out hostTraceOutput) || hostTraceOutput == null)
            {
                env["host.TraceOutput"] = _output;
            }

            // If the host didn't provide a completion/cancelation token, substitute one and invoke it on error or completion.
            object callCompleted;
            if (!env.TryGetValue("owin.CallCompleted", out callCompleted) || callCompleted == null)
            {
                var completed = new TaskCompletionSource<object>();
                env["owin.CallCompleted"] = completed.Task;

                try
                {
                    return _app.Invoke(env)
                        .Catch(info =>
                        {
                            completed.TrySetException(info.Exception);
                            return info.Throw();
                        })
                        .Finally(() => completed.TrySetResult(null));
                }
                catch (Exception ex)
                {
                    completed.TrySetException(ex);
                    throw;
                }
            }

            return _app.Invoke(env);
        }
    }
}
