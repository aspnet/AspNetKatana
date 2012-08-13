using System;
using System.IO;
using System.Threading.Tasks;
using Owin;

namespace Katana.Engine.Utils
{
    public static class  Encapsulate
    {
        public static AppDelegate Middleware(AppDelegate app, TextWriter output)
        {
            return parameters =>
            {
                object hostTraceOutput;
                if (!parameters.Environment.TryGetValue("host.TraceOutput", out hostTraceOutput) || hostTraceOutput == null)
                {
                    parameters.Environment["host.TraceOutput"] = output;
                }

                // If the host didn't provide a completion/cancelation token, substitute one and invoke it on error or completion.
                object callCompleted;
                if (!parameters.Environment.TryGetValue("owin.CallCompleted", out callCompleted) || callCompleted == null)
                {
                    TaskCompletionSource<object> completed = new TaskCompletionSource<object>();
                    parameters.Environment["owin.CallCompleted"] = completed.Task;

                    Action complete =
                        () =>
                        {
                            completed.TrySetResult(null);
                        };

                    // Wrap the body delegate to invoke completion on success or failure.
                    Func<ResultParameters, ResultParameters> wrapBody =
                        result =>
                        {
                            Func<Stream, Task> nestedBody = result.Body;
                            result.Body =
                                stream =>
                                {
                                    try
                                    {
                                        Task bodyTask = nestedBody(stream);
                                        if (bodyTask.IsCompleted)
                                        {
                                            // For errors let the Catch call complete.
                                            bodyTask.ThrowIfFaulted();
                                            if (bodyTask.IsCanceled)
                                            {
                                                throw new TaskCanceledException();
                                            }

                                            // Request & Body completed without errors.
                                            complete();
                                            return bodyTask;
                                        }

                                        return bodyTask.ContinueWith(
                                            bt =>
                                            {
                                                // Sucess or failure, the request is completed.
                                                complete();
                                                bt.ThrowIfFaulted();
                                                if (bt.IsCanceled)
                                                {
                                                    throw new TaskCanceledException();
                                                }
                                            });
                                    }
                                    catch (Exception)
                                    {
                                        complete();
                                        throw;
                                    }
                                };

                            // Return the updated task result struct.
                            return result;
                        };

                    try
                    {
                        Task<ResultParameters> syncAppTask = app(parameters);

                        if (syncAppTask.IsCompleted)
                        {
                            syncAppTask.ThrowIfFaulted();
                            if (syncAppTask.IsCanceled)
                            {
                                throw new TaskCanceledException();
                            }

                            ResultParameters result = syncAppTask.Result;
                            if (result.Body == null)
                            {
                                complete();
                                return syncAppTask;
                            }

                            result = wrapBody(result);
                            return TaskHelpers.FromResult(result);
                        }

                        return syncAppTask.ContinueWith<ResultParameters>(
                            (Task<ResultParameters> asyncAppTask) =>
                            {
                                if (asyncAppTask.IsFaulted || asyncAppTask.IsCanceled)
                                {
                                    complete();
                                    asyncAppTask.ThrowIfFaulted();
                                    throw new TaskCanceledException();
                                }

                                ResultParameters result = asyncAppTask.Result;
                                if (result.Body == null)
                                {
                                    complete();
                                    return result;
                                }

                                return wrapBody(result);
                            });
                    }
                    catch (Exception)
                    {
                        complete();
                        throw;
                    }
                }
                else
                {
                    return app(parameters);
                }
            };
        }
    }
}
