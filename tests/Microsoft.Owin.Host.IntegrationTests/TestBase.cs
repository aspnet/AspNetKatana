// -----------------------------------------------------------------------
// <copyright file="TestBase.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
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
            _disposing.Cancel(false);
        }

        public int RunWebServer(
            string serverName = null,
            Action<IAppBuilder> application = null)
        {
            Debug.Assert(application != null, "application != null");
            Debug.Assert(application.Method.DeclaringType != null, "application.Method.DeclaringType != null");

            return RunWebServer(
                serverName: serverName,
                applicationName: application.Method.DeclaringType.FullName + "." + application.Method.Name);
        }

        public int RunWebServer(
            string serverName = null,
            string applicationName = null)
        {
            if (serverName == "Microsoft.Owin.Host.SystemWeb")
            {
                return RunWebServerSystemWeb(applicationName);
            }
            else
            {
                return RunWebServerViaEngine(serverName, applicationName);
            }
        }

        private int RunWebServerViaEngine(
            string serverName,
            string applicationName)
        {
            var port = GetAvailablePort();

            var sourceDirectory = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

            var targetDirectory = BuildTargetDirectory(
                sourceDirectory,
                applicationName,
                port);

            Directory.SetCurrentDirectory(targetDirectory);

            var server = WebApplication.Start(
                boot: "Default",
                server: serverName,
                app: applicationName,
                port: port,
                host: "localhost");

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

        private int RunWebServerSystemWeb(string applicationName)
        {
            var tcs = new TaskCompletionSource<object>();

            var port = GetAvailablePort();

            var sourceDirectory = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

            var targetDirectory = BuildTargetDirectory(
                sourceDirectory,
                applicationName,
                port);

            var targetHostConfig = Path.Combine(targetDirectory, "ApplicationHost.config");

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

            return port;
        }

        private static string BuildTargetDirectory(
            string workingDirectory,
            string applicationName,
            int port)
        {
            string targetDirectory = Path.Combine(workingDirectory, "Port" + port + "-" + Guid.NewGuid().ToString("n"));
            string binDirectory = Path.Combine(targetDirectory, "bin");

            string sourceHostConfig = Path.Combine(workingDirectory, "applicationHost.config");
            var targetHostConfig = Path.Combine(targetDirectory, "applicationHost.config");

            string sourceWebConfig = Path.Combine(workingDirectory, "web.config");
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
                    .Replace("TheApplicationName", applicationName));

            foreach (var assemblyName in Directory.GetFiles(workingDirectory, "*.dll"))
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

        // Activate an application window.
        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr windowHandle);

        private void OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Trace.WriteLine(e.Data);
        }
    }
}
