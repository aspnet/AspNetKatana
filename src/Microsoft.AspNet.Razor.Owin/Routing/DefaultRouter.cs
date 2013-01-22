// <copyright file="DefaultRouter.cs" company="Katana contributors">
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
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Gate;
using Microsoft.AspNet.Razor.Owin.IO;

namespace Microsoft.AspNet.Razor.Owin.Routing
{
    public class DefaultRouter : IRouter
    {
        private readonly HashSet<string> _knownExtensions = new HashSet<string>(new string[]
        {
            ".cshtml"
        }, StringComparer.OrdinalIgnoreCase);

        private readonly HashSet<string> _defaultDocumentNames = new HashSet<string>(new string[]
        {
            "Default",
            "Index"
        }, StringComparer.OrdinalIgnoreCase);

        protected DefaultRouter()
        {
        }

        public IFileSystem FileSystem { get; protected set; }

        public ISet<string> KnownExtensions
        {
            get { return _knownExtensions; }
        }

        public ISet<string> DefaultDocumentNames
        {
            get { return _defaultDocumentNames; }
        }

        public DefaultRouter(IFileSystem fileSystem)
        {
            Requires.NotNull(fileSystem, "fileSystem");

            FileSystem = fileSystem;
        }

        public Task<RouteResult> Route(Request request, ITrace tracer)
        {
            Requires.NotNull(request, "request");
            Requires.NotNull(tracer, "tracer");

            // This is so slooooow!
            IFile file;
            string[] pathFragments = request.Path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            for (int end = pathFragments.Length - 1; end >= 0; end--)
            {
                Tuple<string, string> candidate = CreateCandidate(pathFragments, end);
                file = ResolveCandidate(candidate.Item1.Replace('/', Path.DirectorySeparatorChar));
                if (file != null)
                {
                    return Task.FromResult(RouteResult.Successful(file, candidate.Item2));
                }
            }
            file = ResolveCandidate(String.Empty);
            if (file != null)
            {
                return Task.FromResult(RouteResult.Successful(file, request.Path.TrimStart('/')));
            }
            else
            {
                return Task.FromResult(RouteResult.Failed());
            }
        }

        private IFile ResolveCandidate(string physicalPath)
        {
            foreach (var extension in KnownExtensions)
            {
                IFile file = FileSystem.GetFile(physicalPath + extension);
                if (file.Exists)
                {
                    return file;
                }
                else
                {
                    // Try "[name]/Default.cshtml"
                    foreach (var docNames in DefaultDocumentNames)
                    {
                        file = FileSystem.GetFile(Path.Combine(physicalPath, docNames + extension));
                        if (file.Exists)
                        {
                            return file;
                        }
                    }
                }
            }
            return null;
        }

        private static Tuple<string, string> CreateCandidate(string[] pathFragments, int end)
        {
            // TODO: Shortcuts, precalcuate string lengths, etc.
            var pathBuilder = new StringBuilder();
            var dataBuilder = new StringBuilder();
            for (int i = 0; i < pathFragments.Length; i++)
            {
                if (i > 0 && i < end + 1)
                {
                    pathBuilder.Append("/");
                }
                else if (i > end + 1)
                {
                    dataBuilder.Append("/");
                }

                if (i <= end)
                {
                    pathBuilder.Append(pathFragments[i]);
                }
                else
                {
                    dataBuilder.Append(pathFragments[i]);
                }
            }
            return Tuple.Create(pathBuilder.ToString(), dataBuilder.ToString());
        }
    }
}
