// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.Owin.Infrastructure;

namespace Microsoft.Owin.Security.Tests
{
    public class TestClock : ISystemClock
    {
        public TestClock()
        {
            UtcNow = new DateTimeOffset(2013, 6, 11, 12, 34, 56, 789, TimeSpan.Zero);
        }

        public DateTimeOffset UtcNow { get; set; }

        public void Add(TimeSpan timeSpan)
        {
            UtcNow = UtcNow + timeSpan;
        }
    }
}