// <copyright file="TestBase.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2011-2013 Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;
using Owin;

#if NET40
namespace Microsoft.Owin.Host40.IntegrationTests
#else

namespace Microsoft.Owin.Host45.IntegrationTests
#endif
{
    public class TestBase : IDisposable
    {
        private readonly CancellationTokenSource _disposing = new CancellationTokenSource();

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _disposing.Cancel(false);
                _disposing.Dispose();
            }
        }

        public int RunWebServer(
            string serverName = null,
            Action<IAppBuilder> application = null,
            string configFileName = null)
        {
            Debug.Assert(application != null, "application != null");
            Debug.Assert(application.Method.DeclaringType != null, "application.Method.DeclaringType != null");

            return RunWebServer(
                serverName: serverName,
                applicationName: application.Method.DeclaringType.FullName + "." + application.Method.Name,
                configFileName: configFileName);
        }

        public int RunWebServer(
            string serverName = null,
            string applicationName = null,
            string configFileName = null)
        {
            if (serverName == "Microsoft.Owin.Host.SystemWeb")
            {
                return RunWebServerSystemWeb(applicationName, configFileName);
            }
            else
            {
                return RunWebServerViaEngine(serverName, applicationName, configFileName);
            }
        }

        private int RunWebServerViaEngine(string serverName, string applicationName, string configFileName)
        {
            int port = GetAvailablePort();

            string sourceDirectory = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

            string targetDirectory = BuildTargetDirectory(
                sourceDirectory,
                configFileName,
                applicationName,
                port);

            Directory.SetCurrentDirectory(targetDirectory);

            IDisposable server = WebApplication.Start(options =>
            {
                options.Boot = "Domain";
                options.Server = serverName;
                options.App = applicationName;
                options.Url = "http://localhost:" + port + "/";
            });

            _disposing.Token.Register(() =>
            {
                server.Dispose();
                try
                {
                    Directory.Delete(targetDirectory, true);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(string.Format("Cleanup error {0}", ex.Message));
                }
            });

            return port;
        }

        private int RunWebServerSystemWeb(string applicationName, string configFileName)
        {
            var tcs = new TaskCompletionSource<object>();

            int port = GetAvailablePort();

            string sourceDirectory = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

            string targetDirectory = BuildTargetDirectory(
                sourceDirectory,
                configFileName,
                applicationName,
                port);

            string targetHostConfig = Path.Combine(targetDirectory, "ApplicationHost.config");

            string programFile32 = Environment.GetEnvironmentVariable("ProgramFiles(x86)");
            string iisExpressExe = Path.Combine(programFile32, "IIS Express", "iisexpress.exe");

            var info = new ProcessStartInfo
            {
                WorkingDirectory = targetDirectory,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                ErrorDialog = false,
                CreateNoWindow = false,
                FileName = iisExpressExe,
                Arguments = "/config:\"" + targetHostConfig + "\" /trace:error",
            };

            // Log.Debug("Executing {0}", Definition.Command);
            Process process = Process.Start(info);

            process.OutputDataReceived += OutputDataReceived;
            process.ErrorDataReceived += OutputDataReceived;
            process.Exited += (a, b) => tcs.TrySetResult(null);
            process.EnableRaisingEvents = true;

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            _disposing.Token.Register(() =>
            {
                tcs.Task.Wait(250);
                if (!process.HasExited)
                {
                    process.Kill();
                    tcs.Task.Wait();
                }
                try
                {
                    Directory.Delete(targetDirectory, true);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(string.Format("Cleanup error {0}", ex.Message));
                }
            });

            // Wait for the server to get started.
            Thread.Sleep(1000);

            return port;
        }

        private static string BuildTargetDirectory(
            string workingDirectory,
            string configFileName,
            string applicationName,
            int port)
        {
            string targetDirectory = Path.Combine(workingDirectory, "Port" + port + "-" + Guid.NewGuid().ToString("n"));
            string binDirectory = Path.Combine(targetDirectory, "bin");

            string sourceHostConfig = Path.Combine(workingDirectory, "applicationHost.config");
            string targetHostConfig = Path.Combine(targetDirectory, "applicationHost.config");

            string sourceWebConfig = Path.Combine(workingDirectory, configFileName ?? "web.config");
            string targetWebConfig = Path.Combine(targetDirectory, "web.config");

            Directory.CreateDirectory(targetDirectory);
            Directory.CreateDirectory(binDirectory);

            File.WriteAllText(
                targetHostConfig,
                File.ReadAllText(sourceHostConfig)
                    .Replace("TheBindingInformation", ":" + port + ":localhost")
                    .Replace("ThePhysicalPath", targetDirectory));

            File.WriteAllText(
                targetWebConfig,
                File.ReadAllText(sourceWebConfig)
#if NET40
.Replace("TheApplicationName", applicationName));
#else
                    .Replace("TheApplicationName", applicationName)
                    .Replace("targetFramework=\"4.0\"", "targetFramework=\"4.5\""));
#endif

            foreach (var assemblyName in Directory.GetFiles(workingDirectory, "*.dll"))
            {
                File.Copy(
                    Path.Combine(workingDirectory, assemblyName),
                    Path.Combine(binDirectory, Path.GetFileName(assemblyName)),
                    overwrite: false);
            }
            foreach (var assemblyName in Directory.GetFiles(workingDirectory, "*.exe"))
            {
                File.Copy(
                    Path.Combine(workingDirectory, assemblyName),
                    Path.Combine(binDirectory, Path.GetFileName(assemblyName)),
                    overwrite: false);
            }
            return targetDirectory;
        }

        private static int GetAvailablePort()
        {
            var socket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.IP);

            socket.Bind(new IPEndPoint(IPAddress.Any, 0));
            var endPoint = socket.LocalEndPoint as IPEndPoint;
            int port = 0;
            if (endPoint != null)
            {
                port = endPoint.Port;
            }
            socket.Close();
            return port;
        }

        private void OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Trace.WriteLine(e.Data);
        }
    }
}
