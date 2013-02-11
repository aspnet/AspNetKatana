// <copyright file="RazorCompiler.cs" company="Microsoft Open Technologies, Inc.">
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
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Razor;
using System.Web.Razor.Generator;
using Microsoft.AspNet.Razor.Owin.IO;
using Microsoft.CSharp;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;
using CSCompilation = Roslyn.Compilers.CSharp.Compilation;

namespace Microsoft.AspNet.Razor.Owin.Compilation
{
    public class RazorCompiler : ICompiler
    {
        private static readonly Regex InvalidClassNameChars = new Regex("[^A-Za-z0-9_]");

        private static readonly Dictionary<DiagnosticSeverity, MessageLevel> SeverityMap = new Dictionary<DiagnosticSeverity, MessageLevel>()
        {
            { DiagnosticSeverity.Error, MessageLevel.Error },
            { DiagnosticSeverity.Info, MessageLevel.Info },
            { DiagnosticSeverity.Warning, MessageLevel.Warning }
        };

        public bool CanCompile(IFile file)
        {
            return String.Equals(file.Extension, ".cshtml");
        }

        public Task<CompilationResult> Compile(IFile file)
        {
            string className = MakeClassName(file.Name);
            var engine = new RazorTemplateEngine(new RazorEngineHost(new CSharpRazorCodeLanguage())
            {
                DefaultBaseClass = "Microsoft.AspNet.Razor.Owin.PageBase",
                GeneratedClassContext = new GeneratedClassContext(
                    executeMethodName: "Execute",
                    writeMethodName: "Write",
                    writeLiteralMethodName: "WriteLiteral",
                    writeToMethodName: "WriteTo",
                    writeLiteralToMethodName: "WriteLiteralTo",
                    templateTypeName: "Template",
                    defineSectionMethodName: "DefineSection")
                {
                    ResolveUrlMethodName = "Href"
                }
            });
            engine.Host.NamespaceImports.Add("System");
            engine.Host.NamespaceImports.Add("System.Linq");
            engine.Host.NamespaceImports.Add("System.Collections.Generic");

            GeneratorResults results;
            using (TextReader rdr = file.OpenRead())
            {
                results = engine.GenerateCode(rdr, className, "RazorCompiled", file.FullPath);
            }

            var messages = new List<CompilationMessage>();
            if (!results.Success)
            {
                foreach (var error in results.ParserErrors)
                {
                    messages.Add(new CompilationMessage(
                        MessageLevel.Error,
                        error.Message,
                        new FileLocation(file.FullPath, error.Location.LineIndex, error.Location.CharacterIndex)));
                }
            }

            // Regardless of success or failure, we're going to try and compile
            return Task.FromResult(CompileCSharp("RazorCompiled." + className, file, results.Success, messages, results.GeneratedCode));
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "Partial trust not supported")]
        private CompilationResult CompileCSharp(string fullClassName, IFile file, bool success, List<CompilationMessage> messages, CodeCompileUnit codeCompileUnit)
        {
            // Generate code text
            var code = new StringBuilder();
            using (var provider = new CSharpCodeProvider())
            {
                using (var writer = new StringWriter(code))
                {
                    provider.GenerateCodeFromCompileUnit(codeCompileUnit, writer, new CodeGeneratorOptions());
                }
            }

            // Parse
            SyntaxTree tree = SyntaxTree.ParseText /*.ParseCompilationUnit*/(code.ToString(), "__Generated.cs");

            // Create a compilation
            CSCompilation comp = CSCompilation.Create(
                "Compiled",
                new CompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                syntaxTrees: new[] { tree },
                references: new[]
                {
                    new MetadataFileReference(typeof(object).Assembly.Location),
                    new MetadataFileReference(typeof(Enumerable).Assembly.Location),
                    new MetadataFileReference(typeof(PageBase).Assembly.Location),
                    new MetadataFileReference(typeof(Gate.Request).Assembly.Location)
                });

            // Emit to a collectable assembly
            AssemblyBuilder asm = AssemblyBuilder.DefineDynamicAssembly(
                new AssemblyName("Razor_" + Guid.NewGuid().ToString("N")), AssemblyBuilderAccess.RunAndCollect);
            ModuleBuilder mod = asm.DefineDynamicModule("RazorCompilation");
            ReflectionEmitResult result = comp.Emit(mod);

            // Extract the type
            Type typ = null;
            if (result.Success)
            {
                typ = asm.GetType(fullClassName);
            }
            else
            {
                foreach (var diagnostic in result.Diagnostics)
                {
                    FileLinePositionSpan span = diagnostic.Location.GetLineSpan(true);
                    LinePosition linePosition = span.StartLinePosition;
                    messages.Add(new CompilationMessage(
                        SeverityMap[diagnostic.Info.Severity],
                        diagnostic.Info.GetMessage(),
                        new FileLocation(
                            span.Path,
                            linePosition.Line,
                            linePosition.Character,
                            String.Equals(span.Path, "__Generated.cs", StringComparison.OrdinalIgnoreCase))));
                }
            }

            // Create a compilation result
            if (success && result.Success)
            {
                return CompilationResult.Successful(code.ToString(), typ, messages);
            }
            return CompilationResult.Failed(code.ToString(), messages);
        }

        private string MakeClassName(string fileName)
        {
            return "_" + InvalidClassNameChars.Replace(fileName, String.Empty);
        }
    }
}
