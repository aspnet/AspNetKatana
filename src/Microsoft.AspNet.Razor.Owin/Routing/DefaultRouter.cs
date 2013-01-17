// -----------------------------------------------------------------------
// <copyright file="DefaultRouter.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gate;
using Microsoft.AspNet.Razor.Owin;
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
            foreach (string extension in KnownExtensions)
            {
                IFile file = FileSystem.GetFile(physicalPath + extension);
                if (file.Exists)
                {
                    return file;
                }
                else
                {
                    // Try "[name]/Default.cshtml"
                    foreach (string docNames in DefaultDocumentNames)
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
            StringBuilder pathBuilder = new StringBuilder();
            StringBuilder dataBuilder = new StringBuilder();
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
