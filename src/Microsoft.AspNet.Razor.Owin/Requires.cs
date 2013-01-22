// <copyright file="Requires.cs" company="Katana contributors">
//   Copyright 2011-2013 Katana contributors
// </copyright>
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

using System;
using System.Globalization;

namespace Microsoft.AspNet.Razor.Owin
{
    internal static class Requires
    {
        public static void NotNullOrEmpty(string value, string paramName)
        {
            if (String.IsNullOrEmpty(value))
            {
                throw new ArgumentException(
                    String.Format(
                        CultureInfo.CurrentCulture,
                        Resources.Argument_NotNullOrEmpty,
                        paramName),
                    paramName);
            }
        }

        public static void NotNull(object value, string paramName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }

        public static void InRange(bool condition, string paramName)
        {
            if (!condition)
            {
                throw new ArgumentOutOfRangeException(paramName);
            }
        }
    }
}
