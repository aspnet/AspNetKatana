// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reflection;

[assembly: AssemblyCompany("Microsoft Corporation")]
[assembly: AssemblyProduct("Microsoft OWIN")]
[assembly: AssemblyCopyright("\x00a9 Microsoft Corporation All rights reserved.")]
[assembly: AssemblyTrademark("")]
#if NET40 // For servicing the dual compiled projects need different strong name versions
[assembly: AssemblyVersion("2.0.0.400")]
#elif NET45
[assembly: AssemblyVersion("2.0.0.450")]
#else
[assembly: AssemblyVersion("2.0.0.0")]
#endif
[assembly: AssemblyFileVersion("2.0.20815.0")]
[assembly: AssemblyInformationalVersion("2.0.0-rtw-20815-000-dev")]
[assembly: AssemblyMetadata("Serviceable", "True")]
