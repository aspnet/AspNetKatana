// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Cors;

namespace Microsoft.Owin.Cors
{
    public class CorsMiddleware : OwinMiddleware
    {
        private readonly CorsPolicy _corsPolicy;
        private readonly ICorsEngine _corsEngine;

        public CorsMiddleware(OwinMiddleware next, CorsOptions options)
            : base(next)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            _corsPolicy = options.CorsPolicy;
            _corsEngine = options.CorsEngine ?? new CorsEngine();
        }

        public override async Task Invoke(IOwinContext context)
        {
            CorsRequestContext corsRequestContext = GetCorsRequestContext(context);

            if (corsRequestContext != null)
            {
                if (corsRequestContext.IsPreflight)
                {
                    await HandleCorsPreflightRequestAsync(context, corsRequestContext);
                }
                else
                {
                    await HandleCorsRequestAsync(context, corsRequestContext);
                }
            }
            else
            {
                await Next.Invoke(context);
            }
        }

        private async Task HandleCorsRequestAsync(IOwinContext context, CorsRequestContext corsRequestContext)
        {
            if (context == null)
            {
                throw new ArgumentNullException("request");
            }
            if (corsRequestContext == null)
            {
                throw new ArgumentNullException("corsRequestContext");
            }

            CorsResult result;
            if (TryEvaluateCorsPolicy(corsRequestContext, out result))
            {
                WriteCorsHeaders(context, result);
            }

            await Next.Invoke(context);
        }

        private Task HandleCorsPreflightRequestAsync(IOwinContext context, CorsRequestContext corsRequestContext)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (corsRequestContext == null)
            {
                throw new ArgumentNullException("corsRequestContext");
            }

            CorsResult result;
            if (!String.IsNullOrEmpty(corsRequestContext.AccessControlRequestMethod) &&
                TryEvaluateCorsPolicy(corsRequestContext, out result))
            {
                context.Response.StatusCode = 200;
                WriteCorsHeaders(context, result);
            }
            else
            {
                // We couldn't evaluate the cors policy so it's a bad request
                context.Response.StatusCode = 400;
            }

            return Task.FromResult(0);
        }

        private bool TryEvaluateCorsPolicy(CorsRequestContext corsRequestContext, out CorsResult result)
        {
            result = _corsEngine.EvaluatePolicy(corsRequestContext, _corsPolicy);
            return result != null && result.IsValid;
        }

        private static void WriteCorsHeaders(IOwinContext context, CorsResult result)
        {
            IDictionary<string, string> corsHeaders = result.ToResponseHeaders();
            if (corsHeaders != null)
            {
                foreach (KeyValuePair<string, string> header in corsHeaders)
                {
                    context.Response.Headers.Set(header.Key, header.Value);
                }
            }
        }

        private static CorsRequestContext GetCorsRequestContext(IOwinContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            string origin = context.Request.Headers.Get(CorsConstants.Origin);

            if (String.IsNullOrEmpty(origin))
            {
                return null;
            }

            var requestContext = new CorsRequestContext
            {
                RequestUri = context.Request.Uri,
                HttpMethod = context.Request.Method,
                Host = context.Request.Host,
                Origin = origin,
                AccessControlRequestMethod = context.Request.Headers.Get(CorsConstants.AccessControlRequestMethod)
            };

            IList<string> headerValues = context.Request.Headers.GetCommaSeparatedValues(CorsConstants.AccessControlRequestHeaders);

            if (headerValues != null)
            {
                foreach (string header in headerValues)
                {
                    requestContext.AccessControlRequestHeaders.Add(header);
                }
            }

            return requestContext;
        }
    }
}
