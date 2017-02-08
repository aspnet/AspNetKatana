// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Text;
using Microsoft.Owin.Hosting;

namespace FunctionalTests.Facts.OwinHost
{
    public class MyStartOptions : StartOptions
    {
        const string OPTION_FORMAT = "{0} {1} ";

        public bool ExecuteInCommandLine { get; set; }

        public MyStartOptions(bool executeInCommandLine)
        {
            this.ExecuteInCommandLine = executeInCommandLine;
        }

        public string GetCommandLine()
        {
            StringBuilder commandLineOptions = new StringBuilder();

            commandLineOptions = string.IsNullOrWhiteSpace(ServerFactory) ? commandLineOptions : commandLineOptions.AppendFormat(OPTION_FORMAT, "-s", ServerFactory);

            for (int urlIndex = 0; urlIndex < Urls.Count; urlIndex++)
            {
                commandLineOptions.AppendFormat(OPTION_FORMAT, "-u", Urls[urlIndex]);
            }

            commandLineOptions = !Port.HasValue ? commandLineOptions : commandLineOptions.AppendFormat(OPTION_FORMAT, "-p", Port.Value);
            commandLineOptions = string.IsNullOrWhiteSpace(TraceOutput) ? commandLineOptions : commandLineOptions.AppendFormat(OPTION_FORMAT, "-o", TraceOutput);
            commandLineOptions = string.IsNullOrWhiteSpace(TargetApplicationDirectory) ? commandLineOptions : commandLineOptions.AppendFormat(OPTION_FORMAT, "-d", TargetApplicationDirectory);

            if (Settings.Count > 0 && this.ExecuteInCommandLine)
            {
                StringBuilder inputSettings = new StringBuilder();
                foreach (var kvp in Settings)
                {
                    inputSettings.AppendFormat("{0} = {1}{2}", kvp.Key, kvp.Value, Environment.NewLine);
                }

                var settingsFile = System.Guid.NewGuid().ToString() + ".settings";
                File.WriteAllText(settingsFile, inputSettings.ToString());
                commandLineOptions.AppendFormat(OPTION_FORMAT, "--settings", settingsFile);
            }

            return commandLineOptions.ToString();
        }

        public string TraceOutput { get; set; }

        public string TargetApplicationDirectory { get; set; }

        public bool DontPassStartupClassInCommandLine { get; set; }

        public string FriendlyAppStartupName { get; set; }
    }
}