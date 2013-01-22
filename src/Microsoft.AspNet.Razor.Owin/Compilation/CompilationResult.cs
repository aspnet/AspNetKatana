// <copyright file="CompilationResult.cs" company="Katana contributors">
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
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNet.Razor.Owin.Compilation
{
    public class CompilationResult
    {
        private readonly Type _type;

        private CompilationResult(bool success, string generatedCode, IList<CompilationMessage> messages, Type typ, bool fromCache)
        {
            Success = success;
            GeneratedCode = generatedCode;
            Messages = messages;
            SatisfiedFromCache = fromCache;
            _type = typ;
        }

        public bool Success { get; private set; }
        public bool SatisfiedFromCache { get; private set; }
        public IList<CompilationMessage> Messages { get; private set; }
        public string GeneratedCode { get; private set; }

        public Type GetCompiledType()
        {
            if (_type == null)
            {
                throw new InvalidOperationException("Compilation Failed. There is no compiled Type");
            }
            return _type;
        }

        public static CompilationResult Failed(string generatedCode, IEnumerable<CompilationMessage> messages)
        {
            return new CompilationResult(false, generatedCode, messages.ToList(), null, fromCache: false);
        }

        public static CompilationResult FromCache(Type typ)
        {
            return new CompilationResult(true, null, new List<CompilationMessage>(), typ, fromCache: true);
        }

        public static CompilationResult Successful(string generatedCode, Type typ, IEnumerable<CompilationMessage> messages)
        {
            return new CompilationResult(true, generatedCode, messages.ToList(), typ, fromCache: false);
        }
    }
}
