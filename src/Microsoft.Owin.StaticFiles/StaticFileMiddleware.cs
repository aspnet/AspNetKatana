// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Microsoft.Owin.StaticFiles
{
    /// <summary>
    /// Enables serving static files for a given request path
    /// </summary>
    public class StaticFileMiddleware : OwinMiddleware
    {
        private readonly StaticFileOptions _options;
        private readonly PathString _matchUrl;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="next"></param>
        /// <param name="options"></param>
        public StaticFileMiddleware(OwinMiddleware next, StaticFileOptions options)
            : base(next)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            _options = options;
            _matchUrl = options.RequestPath;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task Invoke(IOwinContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            var fileContext = new StaticFileContext(context, _options, _matchUrl);
            if (fileContext.ValidateMethod()
                && fileContext.ValidatePath()
                && fileContext.LookupContentType()
                && fileContext.LookupFileInfo())
            {
                FileAccessPolicyContext accessContext = fileContext.CheckPolicy();
                if (accessContext.IsRejected)
                {
                    // Status code set by policy
                    return Constants.CompletedTask;
                }
                if (accessContext.IsPassThrough)
                {
                    return Next.Invoke(context);
                }

                fileContext.ComprehendRequestHeaders();

                switch (fileContext.GetPreconditionState())
                {
                    case StaticFileContext.PreconditionState.Unspecified:
                    case StaticFileContext.PreconditionState.ShouldProcess:
                        fileContext.ApplyResponseHeaders();
                        if (fileContext.IsHeadMethod)
                        {
                            return fileContext.SendStatusAsync(Constants.Status200Ok);
                        }
                        return fileContext.SendAsync();

                    case StaticFileContext.PreconditionState.NotModified:
                        fileContext.ApplyResponseHeaders();
                        return fileContext.SendStatusAsync(Constants.Status304NotModified);

                    case StaticFileContext.PreconditionState.PartialContent:
                        fileContext.ApplyResponseHeaders();
                        if (fileContext.IsHeadMethod)
                        {
                            return fileContext.SendStatusAsync(Constants.Status206PartialContent);
                        }
                        return fileContext.SendRangeAsync();

                    case StaticFileContext.PreconditionState.PreconditionFailed:
                        return fileContext.SendStatusAsync(Constants.Status412PreconditionFailed);

                    case StaticFileContext.PreconditionState.NotSatisfiable:
                        return fileContext.SendStatusAsync(Constants.Status416RangeNotSatisfiable);

                    default:
                        throw new NotImplementedException(fileContext.GetPreconditionState().ToString());
                }
            }

            return Next.Invoke(context);
        }
    }
}
