// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using FunctionalTests.Common;
using Microsoft.Owin;
using Owin;
using Xunit;
using Xunit.Extensions;

namespace FunctionalTests.Facts.General
{
    public class EnvironmentDictionaryVerificationTest
    {
        [Theory, Trait("FunctionalTests", "General")]
        [InlineData(HostType.IIS)]
        [InlineData(HostType.HttpListener)]
        public void EnvironmentDictionaryVerification(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                string applicationUrl = deployer.Deploy(hostType, Configuration);

                List<KeyValuePair<string, string[]>> additionalHeaders = new List<KeyValuePair<string, string[]>>();
                additionalHeaders.Add(new KeyValuePair<string, string[]>("CustomHeader1", new string[] { "CustomHeaderValue1" }));

                string targetUrl = applicationUrl + "/Test/?query=value";
                string getResponseText = HttpClientUtility.GetResponseTextFromUrl(targetUrl, additionalHeaders);
                Uri targetUri = new Uri(targetUrl);
                Dictionary<string, string> owinDictionary = ParseResponse(getResponseText);

                #region Utility function
                Func<string, bool> validatePath = (httpMethod) =>
                {
                    if (hostType == HostType.IIS)
                    {
                        WebDeployer webDeployer = (WebDeployer)deployer.Application;
                        string virtualDirectoryName = webDeployer.Application.Path.TrimStart(new char[] { '/' });
                        if (owinDictionary["owin.RequestPath"] != "/Test/" || owinDictionary["owin.RequestPathBase"] != "/" + virtualDirectoryName)
                        {
                            Assert.True(false, string.Format("{0} environment dictionary verification failed. At least one of the values returned by the server does not match the expected value. String returned by server : {1}", httpMethod, getResponseText));
                        }
                    }
                    else
                    {
                        if (owinDictionary["owin.RequestPath"].TrimStart(new char[] { '/' }) != targetUri.AbsolutePath.TrimStart(new char[] { '/' }) ||
                            !string.IsNullOrWhiteSpace(owinDictionary["owin.RequestPathBase"]))
                        {
                            Assert.True(false, string.Format("{0} environment dictionary verification failed. At least one of the values returned by the server does not match the expected value. String returned by server : {1}", httpMethod, getResponseText));
                        }
                    }

                    return true;
                };
                #endregion

                //GET
                validatePath("GET");

                if (owinDictionary["CustomHeader1"] != "CustomHeaderValue1" || !owinDictionary["Host"].Contains(targetUri.Host) ||
                    owinDictionary["owin.RequestMethod"] != "GET" || owinDictionary["owin.RequestProtocol"] != "HTTP/1.1" || owinDictionary["owin.RequestQueryString"] != targetUri.Query.TrimStart(new char[] { '?' }) ||
                owinDictionary["owin.RequestScheme"] != targetUri.Scheme || owinDictionary["owin.Version"] != "1.0")
                {
                    Assert.True(false, string.Format("GET environment dictionary verification failed. At least one of the values returned by the server does not match the expected value. String returned by server : {0}", getResponseText));
                }

                //POST
                string postResponseText = HttpClientUtility.PostResponseTextFromUrl(targetUrl, additionalHeaders);
                owinDictionary = ParseResponse(postResponseText);
                validatePath("POST");

                if (owinDictionary["CustomHeader1"] != "CustomHeaderValue1" || !owinDictionary["Host"].Contains(targetUri.Host) ||
                    owinDictionary["owin.RequestMethod"] != "POST" || owinDictionary["owin.RequestProtocol"] != "HTTP/1.1" ||
                    owinDictionary["owin.RequestQueryString"] != targetUri.Query.TrimStart(new char[] { '?' }) ||
                    owinDictionary["owin.RequestScheme"] != targetUri.Scheme || owinDictionary["owin.Version"] != "1.0")
                {
                    Assert.True(false, string.Format("POST environment dictionary verification failed. At least one of the values returned by the server does not match the expected value. String returned by server : {0}", postResponseText));
                }
            }
        }

        private static Dictionary<string, string> ParseResponse(string response)
        {
            Dictionary<string, string> responseDictionary = new Dictionary<string, string>();
            foreach (string keyValue in response.Split(new string[] { "^^" }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] keyValuePair = keyValue.Split(new string[] { "==" }, StringSplitOptions.RemoveEmptyEntries);
                if (keyValuePair.Length == 2)
                {
                    responseDictionary.Add(keyValuePair[0], keyValuePair[1]);
                }
                else if (keyValuePair.Length == 1)
                {
                    responseDictionary.Add(keyValuePair[0], string.Empty);
                }
            }

            return responseDictionary;
        }

        internal void Configuration(IAppBuilder appBuilder)
        {
            appBuilder.Run(context =>
            {
                Func<string, string> formatFunction = (key) =>
                {
                    return string.Format("{0}=={1}", key, context.Get<string>(key));
                };

                List<string> responseTokens = new List<string>();

                // Request essential values as per OWIN specifications
                ThrowIfKeyNotFound<Stream>(context, "owin.RequestBody");

                foreach (var kvp in context.Get<IDictionary<string, string[]>>("owin.RequestHeaders"))
                {
                    responseTokens.Add(string.Format("{0}=={1}", kvp.Key, string.Join(";", kvp.Value)));
                }

                responseTokens.Add(formatFunction("owin.RequestMethod"));
                responseTokens.Add(formatFunction("owin.RequestPath"));
                responseTokens.Add(formatFunction("owin.RequestPathBase"));
                responseTokens.Add(formatFunction("owin.RequestProtocol"));
                responseTokens.Add(formatFunction("owin.RequestQueryString"));
                responseTokens.Add(formatFunction("owin.RequestScheme"));

                // Response essential values as per OWIN specifications
                ThrowIfKeyNotFound<Stream>(context, "owin.ResponseBody");
                ThrowIfKeyNotFound<object>(context, "owin.ResponseHeaders");
                ThrowIfKeyNotFound<object>(context, "owin.ResponseStatusCode");

                // Other data as per OWIN specification
                ThrowIfKeyNotFound<CancellationToken>(context, "owin.CallCancelled");
                responseTokens.Add(formatFunction("owin.Version"));

                // Katana specific keys
                ThrowIfKeyNotFound<TextWriter>(context, "host.TraceOutput");
                ThrowIfKeyNotFound<CancellationToken>(context, "host.OnAppDisposing");

                ThrowIfKeyNotFound<string>(context, "host.AppName");
                ThrowIfKeyNotFound<Action<Action<object>, object>>(context, "server.OnSendingHeaders");

                ThrowIfKeyNotFound<IDictionary<string, object>>(context, "server.Capabilities");
                ThrowIfKeyNotFound<string>(context, "server.RemoteIpAddress");
                ThrowIfKeyNotFound<string>(context, "server.RemotePort");
                ThrowIfKeyNotFound<string>(context, "server.LocalIpAddress");
                ThrowIfKeyNotFound<string>(context, "server.LocalPort");
                ThrowIfKeyNotFound<bool>(context, "server.IsLocal");

                return context.Response.WriteAsync(string.Join("^^", responseTokens.ToArray()));
            });
        }

        private static void ThrowIfKeyNotFound<T>(IOwinContext context, string key)
        {
            if (context.Get<T>(key) == null)
            {
                throw new Exception(string.Format("Key with name '{0}' cannot be found with type '{1}", key, typeof(T).Name));
            }
        }
    }
}