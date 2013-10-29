// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;

namespace Microsoft.Owin.Host.SystemWeb
{
    internal partial class OwinCallContext
    {
        private const string IIS7WorkerRequestTypeName = "System.Web.Hosting.IIS7WorkerRequest";
        private static readonly Lazy<RemoveHeaderDel> IIS7RemoveHeader = new Lazy<RemoveHeaderDel>(GetRemoveHeaderDelegate);

        private bool _compressionDisabled;

        private delegate void RemoveHeaderDel(HttpWorkerRequest workerRequest);

        private void DisableResponseCompression()
        {
            if (_compressionDisabled)
            {
                return;
            }

            // This forces the IIS compression module to leave this response alone.
            // If we don't do this, it will buffer the response to suit its own compression
            // logic, resulting in partial messages being sent to the client.
            RemoveAcceptEncoding();

            _httpResponse.CacheControl = "no-cache";
            _httpResponse.AddHeader("Connection", "keep-alive");

            _compressionDisabled = true;
        }

        private void RemoveAcceptEncoding()
        {
            try
            {
                var workerRequest = (HttpWorkerRequest)_httpContext.GetService(typeof(HttpWorkerRequest));
                if (HttpRuntime.UsingIntegratedPipeline && IIS7RemoveHeader.Value != null)
                {
                    // Optimized code path for IIS7, accessing Headers causes all headers to be read
                    IIS7RemoveHeader.Value.Invoke(workerRequest);
                }
                else
                {
                    try
                    {
                        _httpRequest.Headers.Remove("Accept-Encoding");
                    }
                    catch (PlatformNotSupportedException)
                    {
                        // Happens on cassini
                    }
                }
            }
            catch (NotImplementedException)
            {
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Lazy static initialize must not throw.")]
        private static RemoveHeaderDel GetRemoveHeaderDelegate()
        {
            try
            {
                Type iis7WorkerType = typeof(HttpContext).Assembly.GetType(IIS7WorkerRequestTypeName);
                MethodInfo methodInfo = iis7WorkerType.GetMethod("SetKnownRequestHeader", BindingFlags.NonPublic | BindingFlags.Instance);

                ParameterExpression workerParamExpr = Expression.Parameter(typeof(HttpWorkerRequest));
                UnaryExpression iis7WorkerParamExpr = Expression.Convert(workerParamExpr, iis7WorkerType);
                MethodCallExpression callExpr = Expression.Call(iis7WorkerParamExpr, methodInfo,
                    Expression.Constant(HttpWorkerRequest.HeaderAcceptEncoding),
                    Expression.Constant(null, typeof(string)), Expression.Constant(false));
                return Expression.Lambda<RemoveHeaderDel>(callExpr, workerParamExpr).Compile();
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
