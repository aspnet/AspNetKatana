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
                fileContext.ComprehendRequestHeaders();
                fileContext.ApplyResponseHeaders();

                switch (fileContext.GetPreconditionState())
                {
                    case StaticFileContext.PreconditionState.Unspecified:
                    case StaticFileContext.PreconditionState.ShouldProcess:
                        if (fileContext.IsHeadMethod)
                        {
                            return fileContext.SendStatusAsync(Constants.Status200Ok);
                        }
                        return fileContext.SendAsync();

                    case StaticFileContext.PreconditionState.NotModified:
                        return fileContext.SendStatusAsync(Constants.Status304NotModified);

                    case StaticFileContext.PreconditionState.PartialContent:
                        if (fileContext.IsHeadMethod)
                        {
                            return fileContext.SendStatusAsync(Constants.Status206PartialContent);
                        }
                        return fileContext.SendRangesAsync();

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
