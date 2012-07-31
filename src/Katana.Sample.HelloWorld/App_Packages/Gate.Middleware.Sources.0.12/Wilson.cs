using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
            return call =>
            {
                var request = new Request(call);
                var response = new Response { ContentType = "text/html" };
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

                return response.EndAsync();
            };
        }

        public static AppDelegate AsyncApp()
        {
            return call =>
            {
                var request = new Request(call);
                var response = new Response()
                {
                    ContentType = "text/html",
                };
                var wilson = "left - right\r\n123456789012\r\nhello world!\r\n";

                response.StartAsync().Then(
                    resp1 =>
                    {
                        var href = "?flip=left";
                        if (request.Query["flip"] == "left")
                        {
                            wilson = wilson.Split(new[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(line => new string(line.Reverse().ToArray()))
                                .Aggregate("", (agg, line) => agg + line + System.Environment.NewLine);
                            href = "?flip=right";
                        }

                        return TimerLoop(350,
                            () => resp1.Write("<title>Hutchtastic</title>"),
                            () => resp1.Write("<pre>"),
                            () => resp1.Write(wilson),
                            () => resp1.Write("</pre>"),
                            () =>
                            {
                                if (request.Query["flip"] == "crash")
                                {
                                    throw new ApplicationException("Wilson crashed!");
                                }
                            },
                            () => resp1.Write("<p><a href='" + href + "'>flip!</a></p>"),
                            () => resp1.Write("<p><a href='?flip=crash'>crash!</a></p>"),
                            () => resp1.End());
                    })
                    .Catch(errorInfo =>
                    {
                        response.Error(errorInfo.Exception);
                        return errorInfo.Handled();
                    });

                return response.ResultTask;
            };
        }

        static Task TimerLoop(double interval, params Action[] steps)
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
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
                        tcs.TrySetException(ex);
                    }
                }
                else
                {
                    tcs.TrySetResult(null);
                    timer.Stop();
                }
            };
            timer.Start();
            return tcs.Task;
        }
    }
}