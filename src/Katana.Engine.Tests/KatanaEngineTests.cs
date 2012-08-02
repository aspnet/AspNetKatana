using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Owin;
using Xunit;
using Shouldly;

namespace Katana.Engine.Tests
{
    public class KatanaEngineTests
    {
        TextWriter Output { get; set; }
        CallParameters CallParams { get; set; }

        public KatanaEngineTests()
        {
            Output = new StringWriter();
        }

        private CallParameters CreateEmptyRequest()
        {
            return new CallParameters()
            {
                Environment = new Dictionary<string, object>(),
                Headers = new Dictionary<string, string[]>(),
                Body = null,
            };
        }

        private static Task GetCallCompletion(CallParameters call)
        {
            return (Task)call.Environment["owin.CallCompleted"];
        }

        [Fact]
        public void TextWriterAddedIfNotPresentInEnvironment()
        {
            object actualOutput = null;
            var encapsulateOutput = new StringWriter();

            var app = KatanaEngine.Encapsulate(
                call => 
                { 
                    actualOutput = call.Environment["host.TraceOutput"]; 
                    return new TaskCompletionSource<ResultParameters>().Task; 
                },
                encapsulateOutput);

            app(CreateEmptyRequest());
            actualOutput.ShouldBeSameAs(encapsulateOutput);
        }

        [Fact]
        public void TextWriterNotChangedIfPresent()
        {
            object actualOutput = null;
            var encapsulateOutput = new StringWriter();
            var environmentOutput = new StringWriter();

            var app = KatanaEngine.Encapsulate(
                call =>
                {
                    actualOutput = call.Environment["host.TraceOutput"];
                    return new TaskCompletionSource<ResultParameters>().Task;
                },
                encapsulateOutput);

            CallParameters callParams = CreateEmptyRequest();
            callParams.Environment["host.TraceOutput"] = environmentOutput;

            app(callParams);
            actualOutput.ShouldBeSameAs(environmentOutput);
            actualOutput.ShouldNotBeSameAs(encapsulateOutput);
        }

        [Fact]
        public void CallCompletedNotChangedIfPresent()
        {
            var callCompleted = false;

            var app = KatanaEngine.Encapsulate(
                call => 
                {
                    GetCallCompletion(call).Finally(() => callCompleted = true, true);
                    return new TaskCompletionSource<ResultParameters>().Task;
                },
                Output);

            var tcs = new TaskCompletionSource<object>();
            CallParameters parameters = CreateEmptyRequest();
            parameters.Environment["owin.CallCompleted"] = tcs.Task;

            app(parameters);
            callCompleted.ShouldBe(false);
            tcs.TrySetResult(null);
            callCompleted.ShouldBe(true);
        }

        [Fact]
        public void CallCompletedProvidedIfMissing()
        {
            var callCompleted = false;

            var app = KatanaEngine.Encapsulate(
                call =>
                {
                    GetCallCompletion(call).Finally(() => callCompleted = true, true);
                    return new TaskCompletionSource<ResultParameters>().Task;
                },
                Output);

            app(CreateEmptyRequest());

            callCompleted.ShouldBe(false);
        }

        [Fact]
        public void AsyncFaultWillTriggerTheProvidedToken()
        {
            var callCompleted = false;
            TaskCompletionSource<ResultParameters> tcs = new TaskCompletionSource<ResultParameters>();

            var app = KatanaEngine.Encapsulate(
                call =>
                {
                    GetCallCompletion(call).Finally(() => callCompleted = true, true);
                    return tcs.Task;
                },
                Output);
            
            Task appTask = app(CreateEmptyRequest());

            callCompleted.ShouldBe(false); // disposed before exception
            appTask.IsCompleted.ShouldBe(false); //Completed before exception.

            tcs.TrySetException(new Exception("Simulating Async Exception"));
            try
            {
                appTask.Wait();
            }
            catch (AggregateException)
            {
            }
            appTask.IsCompleted.ShouldBe(true); // Completed after exception.
            callCompleted.ShouldBe(true); // disposed after exception.
        }

        [Fact]
        public void SyncFaultWillTriggerTheProvidedToken()
        {
            var callCompleted = false;

            var app = KatanaEngine.Encapsulate(
                call =>
                {
                    GetCallCompletion(call).Finally(() => callCompleted = true, true);
                    throw new ApplicationException("Boom");
                },
                Output);

            Exception caught = null;
            try
            {
                app(CreateEmptyRequest());
            }
            catch (Exception ex)
            {
                caught = ex;
            }
            callCompleted.ShouldBe(true);
            caught.ShouldNotBe(null);
            caught.Message.ShouldBe("Boom");
        }

        [Fact]
        public void ResponseBodyEndWillTriggerTheProvidedToken()
        {
            var callCompleted = false;
            TaskCompletionSource<ResultParameters> tcs = new TaskCompletionSource<ResultParameters>();

            var app = KatanaEngine.Encapsulate(
                call =>
                {
                    GetCallCompletion(call).Finally(() => callCompleted = true, true);
                    return tcs.Task;
                },
                Output);
            
            Task<ResultParameters> appTask = app(CreateEmptyRequest());

            callCompleted.ShouldBe(false);
            appTask.IsCompleted.ShouldBe(false);

            Func<Stream, Task> bodyDelegate =
                stream =>
                {
                    TaskCompletionSource<object> completed = new TaskCompletionSource<object>();
                    completed.TrySetResult(null);
                    return completed.Task;
                };                    

            ResultParameters createdResult = new ResultParameters()
            {
                Status = 200,
                Body = bodyDelegate,
                Headers = new Dictionary<string, string[]>(),
                Properties = new Dictionary<string, object>()
            };

            tcs.TrySetResult(createdResult);

            callCompleted.ShouldBe(false);

            appTask.Wait(1000).ShouldBe(true);
            appTask.IsFaulted.ShouldBe(false);
            appTask.IsCanceled.ShouldBe(false);

            ResultParameters returnedResult = appTask.Result;

            returnedResult.Body.ShouldNotBe(null);
            returnedResult.Body.ShouldNotBeSameAs(bodyDelegate);

            Task bodyTask = returnedResult.Body(null);
            
            bodyTask.Wait(1000).ShouldBe(true);
            bodyTask.IsCompleted.ShouldBe(true);
            bodyTask.IsFaulted.ShouldBe(false);
            bodyTask.IsCanceled.ShouldBe(false);
            callCompleted.ShouldBe(true);
        }
    }
}
