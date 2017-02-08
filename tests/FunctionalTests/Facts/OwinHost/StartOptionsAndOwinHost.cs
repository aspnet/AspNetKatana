// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using Owin;
using Xunit;
using Xunit.Extensions;

namespace FunctionalTests.Facts.OwinHost
{
    public partial class StartOptionsAndOwinHost
    {
        [Theory, Trait("FunctionalTests", "General")]
        [InlineData(false, null, 9999, false, false, null)]
        [InlineData(false, null, 9999, false, false, new string[] { "http://localhost:5555/", "http://localhost:6666", "http://localhost:7777" })]
        [InlineData(false, null, null, true, false, null)]
        [InlineData(false, null, null, false, false, null)]
        [InlineData(false, null, null, false, true, null)]
        [InlineData(true, "log.txt", 9999, false, false, null)]
        [InlineData(true, null, 9999, false, false, new string[] { "http://localhost:5555/", "http://localhost:6666", "http://localhost:7777" })]
        [InlineData(true, null, null, true, false, null)]
        [InlineData(true, null, null, false, false, new string[] { "http://localhost:5555/" })]
        [InlineData(true, null, null, false, true, null)]
        public void StartupOptionsAndOwinHostFacts(bool useOwinHostExe, string traceOutputFile,
            int? port, bool portInEnvironmentVar,
            bool passSettings, string[] urls)
        {
            try
            {
                MyStartOptions options = new MyStartOptions(useOwinHostExe);
                string defaultApplicationUrl = "http://localhost:5000/";

                if (!string.IsNullOrWhiteSpace(traceOutputFile))
                {
                    DeleteFile(traceOutputFile);
                    options.TraceOutput = traceOutputFile;
                }

                #region Port
                if (port.HasValue)
                {
                    options.Port = port.Value;
                    defaultApplicationUrl = string.Format("http://localhost:{0}/", port.Value);
                }

                if (portInEnvironmentVar)
                {
                    //Set environment Variable
                    Environment.SetEnvironmentVariable("PORT", "10001", EnvironmentVariableTarget.Process);
                    defaultApplicationUrl = string.Format("http://localhost:{0}/", 10001);
                }
                #endregion Port

                var traceFileName = System.Guid.NewGuid().ToString() + ".txt";
                if (passSettings)
                {
                    options.Settings.Add("traceoutput", traceFileName);
                }

                if (urls != null)
                {
                    for (int i = 0; i < urls.Length; i++)
                    {
                        options.Urls.Add(urls[i]);
                    }
                }

                string expectedLogText = null;

                using (new HostServer(options))
                {
                    List<string> allUrls = new List<string>(options.Urls);
                    if (allUrls.Count == 0)
                    {
                        allUrls.Add(defaultApplicationUrl);
                    }

                    for (int i = 0; i < allUrls.Count; i++)
                    {
                        var httpClient = new HttpClient();
                        var headers = new List<KeyValuePair<string, string>>();

                        if (!string.IsNullOrWhiteSpace(traceOutputFile) || passSettings)
                        {
                            expectedLogText = System.Guid.NewGuid().ToString();
                            httpClient.DefaultRequestHeaders.Add("outputFile", expectedLogText);
                        }

                        var response = httpClient.GetAsync(allUrls[i]).Result.Content.ReadAsStringAsync().Result;
                        Assert.Equal("SUCCESS", response);
                    }
                }

                #region Verify logoutput
                if (!string.IsNullOrWhiteSpace(traceOutputFile))
                {
                    ValidateLog(traceOutputFile, expectedLogText);
                }
                else if (passSettings)
                {
                    ValidateLog(traceFileName, expectedLogText);
                }
                #endregion Verify logoutput
            }
            finally
            {
                Environment.SetEnvironmentVariable("PORT", null, EnvironmentVariableTarget.Process);
            }
        }

        public void Configuration(IAppBuilder app)
        {
            app.Run(context =>
            {
                if (context.Request.Headers["outputFile"] != null)
                {
                    context.TraceOutput.WriteLine(context.Request.Headers["outputFile"]);
                }

                var capabilities = context.Request.Get<IDictionary<string, object>>("server.Capabilities");
                if (capabilities.ContainsKey("server.Name") && capabilities["server.Name"].ToString() == "MyServer")
                {
                    return context.Response.WriteAsync(context.Request.Headers["MyServer"]);
                }

                return context.Response.WriteAsync("SUCCESS");
            });
        }

        Action<string, string> ValidateLog = (logFileName, expectedLogContent) =>
        {
            Assert.True(File.Exists(logFileName), string.Format("Log file '{0}' not created", logFileName));
            Assert.True(new FileInfo(logFileName).Length > 0, string.Format("No log written in file '{0}'", logFileName));
        };

        Action<string> DeleteFile = (fileName) =>
        {
            if (File.Exists(fileName))
            {
                try
                {
                    File.Delete(fileName);
                }
                catch (Exception exception)
                {
                    Trace.WriteLine(string.Format("Warning: Unable to delete the file {0} : {1}", fileName, exception.Message));
                }
            }
        };
    }
}