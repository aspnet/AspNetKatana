// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Owin;
using Xunit;
using Xunit.Extensions;

namespace Microsoft.Owin.Host.IntegrationTests
{
    public class PathEscapingTests : TestBase
    {
        public void EchoPath(IAppBuilder app)
        {
            app.Run(context =>
            {
                PathString path = context.Request.Path;
                byte[] pathBytes = Encoding.UTF8.GetBytes(path.Value);
                string encodedPath = Convert.ToBase64String(pathBytes);
                byte[] wireBytes = Encoding.ASCII.GetBytes(encodedPath);
                context.Response.ContentLength = wireBytes.Length;
                context.Response.Write(encodedPath);
                context.Response.Body.Flush();
                return Task.FromResult(0);
            });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        public void VerifyUnescapedBackslashConverted(string serverName)
        {
            // Note: Http.Sys cooked url makes this backslash a forward slash.
            RunTest(serverName, "/extra%5Cslash/", "/extra/slash/");
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        public void VerifyUnescapedUnicode(string serverName)
        {
            RunTest(serverName, "/%E8%91%89/", "/葉/");
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public void VerifyUnescapedPercent(string serverName)
        {
            RunTest(serverName, "/%2541/", "/%41/");
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public void VerifySelfHostUnescapedCharacters(string serverName)
        {
            // Error code comments refer to IIS/Asp.Net restrictions.
            // Commented-out lines without error codes are covered in a separate test.
            // Commented-out lines with error codes have the same error on HttpListener.
            string inputPath = "/"
                // + "%01%02%03%04%05%06%07%08%09%0A%0B%0C%0D%0E%0F%10%11%12%13%14%15%16%17%18%19%1A%1B%1C%1D%1E%1F" // 400
                // + "%20%21%22%23%24" // SP!"#$
                + "%25" // % 400
                + "%26" // & 400
                // + "%27%28%29" // '()
                + "%2A" // * 400
                + "%2B" // + 404
                // + "%2C%2D%2E%2F" // ,-./
                // "%30%31%32%33%34%35%36%37%38%39" // 0-9
                + "%3A" // : 400
                // + "%3B" // ;
                + "%3C" // < 400
                // + "%3D" // =
                + "%3E%3F" // >? 400
                // + "%40" // @
                // + "%41%42%43%44%45%46%47%48%49%4A%4B%4C%4D%4E%4F%50%51%52%53%54%55%56%57%58%59%5A" // A-Z
                // + "%5B" // [
                // + "%5C" // \ Asp.Net changes this to /
                // + "%5D%5E%5F%60" // ]^_`
                // + "%61%62%63%64%65%66%67%68%69%6A%6B%6C%6D%6E%6F%70%71%72%73%74%75%76%77%78%79%7A" // a-z
                // + "%7B%7C%7D%7E" // {|}~
                // + "%7F" 400
                + string.Empty;

            string expected = "/%&*+:<>?";

            RunTest(serverName, inputPath, expected);
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public void VerifyUnescapedCharacters(string serverName)
        {
            // . moved to be not adjacent to / because System.Uri 4.0 would truncate it on the client before sending the request.
            // Error code comments refer to IIS/Asp.Net restrictions.
            string inputPath = "/"
                // + "%01%02%03%04%05%06%07%08%09%0A%0B%0C%0D%0E%0F%10%11%12%13%14%15%16%17%18%19%1A%1B%1C%1D%1E%1F" // 400
                + "%20%21%22%23%24" // SP!"#$
                // + "%25" // % 400
                // + "%26" // & 400
                + "%27%28%29" // '()
                // + "%2A" // * 400
                // + "%2B" // + 404
                + "%2C%2E%2D%2F" // ,.-/
                + "%30%31%32%33%34%35%36%37%38%39" // 0-9
                // + "%3A" // : 400
                + "%3B" // ;
                // + "%3C" // < 400
                + "%3D" // =
                // + "%3E%3F" // >? 400
                + "%40" // @
                + "%41%42%43%44%45%46%47%48%49%4A%4B%4C%4D%4E%4F%50%51%52%53%54%55%56%57%58%59%5A" // A-Z
                + "%5B" // [
                // + "%5C" // \ Asp.Net changes this to /
                + "%5D%5E%5F%60" // ]^_`
                + "%61%62%63%64%65%66%67%68%69%6A%6B%6C%6D%6E%6F%70%71%72%73%74%75%76%77%78%79%7A" // a-z
                + "%7B%7C%7D%7E" // {|}~
                // + "%7F" 400
                + string.Empty;

            string expected = "/"
                + " !\"#$'(),.-/"
                + "0123456789"
                + ";=@"
                + "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
                + "[]^_`"
                + "abcdefghijklmnopqrstuvwxyz"
                + "{|}~";

            RunTest(serverName, inputPath, expected);
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        public void VerifyPathCanonicalization(string serverName)
        {
            // RunTest(serverName, "/a./b/../..c/.//.d./.", "/a./..c/.d./");
            RunTest(serverName, "//", "/");
            RunTest(serverName, "/.", "/");
            RunTest(serverName, "/a", "/a");
            // RunTest(serverName, "/a.", "/a."); // 404
            // RunTest(serverName, "/a./", "/a./"); // 404
            RunTest(serverName, "/a/", "/a/");
            RunTest(serverName, "/a/./b/", "/a/b/");
            RunTest(serverName, "/a/b/..", "/a/");
            RunTest(serverName, "/a/b/../", "/a/");
            RunTest(serverName, "/a/b/../c", "/a/c");
            RunTest(serverName, "/a/b/../c/d/", "/a/c/d/");
        }

        private void RunTest(string serverName, string sendPath, string expected)
        {
            int port = RunWebServer(serverName, EchoPath);
            string response = SendRequest(port, sendPath);
            CheckResponseStatusCode(response, 200);
            Assert.True(response.EndsWith(Convert.ToBase64String(Encoding.UTF8.GetBytes(expected))), response);
            VerifyUriRoundTrip(expected);
        }

        private string SendRequest(int port, string path)
        {
            string request =
                "GET " + path + " HTTP/1.1\r\n"
                    + "Host: localhost:" + port + "\r\n"
                    + "Connection: Close\r\n"
                    + "\r\n";
            byte[] requestBytes = Encoding.ASCII.GetBytes(request);

            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(IPAddress.Loopback, port);
            socket.Send(requestBytes);

            using (var reader = new StreamReader(new NetworkStream(socket, true)))
            {
                return reader.ReadToEnd();
            }
        }

        private void CheckResponseStatusCode(string response, int statusCode)
        {
            string[] lines = response.Split('\r', '\n');
            Assert.True(lines[0].Contains(statusCode.ToString()), response);
        }

        private void VerifyUriRoundTrip(string expected)
        {
            // We know these can't round trip.
            string safeExpected = expected
                .Replace("%", "%25")
                .Replace("?", "%3F")
                .Replace("#", "%23")
                .Replace(@"\", "%5C");

            Assert.Equal(expected, "/" + new Uri("http://localhost" + safeExpected).GetComponents(UriComponents.Path, UriFormat.Unescaped));
        }
    }
}
