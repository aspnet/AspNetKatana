// <copyright file="DefaultCompilationManager.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin.FileSystems;

namespace Microsoft.AspNet.Razor.Owin.Compilation
{
    public class DefaultCompilationManager : ICompilationManager
    {
        private readonly IList<ICompiler> _compilers = new List<ICompiler>()
        {
            new RazorCompiler()
        };

        protected DefaultCompilationManager()
        {
            Cache = new Dictionary<string, WeakReference<Type>>();
        }

        public DefaultCompilationManager(IContentIdentifier identifier) : this()
        {
            Requires.NotNull(identifier, "identifier");

            ContentIdentifier = identifier;
        }

        public IContentIdentifier ContentIdentifier { get; protected set; }

        public IList<ICompiler> Compilers
        {
            get { return _compilers; }
        }

        internal IDictionary<string, WeakReference<Type>> Cache { get; private set; }

        public Task<CompilationResult> Compile(IFileInfo file, ITrace tracer)
        {
            Requires.NotNull(file, "file");
            Requires.NotNull(tracer, "tracer");

            // Generate a content id
            string contentId = ContentIdentifier.GenerateContentId(file);
            tracer.WriteLine("CompilationManager - Content ID: {0}", contentId);

            WeakReference<Type> cacheEntry;
            if (Cache.TryGetValue(contentId, out cacheEntry))
            {
                Type cached;
                if (cacheEntry.TryGetTarget(out cached))
                {
                    return Task.FromResult(CompilationResult.FromCache(cached));
                }
                else
                {
                    tracer.WriteLine("CompilationManager - Expired: {0}", contentId);
                    Cache.Remove(contentId);
                }
            }

            foreach (var compiler in _compilers)
            {
                if (compiler.CanCompile(file))
                {
                    tracer.WriteLine("CompilationManager - Selected compiler: '{0}'", compiler.GetType().Name);
                    return CompileWith(compiler, contentId, file);
                }
            }

            return Task.FromResult(CompilationResult.Failed(null, new[]
            {
                new CompilationMessage(
                    MessageLevel.Error,
                    Resources.DefaultCompilationManager_CannotFindCompiler,
                    new FileLocation(file.PhysicalPath ?? file.Name))
            }));
        }

        private async Task<CompilationResult> CompileWith(ICompiler compiler, string contentId, IFileInfo file)
        {
            CompilationResult result = await compiler.Compile(file);
            if (result.Success)
            {
                Cache[contentId] = new WeakReference<Type>(result.GetCompiledType());
            }
            return result;
        }
    }
}
