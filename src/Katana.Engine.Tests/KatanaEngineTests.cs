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
                Completed = CancellationToken.None,
            };
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
        public void CallDisposedNotChangedIfPresent()
        {
            var callDisposed = false;

            var app = KatanaEngine.Encapsulate(
                call => 
                {
                    call.Completed.Register(() => callDisposed = true);
                    return new TaskCompletionSource<ResultParameters>().Task;
                },
                Output);

            var cts = new CancellationTokenSource();
            CallParameters parameters = CreateEmptyRequest();
            parameters.Completed = cts.Token;

            app(parameters);
            Assert.That(callDisposed, Is.False);
            cts.Cancel();
            Assert.That(callDisposed, Is.True);
        }
         
        [Test]
        public void CallDisposedProvidedIfMissing()
        {
            var callDisposed = false;

            var app = KatanaEngine.Encapsulate(
                call => 
                {
                    call.Completed.Register(() => callDisposed = true);
                    return new TaskCompletionSource<ResultParameters>().Task;
                },
                Output);

            app(CreateEmptyRequest());

            Assert.That(callDisposed, Is.False);
        }
        
        [Test]
        public void AsyncFaultWillTriggerTheProvidedToken()
        {
            var callDisposed = false;
            TaskCompletionSource<ResultParameters> tcs = new TaskCompletionSource<ResultParameters>();

            var app = KatanaEngine.Encapsulate(
                call => 
                {
                    call.Completed.Register(() => callDisposed = true);
                    return tcs.Task;
                },
                Output);
            
            Task appTask = app(CreateEmptyRequest());

            Assert.False(callDisposed, "disposed before exception.");
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
            Assert.True(callDisposed, "disposed after exception.");
        }

        [Test]
        public void SyncFaultWillTriggerTheProvidedToken()
        {
            var callDisposed = false;

            var app = KatanaEngine.Encapsulate(
                call => 
                {
                    call.Completed.Register(() => callDisposed = true);
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
            Assert.That(callDisposed, Is.True);
            Assert.That(caught, Is.Not.Null);
            Assert.That(caught.Message, Is.EqualTo("Boom"));
        }

        [Test]
        public void ResponseBodyEndWillTriggerTheProvidedToken()
        {
            var callDisposed = false;
            TaskCompletionSource<ResultParameters> tcs = new TaskCompletionSource<ResultParameters>();

            var app = KatanaEngine.Encapsulate(
                call => 
                {
                    call.Completed.Register(() => callDisposed = true);
                    return tcs.Task;
                },
                Output);
            
            Task<ResultParameters> appTask = app(CreateEmptyRequest());
            
            Assert.False(callDisposed);
            Assert.False(appTask.IsCompleted);

            BodyDelegate bodyDelegate =
                (stream, canceled) =>
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

            Assert.False(callDisposed);

            Assert.True(appTask.Wait(1000));
            Assert.False(appTask.IsFaulted);
            Assert.False(appTask.IsCanceled);

            ResultParameters returnedResult = appTask.Result;

            Assert.IsNotNull(returnedResult.Body);
            Assert.That(returnedResult.Body, Is.Not.SameAs(bodyDelegate));

            Task bodyTask = returnedResult.Body(null, CancellationToken.None);
            
            Assert.True(bodyTask.Wait(1000));
            Assert.That(bodyTask.IsCompleted, Is.True);
            Assert.That(bodyTask.IsFaulted, Is.False);
            Assert.That(bodyTask.IsCanceled, Is.False);
            Assert.That(callDisposed, Is.True);
        }
    }
}
