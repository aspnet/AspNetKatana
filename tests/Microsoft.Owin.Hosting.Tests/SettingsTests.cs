// <copyright file="SettingsTests.cs" company="Microsoft Open Technologies, Inc.">
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
using Microsoft.Owin.Hosting.Utilities;
using Xunit;

namespace Microsoft.Owin.Hosting.Tests
{
    public class SettingsTests
    {
        [Fact]
        public void LoadSettingsFromConfig_CaseInsensitive()
        {
            IDictionary<string, string> settings = SettingsLoader.LoadFromConfig();
            string value;
            Assert.True(settings.TryGetValue("UpperCase", out value));
            Assert.True(string.Equals("UpperCaseValue", value, StringComparison.Ordinal));

            Assert.True(settings.TryGetValue("uppercase", out value));
            Assert.True(string.Equals("UpperCaseValue", value, StringComparison.Ordinal));
        }

        [Fact]
        public void LoadOptionsFromConfig_CaseInsensitive()
        {
            StartOptions options = new StartOptions();
            SettingsLoader.LoadFromConfig(options.Settings);
            IDictionary<string, string> settings = options.Settings;
            string value;
            Assert.True(settings.TryGetValue("UpperCase", out value));
            Assert.True(string.Equals("UpperCaseValue", value, StringComparison.Ordinal));

            Assert.True(settings.TryGetValue("uppercase", out value));
            Assert.True(string.Equals("UpperCaseValue", value, StringComparison.Ordinal));
        }

        [Fact]
        public void LoadSettingsFromFile_CaseInsensitive()
        {
            IDictionary<string, string> settings = SettingsLoader.LoadFromSettingsFile("Settings.txt");
            string value;
            Assert.True(settings.TryGetValue("UpperCase", out value));
            Assert.True(string.Equals("UpperCaseValue", value, StringComparison.Ordinal));

            Assert.True(settings.TryGetValue("uppercase", out value));
            Assert.True(string.Equals("UpperCaseValue", value, StringComparison.Ordinal));
        }

        [Fact]
        public void LoadOptionsFromFile_CaseInsensitive()
        {
            StartOptions options = new StartOptions();
            SettingsLoader.LoadFromSettingsFile("Settings.txt", options.Settings);
            IDictionary<string, string> settings = options.Settings;
            string value;
            Assert.True(settings.TryGetValue("UpperCase", out value));
            Assert.True(string.Equals("UpperCaseValue", value, StringComparison.Ordinal));

            Assert.True(settings.TryGetValue("uppercase", out value));
            Assert.True(string.Equals("UpperCaseValue", value, StringComparison.Ordinal));
        }
    }
}
