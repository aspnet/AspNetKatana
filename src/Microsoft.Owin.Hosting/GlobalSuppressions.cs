// <copyright file="GlobalSuppressions.cs" company="Katana contributors">
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

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Microsoft.Usage", "CA2243:AttributeStringLiteralsShouldParseCorrectly", Justification = "Version contains prerelease data.")]
[assembly: SuppressMessage("Microsoft.Design", "CA2210:AssembliesShouldHaveValidStrongNames", Justification = "This assembly is delay signed")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "Microsoft.Owin.Hosting.Utilities", Justification = "By design")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "Microsoft.Owin.Hosting.Starter", Justification = "By design")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "Microsoft.Owin.Hosting.Settings", Justification = "By design")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Owin.Loader.DefaultLoader.#.ctor(System.Func`2<System.String,System.Action`1<Owin.IAppBuilder>>)", Justification = "Dependency included by source")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Owin.StartupExtensions.#BuildNew(Owin.IAppBuilder,System.Action`1<Owin.IAppBuilder>)", Justification = "Dependency included by source")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Owin.StartupExtensions.#BuildNew`1(Owin.IAppBuilder,System.Action`1<Owin.IAppBuilder>)", Justification = "Dependency included by source")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Owin.StartupExtensions.#AddSignatureConversion(Owin.IAppBuilder,System.Delegate)", Justification = "Dependency included by source")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Owin.StartupExtensions.#UseFunc`1(Owin.IAppBuilder,System.Func`2<!!0,!!0>)", Justification = "Dependency included by source")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Owin.StartupExtensions.#UseFunc(Owin.IAppBuilder,System.Func`2<System.Func`2<System.Collections.Generic.IDictionary`2<System.String,System.Object>,System.Threading.Tasks.Task>,System.Func`2<System.Collections.Generic.IDictionary`2<System.String,System.Object>,System.Threading.Tasks.Task>>)", Justification = "Dependency included by source")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Owin.StartupExtensions.#UseFunc`1(Owin.IAppBuilder,System.Func`3<System.Func`2<System.Collections.Generic.IDictionary`2<System.String,System.Object>,System.Threading.Tasks.Task>,!!0,System.Func`2<System.Collections.Generic.IDictionary`2<System.String,System.Object>,System.Threading.Tasks.Task>>,!!0)", Justification = "Dependency included by source")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Owin.StartupExtensions.#UseFunc`2(Owin.IAppBuilder,System.Func`4<System.Func`2<System.Collections.Generic.IDictionary`2<System.String,System.Object>,System.Threading.Tasks.Task>,!!0,!!1,System.Func`2<System.Collections.Generic.IDictionary`2<System.String,System.Object>,System.Threading.Tasks.Task>>,!!0,!!1)", Justification = "Dependency included by source")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Owin.StartupExtensions.#UseFunc`3(Owin.IAppBuilder,System.Func`5<System.Func`2<System.Collections.Generic.IDictionary`2<System.String,System.Object>,System.Threading.Tasks.Task>,!!0,!!1,!!2,System.Func`2<System.Collections.Generic.IDictionary`2<System.String,System.Object>,System.Threading.Tasks.Task>>,!!0,!!1,!!2)", Justification = "Dependency included by source")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Owin.StartupExtensions.#UseFunc`4(Owin.IAppBuilder,System.Func`6<System.Func`2<System.Collections.Generic.IDictionary`2<System.String,System.Object>,System.Threading.Tasks.Task>,!!0,!!1,!!2,!!3,System.Func`2<System.Collections.Generic.IDictionary`2<System.String,System.Object>,System.Threading.Tasks.Task>>,!!0,!!1,!!2,!!3)", Justification = "Dependency included by source")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Owin.StartupExtensions.#UseFunc`1(Owin.IAppBuilder,System.Func`2<!!0,System.Func`2<System.Func`2<System.Collections.Generic.IDictionary`2<System.String,System.Object>,System.Threading.Tasks.Task>,System.Func`2<System.Collections.Generic.IDictionary`2<System.String,System.Object>,System.Threading.Tasks.Task>>>,!!0)", Justification = "Dependency included by source")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Owin.StartupExtensions.#UseFunc`2(Owin.IAppBuilder,System.Func`3<!!0,!!1,System.Func`2<System.Func`2<System.Collections.Generic.IDictionary`2<System.String,System.Object>,System.Threading.Tasks.Task>,System.Func`2<System.Collections.Generic.IDictionary`2<System.String,System.Object>,System.Threading.Tasks.Task>>>,!!0,!!1)", Justification = "Dependency included by source")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Owin.StartupExtensions.#UseFunc`3(Owin.IAppBuilder,System.Func`4<!!0,!!1,!!2,System.Func`2<System.Func`2<System.Collections.Generic.IDictionary`2<System.String,System.Object>,System.Threading.Tasks.Task>,System.Func`2<System.Collections.Generic.IDictionary`2<System.String,System.Object>,System.Threading.Tasks.Task>>>,!!0,!!1,!!2)", Justification = "Dependency included by source")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Owin.StartupExtensions.#UseFunc`4(Owin.IAppBuilder,System.Func`5<!!0,!!1,!!2,!!3,System.Func`2<System.Func`2<System.Collections.Generic.IDictionary`2<System.String,System.Object>,System.Threading.Tasks.Task>,System.Func`2<System.Collections.Generic.IDictionary`2<System.String,System.Object>,System.Threading.Tasks.Task>>>,!!0,!!1,!!2,!!3)", Justification = "Dependency included by source")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Owin.StartupExtensions.#UseType(Owin.IAppBuilder,System.Type,System.Object[])", Justification = "Dependency included by source")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "Microsoft.Owin.Hosting.Builder", Justification = "By design")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "Microsoft.Owin.Hosting.Services", Justification = "By design")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "Microsoft.Owin.Hosting.Tracing", Justification = "By design")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "Microsoft.Owin.Hosting.Loader", Justification = "By design")]
