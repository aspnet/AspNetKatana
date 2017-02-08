// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Owin;

namespace DifferentNamespace
{
    public class DoesNotFollowConvention
    {
        public static int ConfigurationCalls;
        public static int AlternateConfigurationCalls;

        public static void Configuration(IAppBuilder builder)
        {
            ConfigurationCalls += 1;
        }

        public static void AlternateConfiguration(IAppBuilder builder)
        {
            AlternateConfigurationCalls += 1;
        }
    }
}
