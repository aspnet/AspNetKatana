// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
        public const int HttpsPort = 9090;
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
            string configFileName = null,
            bool https = false)
        {
            Debug.Assert(application != null, "application != null");
            Debug.Assert(application.Method.DeclaringType != null, "application.Method.DeclaringType != null");

            return RunWebServer(
                serverName: serverName,
                applicationName: application.Method.DeclaringType.FullName + "." + application.Method.Name,
                configFileName: configFileName,
                https: https);
        }

        public int RunWebServer(
            string serverName = null,
            string applicationName = null,
            string configFileName = null,
            bool https = false)
        {
            if (serverName == "Microsoft.Owin.Host.SystemWeb")
            {
                return RunWebServerSystemWeb(applicationName, configFileName, https);
            }
            else
            {
                return RunWebServerViaEngine(serverName, applicationName, configFileName, https);
            }
        }

        private int RunWebServerViaEngine(string serverName, string applicationName, string configFileName, bool https)
        {
            int port = GetAvailablePort(https);
            string scheme = https ? "https" : "http";

            string sourceDirectory = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

            string targetDirectory = BuildTargetDirectory(
                sourceDirectory,
                configFileName,
                applicationName,
                scheme,
                port);

            Directory.SetCurrentDirectory(targetDirectory);

            var options = new StartOptions(scheme + "://localhost:" + port + "/")
            {
                ServerFactory = serverName,
                AppStartup = applicationName,
            };
            options.Settings["boot"] = "Domain";

            IDisposable server = WebApp.Start(options);

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

        private int RunWebServerSystemWeb(string applicationName, string configFileName, bool https)
        {
            var tcs = new TaskCompletionSource<object>();

            int port = GetAvailablePort(https);

            string sourceDirectory = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

            string targetDirectory = BuildTargetDirectory(
                sourceDirectory,
                configFileName,
                applicationName,
                https ? "https" : "http",
                port);

            Directory.SetCurrentDirectory(targetDirectory);

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
            string scheme,
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
                    .Replace("TheScheme", scheme)
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
            foreach (var assemblyName in Directory.GetFiles(workingDirectory, "*.pfx"))
            {
                File.Copy(
                    Path.Combine(workingDirectory, assemblyName),
                    Path.Combine(targetDirectory, Path.GetFileName(assemblyName)),
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

        private static int GetAvailablePort(bool https)
        {
            if (https)
            {
                return HttpsPort;
            }

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
