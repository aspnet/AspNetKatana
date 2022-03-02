// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
            Assert.Equal("UpperCaseValue", value, StringComparer.Ordinal);

            Assert.True(settings.TryGetValue("uppercase", out value));
            Assert.Equal("UpperCaseValue", value, StringComparer.Ordinal);
        }

        [Fact]
        public void LoadOptionsFromConfig_CaseInsensitive()
        {
            var options = new StartOptions();
            SettingsLoader.LoadFromConfig(options.Settings);
            IDictionary<string, string> settings = options.Settings;
            string value;
            Assert.True(settings.TryGetValue("UpperCase", out value));
            Assert.Equal("UpperCaseValue", value, StringComparer.Ordinal);

            Assert.True(settings.TryGetValue("uppercase", out value));
            Assert.Equal("UpperCaseValue", value, StringComparer.Ordinal);
        }

        [Fact]
        public void LoadSettingsFromFile_CaseInsensitive()
        {
            IDictionary<string, string> settings = SettingsLoader.LoadFromSettingsFile("Settings.txt");
            string value;
            Assert.True(settings.TryGetValue("UpperCase", out value));
            Assert.Equal("UpperCaseValue", value, StringComparer.Ordinal);

            Assert.True(settings.TryGetValue("uppercase", out value));
            Assert.Equal("UpperCaseValue", value, StringComparer.Ordinal);
        }

        [Fact]
        public void LoadOptionsFromFile_CaseInsensitive()
        {
            var options = new StartOptions();
            SettingsLoader.LoadFromSettingsFile("Settings.txt", options.Settings);
            IDictionary<string, string> settings = options.Settings;
            string value;
            Assert.True(settings.TryGetValue("UpperCase", out value));
            Assert.Equal("UpperCaseValue", value, StringComparer.Ordinal);

            Assert.True(settings.TryGetValue("uppercase", out value));
            Assert.Equal("UpperCaseValue", value, StringComparer.Ordinal);
        }
    }
}
