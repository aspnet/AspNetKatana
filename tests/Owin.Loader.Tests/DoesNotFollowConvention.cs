// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
