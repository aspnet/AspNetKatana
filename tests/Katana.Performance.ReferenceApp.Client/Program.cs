// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Katana.Performance.ReferenceApp.Client
{
    // Use to profile the performance reference app. We use WCAT for actual test runs.
    public class Program
    {
        public static void Main(string[] args)
        {
            Uri uri = new Uri("http://localhost:12345/small-immediate-syncwrite");
            HttpClient client = new HttpClient();
            List<Task> offloads = new List<Task>();

            for (int i = 0; i < 10; i++)
            {
                Task offload = Task.Run(async () =>
                    {
                        for (int j = 0; j < 100000; j++)
                        {
                            HttpResponseMessage response = await client.GetAsync(uri);
                            response.EnsureSuccessStatusCode();
                            response.Dispose();
                        }
                    });
                offloads.Add(offload);
            }

            Task.WaitAll(offloads.ToArray());
        }
    }
}
