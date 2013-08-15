// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting.Engine;
using Microsoft.Owin.Hosting.ServerFactory;
using Microsoft.Owin.Hosting.Services;
using Microsoft.Owin.Hosting.Utilities;
using Shouldly;
using Xunit;

namespace Microsoft.Owin.Hosting.Tests
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class HostingEngineTests
    {
        public HostingEngineTests()
        {
            Output = new StringWriter();
        }

        private TextWriter Output { get; set; }

        private IDictionary<string, object> CreateEmptyRequest()
        {
            return new Dictionary<string, object>();
        }

        [Fact]
        public Task TextWriterAddedIfNotPresentInEnvironment()
        {
            object actualOutput = null;
            var encapsulateOutput = new StringWriter();
            IList<KeyValuePair<string, object>> data = new[] { new KeyValuePair<string, object>("host.TraceOutput", encapsulateOutput) };

            var middleware = new Encapsulate(env =>
            {
                actualOutput = env["host.TraceOutput"];
                return TaskHelpers.Completed();
            }, data);

            return middleware.Invoke(CreateEmptyRequest()).Then(() => { actualOutput.ShouldBeSameAs(encapsulateOutput); });
        }

        [Fact]
        public Task TextWriterNotChangedIfPresent()
        {
            object actualOutput = null;
            var encapsulateOutput = new StringWriter();
            var environmentOutput = new StringWriter();
            IList<KeyValuePair<string, object>> data = new[] { new KeyValuePair<string, object>("host.TraceOutput", encapsulateOutput) };

            var middleware = new Encapsulate(env =>
            {
                actualOutput = env["host.TraceOutput"];
                return TaskHelpers.Completed();
            }, data);

            IDictionary<string, object> env2 = CreateEmptyRequest();
            env2["host.TraceOutput"] = environmentOutput;

            return middleware.Invoke(env2).Then(() =>
            {
                actualOutput.ShouldBeSameAs(environmentOutput);
                actualOutput.ShouldNotBeSameAs(encapsulateOutput);
            });
        }

        [Fact]
        public void InitializeAndCreateShouldBeCalledWithProperties()
        {
            var serverFactoryAlpha = new ServerFactoryAlpha();
            var startInfo = new StartContext(new StartOptions());
            startInfo.ServerFactory = new ServerFactoryAdapter(serverFactoryAlpha);
            startInfo.App = new AppFunc(env => TaskHelpers.Completed());

            var engine = ServicesFactory.Create().GetService<IHostingEngine>();

            serverFactoryAlpha.InitializeCalled.ShouldBe(false);
            serverFactoryAlpha.CreateCalled.ShouldBe(false);
            IDisposable server = engine.Start(startInfo);

            serverFactoryAlpha.InitializeCalled.ShouldBe(true);
            serverFactoryAlpha.CreateCalled.ShouldBe(true);
            serverFactoryAlpha.InitializeProperties.ShouldBeSameAs(serverFactoryAlpha.CreateProperties);
            server.Dispose();
        }

        public class ServerFactoryAlpha
        {
            public bool InitializeCalled { get; set; }
            public IDictionary<string, object> InitializeProperties { get; set; }
            public bool CreateCalled { get; set; }
            public IDictionary<string, object> CreateProperties { get; set; }

            public void Initialize(IDictionary<string, object> properties)
            {
                InitializeCalled = true;
                InitializeProperties = properties;
            }

            public IDisposable Create(AppFunc app, IDictionary<string, object> properties)
            {
                CreateCalled = true;
                CreateProperties = properties;
                return new Disposable(() => { });
            }
        }

        [Fact]
        public void CreateShouldBeProvidedWithAdaptedAppIfNeeded()
        {
            var serverFactoryBeta = new ServerFactoryBeta();
            var startInfo = new StartContext(new StartOptions());
            startInfo.ServerFactory = new ServerFactoryAdapter(serverFactoryBeta);
            startInfo.App = new AppFunc(env => TaskHelpers.Completed());

            var engine = ServicesFactory.Create().GetService<IHostingEngine>();
            serverFactoryBeta.CreateCalled.ShouldBe(false);
            IDisposable server = engine.Start(startInfo);
            serverFactoryBeta.CreateCalled.ShouldBe(true);
            server.Dispose();
        }

        public class ServerFactoryBeta
        {
            public bool CreateCalled { get; set; }

            public IDisposable Create(AppFunc app, IDictionary<string, object> properties)
            {
                CreateCalled = true;
                return new Disposable(() => { });
            }
        }

        [Fact]
        public void PropertiesShouldHaveExpectedKeysFromHost()
        {
            var serverFactory = new ServerFactoryAlpha();
            var startInfo = new StartContext(new StartOptions());
            startInfo.ServerFactory = new ServerFactoryAdapter(serverFactory);
            startInfo.App = new AppFunc(env => TaskHelpers.Completed());

            var engine = ServicesFactory.Create().GetService<IHostingEngine>();
            serverFactory.InitializeCalled.ShouldBe(false);
            serverFactory.CreateCalled.ShouldBe(false);
            IDisposable server = engine.Start(startInfo);

            serverFactory.InitializeProperties.ShouldContainKey("host.TraceOutput");
            serverFactory.InitializeProperties.ShouldContainKey("host.Addresses");

            serverFactory.InitializeProperties["host.TraceOutput"].ShouldBeTypeOf<TextWriter>();
            serverFactory.InitializeProperties["host.Addresses"].ShouldBeTypeOf<IList<IDictionary<string, object>>>();

            server.Dispose();
        }

        [Fact]
        public void MultipleUrlsSpecified()
        {
            var startOptions = new StartOptions();
            startOptions.Urls.Add("beta://localhost:3333");
            startOptions.Urls.Add("delta://foo/");
            startOptions.Urls.Add("gama://*:4444/");
            startOptions.Port = 1111; // Ignored because of Url(s)

            var serverFactory = new ServerFactoryAlpha();
            var startInfo = new StartContext(startOptions);
            startInfo.ServerFactory = new ServerFactoryAdapter(serverFactory);
            startInfo.App = new AppFunc(env => TaskHelpers.Completed());

            var engine = ServicesFactory.Create().GetService<IHostingEngine>();
            serverFactory.InitializeCalled.ShouldBe(false);
            serverFactory.CreateCalled.ShouldBe(false);
            IDisposable server = engine.Start(startInfo);

            serverFactory.InitializeProperties["host.Addresses"].ShouldBeTypeOf<IList<IDictionary<string, object>>>();

            var addresses = (IList<IDictionary<string, object>>)serverFactory.InitializeProperties["host.Addresses"];
            Assert.Equal(3, addresses.Count);

            var expectedAddresses = new[]
            {
                new[] { "beta", "localhost", "3333", string.Empty },
                new[] { "delta", "foo", string.Empty, "/" },
                new[] { "gama", "*", "4444", "/" },
            };

            for (int i = 0; i < addresses.Count; i++)
            {
                IDictionary<string, object> addressDictionary = addresses[i];
                string[] expectedValues = expectedAddresses[i];
                Assert.Equal(expectedValues.Length, addressDictionary.Count);
                Assert.Equal(expectedValues[0], (string)addressDictionary["scheme"]);
                Assert.Equal(expectedValues[1], (string)addressDictionary["host"]);
                Assert.Equal(expectedValues[2], (string)addressDictionary["port"]);
                Assert.Equal(expectedValues[3], (string)addressDictionary["path"]);
            }

            server.Dispose();
        }

        [Fact]
        public void DeconstructUrlSplitsKnownParts()
        {
            DeconstructUrlTest("http://localhost:8080/path", true, "http", "localhost", "8080", "/path");
        }

        [Fact]
        public void MustHaveColonSlashSlash()
        {
            DeconstructUrlTest("http:/localhost:8080/path", false, null, null, null, null);
        }

        [Fact]
        public void WillProvideDefaultPorts()
        {
            DeconstructUrlTest("http://localhost/", true, "http", "localhost", "80", "/");
            DeconstructUrlTest("https://localhost/", true, "https", "localhost", "443", "/");
            DeconstructUrlTest("http://localhost", true, "http", "localhost", "80", string.Empty);
            DeconstructUrlTest("https://localhost", true, "https", "localhost", "443", string.Empty);
        }

        [Fact]
        public void WillAcceptCustomPorts()
        {
            DeconstructUrlTest("http://localhost:81/", true, "http", "localhost", "81", "/");
            DeconstructUrlTest("https://localhost:444/", true, "https", "localhost", "444", "/");
            DeconstructUrlTest("http://localhost:81", true, "http", "localhost", "81", string.Empty);
            DeconstructUrlTest("https://localhost:444", true, "https", "localhost", "444", string.Empty);
        }

        [Fact]
        public void BadPortBecomesPartOfHost()
        {
            DeconstructUrlTest("http://localhost:81a/", true, "http", "localhost:81a", "80", "/");
            DeconstructUrlTest("https://localhost:444b/", true, "https", "localhost:444b", "443", "/");
            DeconstructUrlTest("http://localhost:/", true, "http", "localhost:", "80", "/");
            DeconstructUrlTest("http://:localhost/", true, "http", ":localhost", "80", "/");
        }

        [Fact]
        public void UnknownSchemeAllowed()
        {
            DeconstructUrlTest("abcd://localhost:555/", true, "abcd", "localhost", "555", "/");
            DeconstructUrlTest("abcd://localhost/", true, "abcd", "localhost", string.Empty, "/");
        }

        [Fact]
        public void DoesNotRequireTrailingSlash()
        {
            DeconstructUrlTest("http://localhost:8080", true, "http", "localhost", "8080", string.Empty);
            DeconstructUrlTest("http://localhost", true, "http", "localhost", "80", string.Empty);
        }

        private static void DeconstructUrlTest(string url, bool valid, string scheme, string host, string port, string path)
        {
            string schemePart;
            string hostPart;
            string portPart;
            string pathPart;
            HostingEngine.DeconstructUrl(
                url,
                out schemePart,
                out hostPart,
                out portPart,
                out pathPart).ShouldBe(valid);
            schemePart.ShouldBe(scheme);
            hostPart.ShouldBe(host);
            portPart.ShouldBe(port);
            pathPart.ShouldBe(path);
        }
    }
}
