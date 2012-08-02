using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Owin;

namespace Gate.Builder
{
    using AppAction = Func< // Call
        IDictionary<string, object>, // Environment
        IDictionary<string, string[]>, // Headers
        Stream, // Body
        Task<Tuple< //Result
            IDictionary<string, object>, // Properties
            int, // Status
            IDictionary<string, string[]>, // Headers
            Func< // CopyTo
                Stream, // Body
                Task>>>>; // Done

    internal static class Adapters
    {
        public static AppAction ToFunc(AppDelegate app)
        {
            return (env, headers, body) =>
            {
                var task = app(new CallParameters
                {
                    Environment = env,
                    Headers = headers,
                    Body = body
                });

                return task.Then(result => Tuple.Create(
                    result.Properties,
                    result.Status,
                    result.Headers,
                    result.Body));
            };
        }


        public static AppDelegate ToDelegate(AppAction app)
        {
            return call =>
            {
                var task = app(
                    call.Environment,
                    call.Headers,
                    call.Body);

                return task.Then(result => new ResultParameters
                {
                    Properties = result.Item1,
                    Status = result.Item2,
                    Headers = result.Item3,
                    Body = result.Item4
                });
            };
        }

    }
}
