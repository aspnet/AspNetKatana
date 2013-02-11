// <copyright file="ContractAssert.cs" company="Microsoft Open Technologies, Inc.">
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

using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xunit;

namespace Microsoft.AspNet.Razor.Owin.Tests
{
    internal static class ContractAssert
    {
        public static void NotNull(Expression<Action> op, string paramName, bool ignoreTrace = false)
        {
            Action act = op.Compile();
            var argEx = Assert.Throws<ArgumentNullException>(() => act());
            VerifyArgEx(argEx, paramName, ignoreTrace ? null : op);
        }

        public static void OutOfRange(Expression<Action> op, string paramName, bool ignoreTrace = false)
        {
            InvalidArgument<ArgumentOutOfRangeException>(op, paramName, ignoreTrace);
        }

        public static void InvalidArgument<T>(Expression<Action> op, string paramName, bool ignoreTrace = false) where T : ArgumentException
        {
            Action act = op.Compile();
            var argEx = Assert.Throws<T>(() => act());
            VerifyArgEx(argEx, paramName, ignoreTrace ? null : op);
        }

        public static void NotNullOrEmpty(Expression<Action<string>> op, string paramName, bool ignoreTrace = false)
        {
            Action<string> act = op.Compile();
            VerifyNotNullOrEmpty(Assert.Throws<ArgumentException>(() => act(null)), paramName, ignoreTrace ? null : op);
            VerifyNotNullOrEmpty(Assert.Throws<ArgumentException>(() => act(String.Empty)), paramName, ignoreTrace ? null : op);
        }

        private static void VerifyNotNullOrEmpty(ArgumentException argumentException, string paramName, LambdaExpression op)
        {
            Assert.Equal(
                ToFullArgExMessage(String.Format(Resources.Argument_NotNullOrEmpty, paramName), paramName),
                argumentException.Message);
            VerifyArgEx(argumentException, paramName, op);
        }

        private static void VerifyArgEx(ArgumentException argumentException, string paramName, LambdaExpression op)
        {
            if (op != null && op.Body.NodeType == ExpressionType.Call)
            {
                // Check and make sure that call is on the top of the stack after removing Requires
                var call = ((MethodCallExpression)op.Body);
                MethodInfo expected = call.Method;
                var stack = new StackTrace(argumentException);
                StackFrame frame = stack.GetFrames().SkipWhile(f => f.GetMethod().DeclaringType.FullName == typeof(Requires).FullName).FirstOrDefault();
                MethodBase actual = frame.GetMethod();
                Assert.True(actual != null, "Unable to find stack frame.");

                string expectedSite = expected.DeclaringType.FullName + "." + expected.Name;
                string actualSite = actual.DeclaringType.FullName + "." + actual.Name;
                Assert.True(String.Equals(expectedSite, actualSite),
                    "Expected exception was thrown at an unexpected site." + Environment.NewLine +
                        "Expected: " + expectedSite + Environment.NewLine +
                        "Actual: " + actualSite);
            }

            Assert.Equal(paramName, argumentException.ParamName);
        }

        private static string ToFullArgExMessage(string message, string paramName)
        {
            return String.Format("{0}\r\nParameter name: {1}", message, paramName);
        }
    }
}
