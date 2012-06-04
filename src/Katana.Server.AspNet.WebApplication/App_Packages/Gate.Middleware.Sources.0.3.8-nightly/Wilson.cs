using System;
using System.Linq;
using System.Threading;
using Owin;
using Timer = System.Timers.Timer;

namespace Gate.Middleware
{
    internal class Wilson
    {
        public static AppDelegate App(bool asyncReply)
        {
            return asyncReply ? AsyncApp() : App();
        }

        public static AppDelegate App()
        {
            return (env, result, fault) =>
            {
                var request = new Request(env);
                var response = new Response(result) { Buffer = true, ContentType = "text/html" };
                var wilson = "left - right\r\n123456789012\r\nhello world!\r\n";

                var href = "?flip=left";
                if (request.Query["flip"] == "left")
                {
                    wilson = wilson.Split(new[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(line => new string(line.Reverse().ToArray()))
                        .Aggregate("", (agg, line) => agg + line + System.Environment.NewLine);
                    href = "?flip=right";
                }
                response.Write("<title>Wilson</title>");
                response.Write("<pre>");
                response.Write(wilson);
                response.Write("</pre>");
                if (request.Query["flip"] == "crash")
                {
                    throw new ApplicationException("Wilson crashed!");
                }
                response.Write("<p><a href='" + href + "'>flip!</a></p>");
                response.Write("<p><a href='?flip=crash'>crash!</a></p>");
                response.End();
            };
        }

        public static AppDelegate AsyncApp()
        {
            return (env, result, fault) =>
            {
                var request = new Request(env);
                var response = new Response(result)
                {
                    ContentType = "text/html",
                };
                var wilson = "left - right\r\n123456789012\r\nhello world!\r\n";

                ThreadPool.QueueUserWorkItem(_ =>
                {
                    try
                    {
                        var href = "?flip=left";
                        if (request.Query["flip"] == "left")
                        {
                            wilson = wilson.Split(new[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(line => new string(line.Reverse().ToArray()))
                                .Aggregate("", (agg, line) => agg + line + System.Environment.NewLine);
                            href = "?flip=right";
                        }

                        response.Start(() => TimerLoop(350, response.Error,
                            () => response.Write("<title>Hutchtastic</title>"),
                            () => response.Write("<pre>"),
                            () => response.Write(wilson),
                            () => response.Write("</pre>"),
                            () =>
                            {
                                if (request.Query["flip"] == "crash")
                                {
                                    throw new ApplicationException("Wilson crashed!");
                                }
                            },
                            () => response.Write("<p><a href='" + href + "'>flip!</a></p>"),
                            () => response.Write("<p><a href='?flip=crash'>crash!</a></p>"),
                            response.End));
                    }
                    catch (Exception ex)
                    {
                        fault(ex);
                    }
                });
            };
        }

        static void TimerLoop(double interval, Action<Exception> fault, params Action[] steps)
        {
            var iter = steps.AsEnumerable().GetEnumerator();
            var timer = new Timer(interval);
            timer.Elapsed += (sender, e) =>
            {
                if (iter != null && iter.MoveNext())
                {
                    try
                    {
                        iter.Current();
                    }
                    catch (Exception ex)
                    {
                        iter = null;
                        timer.Stop();
                        fault(ex);
                    }
                }
                else
                {
                    timer.Stop();
                }
            };
            timer.Start();
        }
    }
}