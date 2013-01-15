// -----------------------------------------------------------------------
// <copyright file="RazorApplication.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gate;
using Microsoft.AspNet.Razor.Owin.Compilation;
using Microsoft.AspNet.Razor.Owin.Execution;
using Microsoft.AspNet.Razor.Owin.IO;
using Microsoft.AspNet.Razor.Owin.Routing;
using Owin;

namespace Microsoft.AspNet.Razor.Owin
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class RazorApplication
    {
        /// <summary>
        /// Use at your OWN RISK. NOTHING will be initialized for you!
        /// </summary>
        protected RazorApplication(AppFunc nextApp)
        {
            NextApp = nextApp;
        }

        // Consumers should use IoC or the Default UseRazor extension method to initialize this.
        public RazorApplication(
            AppFunc nextApp,
            IFileSystem fileSystem,
            string virtualRoot,
            IRouter router,
            ICompilationManager compiler,
            IPageActivator activator,
            IPageExecutor executor,
            ITraceFactory tracer)
            : this(nextApp)
        {
            Requires.NotNull(fileSystem, "fileSystem");
            Requires.NotNullOrEmpty(virtualRoot, "virtualRoot");
            Requires.NotNull(router, "router");
            Requires.NotNull(compiler, "compiler");
            Requires.NotNull(activator, "activator");
            Requires.NotNull(executor, "executor");
            Requires.NotNull(tracer, "tracer");

            FileSystem = fileSystem;
            VirtualRoot = virtualRoot;
            Router = router;
            CompilationManager = compiler;
            Executor = executor;
            Activator = activator;
            Tracer = tracer;

            var global = Tracer.ForApplication();
            global.WriteLine("Started at '{0}'=>'{1}'", VirtualRoot, FileSystem.Root);
        }

        protected AppFunc NextApp { get; set; }

        public IFileSystem FileSystem { get; protected set; }
        public string VirtualRoot { get; protected set; }
        public IRouter Router { get; protected set; }
        public ICompilationManager CompilationManager { get; protected set; }
        public IPageExecutor Executor { get; protected set; }
        public IPageActivator Activator { get; protected set; }
        public ITraceFactory Tracer { get; protected set; }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            Stopwatch sw = new Stopwatch();
            Request req = new Request(environment);
            var trace = Tracer.ForRequest(req);
            using (trace.StartTrace())
            {
                trace.WriteLine("Received {0} {1}", req.Method, req.Path);

                if (!IsUnder(VirtualRoot, req.Path))
                {
                    // Not for us!
                    await NextApp(environment);
                    return;
                }

                // Step 1. Route the request to a file
                RouteResult routed = await Router.Route(req, trace);
                if (!routed.Success)
                {
                    // Also not for us!
                    await NextApp(environment);
                    return;
                }
                trace.WriteLine("Router: '{0}' ==> '{1}'::'{2}'", req.Path, routed.File.Path, routed.PathInfo);

                // Step 2. Use the compilation manager to get the file's compiled type
                sw.Start();
                CompilationResult compiled = await CompilationManager.Compile(routed.File, trace);
                sw.Stop();
                if (!compiled.Success)
                {
                    trace.WriteLine("Compiler: '{0}' FAILED", routed.File.Name);
                    throw new CompilationFailedException(compiled.Messages, compiled.GeneratedCode);
                }
                if (compiled.SatisfiedFromCache)
                {
                    trace.WriteLine("Retrieved compiled code from cache in {0}ms", sw.ElapsedMilliseconds);
                }
                else
                {
                    trace.WriteLine("Compiled '{0}' in {1}ms", routed.File.Path, sw.ElapsedMilliseconds);
                }
                sw.Reset();

                // Step 3. Construct an instance using the PageActivator
                Type type = compiled.GetCompiledType();
                ActivationResult activated = Activator.ActivatePage(type, trace);
                if (!activated.Success)
                {
                    trace.WriteLine("Activator: '{0}' FAILED", type.FullName);
                    throw new ActivationFailedException(type);
                }
                trace.WriteLine("Activator: '{0}' SUCCESS", type.FullName);

                // Step 4. Execute the activated instance!
                await Executor.Execute(activated.Page, req, trace);                    
            }
        }

        internal static bool IsUnder(string root, string path)
        {
            if (String.IsNullOrEmpty(root))
            {
                return true;
            }
            root = root.TrimEnd('/');
            return path.StartsWith(root + "/", StringComparison.OrdinalIgnoreCase);
        }
    }
}
