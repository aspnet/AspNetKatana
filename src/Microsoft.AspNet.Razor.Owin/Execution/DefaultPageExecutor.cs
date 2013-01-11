// -----------------------------------------------------------------------
// <copyright file="DefaultPageExecutor.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Gate;
using Microsoft.AspNet.Razor.Owin;

namespace Microsoft.AspNet.Razor.Owin.Execution
{
    public class DefaultPageExecutor : IPageExecutor
    {
        public Task Execute(IRazorPage page, Request request, ITrace tracer)
        {
            Requires.NotNull(page, "page");
            Requires.NotNull(request, "request");
            Requires.NotNull(tracer, "tracer");

            return ExecuteCore(page, request, tracer);
        }

        private static async Task ExecuteCore(IRazorPage page, Request request, ITrace tracer)
        {
            Response resp = new Response(request.Environment);
            await page.Run(request, resp);
        }
    }
}
