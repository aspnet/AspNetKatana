using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Owin;

namespace Katana.Engine.Tests
{
    public class KatanaEngineTests
    {
        TextWriter Output { get; set; }
        CallParameters CallParams { get; set; }

        [SetUp]
        public void Init()
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

        [Test]
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
            Assert.That(actualOutput, Is.SameAs(encapsulateOutput));
        }

        [Test]
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
            Assert.That(actualOutput, Is.SameAs(environmentOutput));
            Assert.That(actualOutput, Is.Not.SameAs(encapsulateOutput));
        }
        
        [Test]
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
            Assert.That(callCompleted, Is.False);
            tcs.TrySetResult(null);
            Assert.That(callCompleted, Is.True);
        }
         
        [Test]
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

            Assert.That(callCompleted, Is.False);
        }
        
        [Test]
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

            Assert.False(callCompleted, "disposed before exception.");
            Assert.False(appTask.IsCompleted, "Completed before exception.");

            tcs.TrySetException(new Exception("Simulating Async Exception"));
            try
            {
                appTask.Wait();
            }
            catch (AggregateException)
            {
            }
            Assert.True(appTask.IsCompleted, "Completed after exception.");
            Assert.True(callCompleted, "disposed after exception.");
        }

        [Test]
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
            Assert.That(callCompleted, Is.True);
            Assert.That(caught, Is.Not.Null);
            Assert.That(caught.Message, Is.EqualTo("Boom"));
        }

        [Test]
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

            Assert.False(callCompleted);
            Assert.False(appTask.IsCompleted);

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

            Assert.False(callCompleted);

            Assert.True(appTask.Wait(1000));
            Assert.False(appTask.IsFaulted);
            Assert.False(appTask.IsCanceled);

            ResultParameters returnedResult = appTask.Result;

            Assert.IsNotNull(returnedResult.Body);
            Assert.That(returnedResult.Body, Is.Not.SameAs(bodyDelegate));

            Task bodyTask = returnedResult.Body(null);
            
            Assert.True(bodyTask.Wait(1000));
            Assert.That(bodyTask.IsCompleted, Is.True);
            Assert.That(bodyTask.IsFaulted, Is.False);
            Assert.That(bodyTask.IsCanceled, Is.False);
            Assert.That(callCompleted, Is.True);
        }
    }
}
