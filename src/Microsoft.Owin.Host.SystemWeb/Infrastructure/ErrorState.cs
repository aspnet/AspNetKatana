// <copyright file="ErrorState.cs" company="Microsoft Open Technologies, Inc.">
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

#if !NET40

using System;
using System.Runtime.ExceptionServices;

namespace Microsoft.Owin.Host.SystemWeb.Infrastructure
{
    internal class ErrorState
    {
        private readonly ExceptionDispatchInfo _exceptionDispatchInfo;

        private ErrorState(ExceptionDispatchInfo exceptionDispatchInfo)
        {
            _exceptionDispatchInfo = exceptionDispatchInfo;
        }

        public static ErrorState Capture(Exception exception)
        {
            ExceptionDispatchInfo exceptionDispatchInfo = ExceptionDispatchInfo.Capture(exception);
            return new ErrorState(exceptionDispatchInfo);
        }

        public void Rethrow()
        {
            _exceptionDispatchInfo.Throw();
        }
    }
}

#else

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Microsoft.Owin.Host.SystemWeb.Infrastructure
{
    internal class ErrorState
    {
        private static readonly Action<Exception> RethrowWithOriginalStack = GetRethrowWithNoStackLossDelegate();

        private readonly Exception _exception;

        private ErrorState(Exception exception)
        {
            _exception = exception;
        }

        public static ErrorState Capture(Exception exception)
        {
            return new ErrorState(exception);
        }

        public void Rethrow()
        {
            RethrowWithOriginalStack(_exception);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We only want to re-throw the original exception.")]
        private static Action<Exception> GetRethrowWithNoStackLossDelegate()
        {
            Func<Exception, Exception> prepForRemoting = null;

            try
            {
                if (AppDomain.CurrentDomain.IsFullyTrusted)
                {
                    // .NET 4 - do the same thing Lazy<T> does by calling Exception.PrepForRemoting
                    // This is an internal method in mscorlib.dll, so pass a test Exception to it to make sure we can call it.
                    ParameterExpression exceptionParameter = Expression.Parameter(typeof(Exception));
                    MethodCallExpression prepForRemotingCall = Expression.Call(exceptionParameter, "PrepForRemoting", Type.EmptyTypes);
                    Expression<Func<Exception, Exception>> lambda = Expression.Lambda<Func<Exception, Exception>>(prepForRemotingCall, exceptionParameter);
                    Func<Exception, Exception> func = lambda.Compile();
                    func(new InvalidOperationException()); // make sure the method call succeeds before assigning the 'prepForRemoting' local variable
                    prepForRemoting = func;
                }
            }
            catch (Exception)
            {
            } // If delegate creation fails (medium trust) we will simply throw the base exception.

            return ex =>
            {
                if (prepForRemoting != null)
                {
                    ex = prepForRemoting(ex);
                }
                throw ex;
            };
        }
    }
}

#endif
