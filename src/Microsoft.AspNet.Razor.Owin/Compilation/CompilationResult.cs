// -----------------------------------------------------------------------
// <copyright file="CompilationResult.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNet.Razor.Owin.Execution;

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
