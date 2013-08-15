// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DifferentNamespace;
using Microsoft.Owin.Builder;
using Xunit;

[assembly: Owin.Loader.Tests.DefaultConfigurationLoaderTests.OwinStartup("AlternateStartupAttribute", typeof(Owin.Loader.Tests.DefaultConfigurationLoaderTests), "Hello")]

namespace Owin.Loader.Tests
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class DefaultConfigurationLoaderTests
    {
        private static int _helloCalls;
        private static int _alphaCalls;

        [Fact]
        public void Strings_are_split_based_on_dots()
        {
            string[] strings = DefaultLoader.DotByDot("this.is.a.test").ToArray();
            Assert.Equal(4, strings.Length);
            Assert.Equal("this.is.a.test", strings[0]);
            Assert.Equal("this.is.a", strings[1]);
            Assert.Equal("this.is", strings[2]);
            Assert.Equal("this", strings[3]);
        }

        [Fact]
        public void Leading_and_trailing_dot_and_empty_strings_are_safe_and_ignored()
        {
            string[] string1 = DefaultLoader.DotByDot(".a.test").ToArray();
            string[] string2 = DefaultLoader.DotByDot("a.test.").ToArray();
            string[] string3 = DefaultLoader.DotByDot(".a.test.").ToArray();
            string[] string4 = DefaultLoader.DotByDot(".").ToArray();
            string[] string5 = DefaultLoader.DotByDot(string.Empty).ToArray();
            string[] string6 = DefaultLoader.DotByDot(null).ToArray();

            AssertArrayEqual(new[] { "a.test", "a" }, string1);
            AssertArrayEqual(new[] { "a.test", "a" }, string2);
            AssertArrayEqual(new[] { "a.test", "a" }, string3);
            AssertArrayEqual(new string[0], string4);
            AssertArrayEqual(new string[0], string5);
            AssertArrayEqual(new string[0], string6);
        }

        private void AssertArrayEqual(string[] expected, string[] actual)
        {
            Assert.Equal(expected.Length, actual.Length);
            foreach (var index in Enumerable.Range(0, actual.Length))
            {
                Assert.Equal(expected[index], actual[index]);
            }
        }

        public static void Hello(IAppBuilder builder)
        {
            _helloCalls += 1;
        }

        [Fact]
        public void Load_will_find_assembly_and_type_and_static_method()
        {
            IList<string> errors = new List<string>();
            var loader = new DefaultLoader();
            Action<IAppBuilder> configuration = loader.Load("Owin.Loader.Tests.DefaultConfigurationLoaderTests.Hello", errors);

            _helloCalls = 0;
            configuration(new AppBuilder());
            Assert.Equal(1, _helloCalls);
        }

        [Fact]
        public void An_extra_segment_will_cause_the_match_to_fail()
        {
            var loader = new DefaultLoader();
            IList<string> errors = new List<string>();
            Action<IAppBuilder> configuration = loader.Load("Owin.Loader.Tests.DefaultConfigurationLoaderTests.Hello.Bar", errors);

            Assert.Null(configuration);
            Assert.NotEmpty(errors);
        }

        [Fact]
        public void Calling_a_class_with_multiple_configs_is_okay()
        {
            IList<string> errors = new List<string>();
            var loader = new DefaultLoader();
            Action<IAppBuilder> foo = loader.Load("Owin.Loader.Tests.DefaultConfigurationLoaderTests+MultiConfigs.Foo", errors);
            Action<IAppBuilder> bar = loader.Load("Owin.Loader.Tests.DefaultConfigurationLoaderTests+MultiConfigs.Bar", errors);

            MultiConfigs.FooCalls = 0;
            MultiConfigs.BarCalls = 0;

            Assert.NotNull(foo);
            Assert.NotNull(bar);

            foo(new AppBuilder());

            Assert.Equal(1, MultiConfigs.FooCalls);
            Assert.Equal(0, MultiConfigs.BarCalls);

            bar(new AppBuilder());

            Assert.Equal(1, MultiConfigs.FooCalls);
            Assert.Equal(1, MultiConfigs.BarCalls);
        }

        [Fact]
        public void Configuration_method_defaults_to_Configuration_if_only_type_name_is_provided()
        {
            IList<string> errors = new List<string>();
            var loader = new DefaultLoader();
            Action<IAppBuilder> configuration = loader.Load("Owin.Loader.Tests.DefaultConfigurationLoaderTests+MultiConfigs", errors);

            MultiConfigs.FooCalls = 0;
            MultiConfigs.BarCalls = 0;
            MultiConfigs.ConfigurationCalls = 0;

            configuration(new AppBuilder());

            Assert.Equal(0, MultiConfigs.FooCalls);
            Assert.Equal(0, MultiConfigs.BarCalls);
            Assert.Equal(1, MultiConfigs.ConfigurationCalls);
        }

        [Fact]
        public void Comma_may_be_used_if_assembly_name_doesnt_match_namespace()
        {
            IList<string> errors = new List<string>();
            var loader = new DefaultLoader();
            Action<IAppBuilder> configuration = loader.Load("DifferentNamespace.DoesNotFollowConvention, Owin.Loader.Tests", errors);
            Action<IAppBuilder> alternateConfiguration = loader.Load("DifferentNamespace.DoesNotFollowConvention.AlternateConfiguration, Owin.Loader.Tests", errors);

            DoesNotFollowConvention.ConfigurationCalls = 0;
            DoesNotFollowConvention.AlternateConfigurationCalls = 0;

            configuration(new AppBuilder());
            Assert.Equal(1, DoesNotFollowConvention.ConfigurationCalls);
            Assert.Equal(0, DoesNotFollowConvention.AlternateConfigurationCalls);
            alternateConfiguration(new AppBuilder());
            Assert.Equal(1, DoesNotFollowConvention.ConfigurationCalls);
            Assert.Equal(1, DoesNotFollowConvention.AlternateConfigurationCalls);
        }

        public static AppFunc Alpha(IDictionary<string, object> properties)
        {
            return env =>
            {
                ++_alphaCalls;
                return GetCompletedTask();
            };
        }

        private static Task GetCompletedTask()
        {
            var tcs = new TaskCompletionSource<object>();
            tcs.TrySetResult(null);
            return tcs.Task;
        }

        [Fact]
        public void Method_that_returns_app_action_may_also_be_called()
        {
            IList<string> errors = new List<string>();
            var loader = new DefaultLoader();
            Action<IAppBuilder> configuration = loader.Load("Owin.Loader.Tests.DefaultConfigurationLoaderTests.Alpha", errors);

            var builder = new AppBuilder();
            configuration(builder);
            var app = builder.Build<AppFunc>();

            _alphaCalls = 0;
            app(new Dictionary<string, object>());
            Assert.Equal(1, _alphaCalls);
        }

        [Fact]
        public void Startup_Configuration_Attribute_will_be_discovered_by_default()
        {
            IList<string> errors = new List<string>();
            var loader = new DefaultLoader();
            Action<IAppBuilder> configuration = loader.Load(string.Empty, errors);
            Startup.ConfigurationCalls = 0;
            configuration(new AppBuilder());
            Assert.Equal(1, Startup.ConfigurationCalls);

            configuration = loader.Load(null, errors);
            Startup.ConfigurationCalls = 0;
            configuration(new AppBuilder());
            Assert.Equal(1, Startup.ConfigurationCalls);
        }

        [Fact]
        public void Friendly_Name_Startup_Configuration_Attribute_will_be_discovered()
        {
            IList<string> errors = new List<string>();
            var loader = new DefaultLoader();
            Action<IAppBuilder> configuration = loader.Load("AFriendlyName", errors);
            Startup.ConfigurationCalls = 0;
            configuration(new AppBuilder());
            Assert.Equal(1, Startup.ConfigurationCalls);
        }

        [Fact]
        public void Friendly_Name_Used_To_Find_Alternate_Config_Method()
        {
            IList<string> errors = new List<string>();
            var loader = new DefaultLoader();
            Action<IAppBuilder> configuration = loader.Load("AlternateConfiguration", errors);
            Startup.AlternateConfigurationCalls = 0;
            configuration(new AppBuilder());
            Assert.Equal(1, Startup.AlternateConfigurationCalls);
        }

        [Fact]
        public void Different_OwinStartupAttribute_Definition_Works()
        {
            IList<string> errors = new List<string>();
            var loader = new DefaultLoader();
            Action<IAppBuilder> configuration = loader.Load("AlternateStartupAttribute", errors);
            _helloCalls = 0;
            configuration(new AppBuilder());
            Assert.Equal(1, _helloCalls);
        }

        public class MultiConfigs
        {
            public static int FooCalls;
            public static int BarCalls;
            public static int ConfigurationCalls;
            public static int OtherConfigurationCalls;

            public static void Foo(IAppBuilder builder)
            {
                FooCalls += 1;
            }

            public static void Bar(IAppBuilder builder)
            {
                BarCalls += 1;
            }

            public static void Configuration(IAppBuilder builder)
            {
                ConfigurationCalls += 1;
            }

            public static object Configuration(IDictionary<string, object> properties)
            {
                OtherConfigurationCalls += 1;
                return new object();
            }
        }

        // Alternate definition, used to confirm that there is not a direct type dependency.
        [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
        internal sealed class OwinStartupAttribute : Attribute
        {
            public OwinStartupAttribute(string friendlyName, Type startupType, string methodName)
            {
                FriendlyName = friendlyName;
                StartupType = startupType;
                MethodName = methodName;
            }

            public Type StartupType { get; private set; }

            public string FriendlyName { get; private set; }

            public string MethodName { get; private set; }
        }
    }
}
