// <copyright file="AppBuilderTests.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2013 Microsoft Open Technologies, Inc. All rights reserved.
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
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Builder;
using Owin;
using Shouldly;
using Xunit;

namespace Microsoft.Owin.Builder.Tests
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class AppBuilderTests
    {
        public delegate string AppOne(string call);
        public delegate string AppTwo(string call);

        [Fact]
        public void DelegateShouldBeCalledToAddMiddlewareAroundTheDefaultApp()
        {
            var theNext = "next";
            var theMiddle = "middle";
            var theDefault = "default";

            Func<string, string> middleware = next =>
            {
                theNext = next;
                return theMiddle;
            };

            var builder = new AppBuilder();
            builder.Properties["builder.DefaultApp"] = theDefault;
            var theApp = builder.BuildNew<string>(x => x.Use(middleware));

            builder.Run(theApp);

            theNext.ShouldBeSameAs(theDefault);
            theApp.ShouldBeSameAs(theMiddle);
            theApp.ShouldNotBeSameAs(theDefault);
        }

        [Fact]
        public void ConversionShouldBeCalledBetweenDifferentSignatures()
        {
            object theDefault = "42";

            Func<string, int> convert1 = app => int.Parse(app, CultureInfo.InvariantCulture) + 1;
            Func<int, string> convert2 = app => app.ToString(CultureInfo.InvariantCulture) + "2";

            Func<string, string> middleware1 = app => app + "3";
            Func<int, int> middleware2 = app => app + 4;

            var builder = new AppBuilder();
            builder.AddSignatureConversion(convert1);
            builder.AddSignatureConversion(convert2);
            builder.Properties["builder.DefaultApp"] = theDefault;

            var theApp = builder.BuildNew<int>(x => x.Use(middleware1).Use(middleware2));

            // "42" + 1: 43         // theDefault passed through convert1 for next middleware
            // 43 + 4: 47           // passed through middleware2
            // 47 + "2": "472"      // passed through convert2 for next middleware
            // "472" + "3": "4723"  // passed through middleware1
            // "4723" + 1: 4724     // passed through convert1 to return

            theApp.ShouldBe(4724);
        }

        [Fact]
        public void InstanceMemberNamedInvokeShouldQualifyAsMiddlewareFactory()
        {
            Func<int, string> theDefault = call => "Hello[" + call + "]";

            var builder = new AppBuilder();
            builder.Properties["builder.DefaultApp"] = theDefault;

            var theApp = builder.BuildNew<Func<int, string>>(
                x => x
                    .Use(new StringPlusValue(" world!"))
                    .Use(new StringPlusValue(" there,")));

            theApp(42).ShouldBe("Hello[42] there, world!");
        }

        [Fact]
        public void DelegateShouldQualifyAsAppWithRun()
        {
            Func<int, string> theDefault = call => "Hello[" + call + "]";
            Func<int, string> theSite = call => "Called[" + call + "]";

            var builder = new AppBuilder();
            builder.Properties["builder.DefaultApp"] = theDefault;

            var theApp = builder.BuildNew<Func<int, string>>(x => x.Run(theSite));

            theApp(42).ShouldBe("Called[42]");
        }

        [Fact]
        public void InstanceMemberNamedInvokeShouldQualifyAsAppWithRun()
        {
            var theSite = new MySite();

            var builder = new AppBuilder();

            var theApp = builder.BuildNew<Func<int, string>>(x => x.Run(theSite));

            theApp(42).ShouldBe("Called[42]");
        }

        [Fact]
        public void TypeofClassConstructorsShouldQualifyAsMiddlewareFactory()
        {
            Func<int, string> theDefault = call => "Hello[" + call + "]";

            var builder = new AppBuilder();
            builder.Properties["builder.DefaultApp"] = theDefault;

            var theApp = builder.BuildNew<Func<int, string>>(
                x => x
                    .Use(typeof(StringPlusValue2), " world!")
                    .Use(typeof(StringPlusValue2), " there,"));

            theApp(42).ShouldBe("Hello[42] there, world!");
        }

        [Fact]
        public void TypeofClassConstructorsWithWrongParameterCountShouldThrow()
        {
            Func<int, string> theDefault = call => "Hello[" + call + "]";

            var builder = new AppBuilder();
            builder.Properties["builder.DefaultApp"] = theDefault;

            Should.Throw<MissingMethodException>(() =>
            {
                var theApp = builder.BuildNew<Func<int, string>>(
                x => x.Use(typeof(StringPlusValue2), "arg 1", "extra arg"));
            });
        }

        [Fact]
        public void OtherObjectShouldThrow()
        {
            Func<int, string> theDefault = call => "Hello[" + call + "]";

            var builder = new AppBuilder();
            builder.Properties["builder.DefaultApp"] = theDefault;

            Should.Throw<NotSupportedException>(() =>
            {
                var theApp = builder.BuildNew<Func<int, string>>(
                x => x.Use(new object()));
            });
        }

        [Fact]
        public void DelegatesWithIdenticalParametersShouldConvertAutomatically()
        {
            var builder = new AppBuilder();
            builder.Properties["builder.DefaultApp"] = new Func<string, string>(call => call);
            builder.UseFunc<AppOne>(next => call => next(call) + "1");
            builder.UseFunc<AppTwo>(next => call => next(call) + "2");
            builder.UseFunc<Func<string, string>>(next => call => next(call) + "3");
            var app = builder.Build<AppTwo>();
            app("0").ShouldBe("0321");
        }

        [Fact]
        public Task TheDefaultDefaultShouldBe404()
        {
            var builder = new AppBuilder();
            var app = builder.Build();

            var request = OwinRequest.Create();
            var response = new OwinResponse(request);
            return app(request.Environment).Then(() => response.StatusCode.ShouldBe(404));
        }

        [Fact]
        public void ConverterCombinationWillBeInvokedIfNeeded()
        {
            var builder = new AppBuilder();
            Func<AppFunc, DifferentType> convert1 = _ => new DifferentType();
            Func<DifferentType, AppFunc> convert2 = _ => call => { throw new NotImplementedException(); };
            builder.AddSignatureConversion(convert1);
            builder.AddSignatureConversion(convert2);

            var diff = builder.Build<DifferentType>();
        }

        [Fact]
        public void NoConversionAvailableThrowsArgumentException()
        {
            var builder = new AppBuilder();
            Assert.Throws<ArgumentException>(() => builder.Build<DifferentType>());
        }

        private class StringPlusValue
        {
            private readonly string _value;

            public StringPlusValue(string value)
            {
                _value = value;
            }

            public Func<int, string> Invoke(Func<int, string> app)
            {
                return call => app(call) + _value;
            }
        }

        private class StringPlusValue2
        {
            private readonly Func<int, string> _app;
            private readonly string _value;

            public StringPlusValue2(Func<int, string> app)
            {
                _app = app;
                _value = " PlusPlus";
            }

            public StringPlusValue2(Func<int, string> app, string value)
            {
                _app = app;
                _value = value;
            }

            public string Invoke(int call)
            {
                return _app(call) + _value;
            }
        }

        public class DifferentType
        {
        }

        public class MySite
        {
            public string Invoke(int call)
            {
                return "Called[" + call + "]";
            }
        }
    }
}