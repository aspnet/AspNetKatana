using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using NUnit.Framework;
using Owin;
using Environment = Gate.Environment;

namespace Katana.Engine.Tests
{
    public class KatanaEngineTests
    {
        AppDelegate AppDelegate { get; set; }
        ResultDelegate ResultDelegate { get; set; }
        Action<Exception> FaultDelegate { get; set; }

        TextWriter Output { get; set; }
        IDictionary<string, object> CalledEnv { get; set; }

        [SetUp]
        public void Init()
        {
            Output = new StringWriter();
            AppDelegate = DefaultApp;
            ResultDelegate = DefaultResult;
            FaultDelegate = DefaultFault;
        }


        void DefaultApp(IDictionary<string, object> env, ResultDelegate result, Action<Exception> fault)
        {
            CalledEnv = env;
        }

        void DefaultResult(string status, IDictionary<string, IEnumerable<string>> headers, BodyDelegate body)
        {
        }

        void DefaultFault(Exception obj)
        {
        }

        [Test]
        public void TextWriterAddedIfNotPresentInEnvironment()
        {
            object actualOutput = null;
            var encapsulateOutput = new StringWriter();

            var app = KatanaEngine.Encapsulate(
                (env, result, fault) => { actualOutput = env["host.TraceOutput"]; },
                encapsulateOutput);

            app(new Environment(), ResultDelegate, FaultDelegate);
            Assert.That(actualOutput, Is.SameAs(encapsulateOutput));
        }

        [Test]
        public void TextWriterNotChangedIfPresent()
        {
            object actualOutput = null;
            var encapsulateOutput = new StringWriter();
            var environmentOutput = new StringWriter();

            var app = KatanaEngine.Encapsulate(
                (env, result, fault) => { actualOutput = env["host.TraceOutput"]; },
                encapsulateOutput);

            var environment = new Environment();
            environment["host.TraceOutput"] = environmentOutput;

            app(environment, ResultDelegate, FaultDelegate);
            Assert.That(actualOutput, Is.SameAs(environmentOutput));
            Assert.That(actualOutput, Is.Not.SameAs(encapsulateOutput));
        }

        [Test]
        public void CallDisposedNotChangedIfPresent()
        {
            var callDisposed = false;

            var app = KatanaEngine.Encapsulate(
                (env, result, fault) => ((CancellationToken)env["host.CallDisposed"]).Register(() => callDisposed = true),
                Output);

            var environment = new Environment();
            var cts = new CancellationTokenSource();
            environment["host.CallDisposed"] = cts.Token;

            app(environment, ResultDelegate, FaultDelegate);
            Assert.That(callDisposed, Is.False);
            cts.Cancel();
            Assert.That(callDisposed, Is.True);
        }

        [Test]
        public void CallDisposedProvidedIfMissing()
        {
            var callDisposed = false;

            var app = KatanaEngine.Encapsulate(
                (env, result, fault) => ((CancellationToken)env["host.CallDisposed"]).Register(() => callDisposed = true),
                Output);

            var environment = new Environment();
            app(environment, ResultDelegate, FaultDelegate);

            Assert.That(callDisposed, Is.False);
        }


        [Test]
        public void AsyncFaultWillTriggerTheProvidedToken()
        {
            var callDisposed = false;
            ResultDelegate resultDelegate = null;
            Action<Exception> faultDelegate = null;

            var app = KatanaEngine.Encapsulate(
                (env, result, fault) =>
                {
                    ((CancellationToken)env["host.CallDisposed"]).Register(() => callDisposed = true);
                    resultDelegate = result;
                    faultDelegate = fault;
                },
                Output);

            var environment = new Environment();
            app(environment, ResultDelegate, FaultDelegate);

            Assert.That(callDisposed, Is.False);
            faultDelegate(new Exception("Simulating Async Exception"));
            Assert.That(callDisposed, Is.True);
        }

        [Test]
        public void SyncFaultWillTriggerTheProvidedToken()
        {
            var callDisposed = false;
            ResultDelegate resultDelegate = null;
            Action<Exception> faultDelegate = null;

            var app = KatanaEngine.Encapsulate(
                (env, result, fault) =>
                {
                    ((CancellationToken)env["host.CallDisposed"]).Register(() => callDisposed = true);
                    resultDelegate = result;
                    faultDelegate = fault;
                    throw new ApplicationException("Boom");
                },
                Output);

            var environment = new Environment();
            Exception caught = null;
            try
            {
                app(environment, ResultDelegate, FaultDelegate);
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
            ResultDelegate resultDelegate = null;
            Action<Exception> faultDelegate = null;

            var app = KatanaEngine.Encapsulate(
                (env, result, fault) =>
                {
                    ((CancellationToken)env["host.CallDisposed"]).Register(() => callDisposed = true);
                    resultDelegate = result;
                },
                Output);

            var environment = new Environment();

            BodyDelegate bodyDelegate = null;
            app(environment, (status, headers, body) => bodyDelegate = body, FaultDelegate);

            Assert.That(callDisposed, Is.False);
            Assert.That(bodyDelegate, Is.Null);

            resultDelegate(
                "200 OK",
                new Dictionary<string, IEnumerable<string>>(),
                (write, flush, end, cancel) => end(null));

            Assert.That(callDisposed, Is.False);
            Assert.That(bodyDelegate, Is.Not.Null);

            bodyDelegate(_ => false, _ => false, _ => { }, CancellationToken.None);

            Assert.That(callDisposed, Is.True);
        }
    }
}
