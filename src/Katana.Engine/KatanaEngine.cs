using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Owin;
using Katana.Engine.Settings;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Katana.Engine
{
    public class KatanaEngine : IKatanaEngine
    {
        private readonly IKatanaSettings _settings;

        public KatanaEngine(IKatanaSettings settings)
        {
            _settings = settings;
        }

        public IDisposable Start(StartInfo info)
        {
            ResolveOutput(info);
            ResolveServerFactory(info);
            ResolveApp(info);
            ResolveUrl(info);
            return StartServer(info);
        }

        void ResolveOutput(StartInfo info)
        {
            if (info.Output != null) return;

            if (!string.IsNullOrWhiteSpace(info.OutputFile))
            {
                info.Output = new StreamWriter(info.OutputFile, true);
            }
            else
            {
                info.Output = _settings.DefaultOutput;
            }
        }

        private void ResolveServerFactory(StartInfo info)
        {
            if (info.ServerFactory != null) return;

            var serverName = info.Server ?? _settings.DefaultServer;

            // TODO: error message for server assembly not found
            var serverAssembly = Assembly.Load(_settings.ServerAssemblyPrefix + serverName);

            // TODO: error message for assembly does not have ServerFactory attribute
            info.ServerFactory = serverAssembly.GetCustomAttributes(false)
                .Cast<Attribute>()
                .Single(x => x.GetType().Name == "ServerFactory");
        }

        private void ResolveApp(StartInfo info)
        {
            if (info.App == null)
            {
                var startup = _settings.Loader.Load(info.Startup);
                info.App = _settings.Builder.Build<AppDelegate>(startup);
            }
            info.App = Encapsulate((AppDelegate)info.App, info.Output);
        }

        public static AppDelegate Encapsulate(AppDelegate app, TextWriter output)
        {
            return parameters =>
            {
                object hostTraceOutput;
                if (!parameters.Environment.TryGetValue("host.TraceOutput", out hostTraceOutput) || hostTraceOutput == null)
                {
                    parameters.Environment["host.TraceOutput"] = output;
                }

                // If the host didn't provide a completion/cancelation token, substitute one and invoke it on error or completion.
                if (parameters.Completed == CancellationToken.None)
                {
                    CancellationTokenSource completion = new CancellationTokenSource();
                    parameters.Completed = completion.Token;

                    Action complete =
                        () =>
                        {
                            try
                            {
                                completion.Cancel();
                                // completion.Dispose();
                            }
                            catch (ObjectDisposedException)
                            {
                            }
                            catch (AggregateException)
                            {
                                // TODO: Trace exception to output
                            }
                        };

                    // Wrap the body delegate to invoke completion on success or failure.
                    Func<ResultParameters, ResultParameters> wrapBody =
                        result =>
                        {
                            BodyDelegate nestedBody = result.Body;
                            result.Body =
                                (stream, canceled) =>
                                {
                                    try
                                    {
                                        if (canceled == CancellationToken.None)
                                        {
                                            canceled = completion.Token;
                                        }

                                        Task bodyTask = nestedBody(stream, canceled);
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

        private void ResolveUrl(StartInfo info)
        {
            if (info.Url != null) return;
            info.Scheme = info.Scheme ?? _settings.DefaultScheme;
            info.Host = info.Host ?? _settings.DefaultHost;
            info.Port = info.Port ?? _settings.DefaultPort;
            info.Path = info.Path ?? "";
            if (info.Path != "" && !info.Path.StartsWith("/"))
            {
                info.Path = "/" + info.Path;
            }
            if (info.Port.HasValue)
            {
                info.Url = info.Scheme + "://" + info.Host + ":" + info.Port + info.Path + "/";
            }
            else
            {
                info.Url = info.Scheme + "://" + info.Host + info.Path + "/";
            }
        }

        private static IDisposable StartServer(StartInfo info)
        {
            // TODO: Katana#2: Need the ability to detect multiple Create methods, AppTaskDelegate and AppDelegate,
            // then choose the most appropriate one based on the next item in the pipeline.
            var serverFactoryMethod = info.ServerFactory.GetType().GetMethod("Create");
            var serverFactoryParameters = serverFactoryMethod.GetParameters()
                .Select(parameterInfo => SelectParameter(parameterInfo, info))
                .ToArray();
            return (IDisposable)serverFactoryMethod.Invoke(info.ServerFactory, serverFactoryParameters.ToArray());
        }


        private static object SelectParameter(ParameterInfo parameterInfo, StartInfo info)
        {
            switch (parameterInfo.Name)
            {
                case "url":
                    return info.Url;
                case "port":
                    return info.Port;
                case "app":
                    return info.App;
                case "host":
                    return info.Host;
                case "path":
                    return info.Path;
                case "output":
                    return info.Output;
            }
            return null;
        }
    }
}
