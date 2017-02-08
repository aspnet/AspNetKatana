// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Owin;

namespace FunctionalTests.Facts.IntegratedPipeline
{
    public class IntegratedPipelineMiddleware : OwinMiddleware
    {
        private const string Error_Incorrect_Middleware_Unwinding = "[Failed]: Correct OWIN middlewareId not called in reverse order while unwinding. Expected middlewareId '{0}' to be called now, but middlewareId '{1}' called instead";
        private const string Error_UnexpectedMiddleware = "[Failed]: OWIN middlewareId '{2}' expected to be called at '{0}' where as it is called at '{1}'";

        private RequestNotification expectedStageName;
        private int middlewareId;

        public IntegratedPipelineMiddleware(OwinMiddleware next, RequestNotification expectedStageName, int middlewareId)
            : base(next)
        {
            this.expectedStageName = expectedStageName;
            this.middlewareId = middlewareId;
        }

        public override Task Invoke(IOwinContext context)
        {
            validateStage(context, expectedStageName, middlewareId);
            context.Get<Stack<int>>("stack").Push(middlewareId);

            return this.Next.Invoke(context).ContinueWith((result) =>
            {
                validateStage(context, RequestNotification.EndRequest, middlewareId);

                var expectedMiddlewareId = context.Get<Stack<int>>("stack").Pop();
                if (expectedMiddlewareId != middlewareId)
                {
                    throw new Exception(string.Format(Error_Incorrect_Middleware_Unwinding, expectedMiddlewareId, middlewareId));
                }
            });
        }

        private void validateStage(IOwinContext context, RequestNotification expectedStageName, int middlewareId)
        {
            var calledAtStage = context.Get<System.Web.HttpContextWrapper>("System.Web.HttpContextBase").CurrentNotification;
            if (calledAtStage != expectedStageName)
            {
                throw new Exception(string.Format(Error_UnexpectedMiddleware, expectedStageName, calledAtStage, middlewareId));
            }
        }
    }
}