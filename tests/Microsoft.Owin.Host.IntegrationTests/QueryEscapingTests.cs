// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Owin;
using Xunit;
using Xunit.Extensions;

#if NET40
namespace Microsoft.Owin.Host40.IntegrationTests
#else

namespace Microsoft.Owin.Host45.IntegrationTests
#endif
{
    public class QueryEscapingTests : TestBase
    {
        public void EchoQuery(IAppBuilder app)
        {
            app.Run(context =>
            {
                QueryString query = context.Request.QueryString;
                context.Response.ContentLength = query.Value.Length;
                context.Response.Write(query.Value);
                context.Response.Body.Flush();
                return TaskHelpers.Completed();
            });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        public void VerifyEscapedUnicode(string serverName)
        {
            string input = "%E8%91%89"; // 葉
            RunTest(serverName, input);
            VerifyUriRoundTrip(input);
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public void VerifyEscapedCharacters(string serverName)
        {
            // In 4.0 Everything will stay escaped.  In 4.5 on SystemWeb, System.Uri will un-escape the un-reserved characters.
            // OwinHttpListener isn't using System.Uri, so everything stays escaped.

            // Everything should stay escaped in the query
            string input = string.Empty
                + "%00%01%02%03%04%05%06%07%08%09%0A%0B%0C%0D%0E%0F%10%11%12%13%14%15%16%17%18%19%1A%1B%1C%1D%1E%1F" // 400
                + "%20%21%22%23%24" // SP!"#$
                + "%25" // % 
                + "%26" // &
                // + "%27%28%29" // '()
                // + "%2A" // *
                + "%2B" // +
                + "%2C" // ,
                // + "%2D%2E" // -.
                + "%2F" // /
                // + "%30%31%32%33%34%35%36%37%38%39" // 0-9
                // + "%3A" // :
                + "%3B" // ;
                + "%3C" // <
                + "%3D" // =
                + "%3E%3F" // >?
                + "%40" // @
                // + "%41%42%43%44%45%46%47%48%49%4A%4B%4C%4D%4E%4F%50%51%52%53%54%55%56%57%58%59%5A" // A-Z
                // + "%5B" // [
                + "%5C" // \
                // + "%5D" // ]
                + "%5E" // ^
                // + "%5F" // _
                + "%60" // `
                // + "%61%62%63%64%65%66%67%68%69%6A%6B%6C%6D%6E%6F%70%71%72%73%74%75%76%77%78%79%7A" // a-z
                + "%7B%7C%7D" // {|}
                // + "%7E" // ~
                + "%7F" // del
                + string.Empty;

            RunTest(serverName, input);
            VerifyUriRoundTrip(input);
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public void VerifyUnEscapedCharacters(string serverName)
        {
            // System.Uri escapes some characters in Asp.Net, marked bellow.
            // OwinHttpListener does not use System.Uri so everything stays un-escaped.

            // Everything should stay escaped in the query
            string input = string.Empty
                // + "\x00\x01\x02\x03\x04\x05\x06\x07\x08\x09\x0A\x0B\x0C\x0D\x0E\x0F\x10\x11\x12\x13\x14\x15\x16\x17\x18\x19\x1A\x1B\x1C\x1D\x1E\x1F" // 400
                // + "\x20" // SP 400
                + "\x21" // !
                // + "\x22" // " Escaped
                // + "\x23" // # NOT Escaped, BREAKS System.Uri (It thinks this marks a fragment)
                + "\x24" // $
                // + "\x25" // % Escaped
                + "\x26" // &
                + "\x27\x28\x29" // '()
                + "\x2A" // *
                + "\x2B" // +
                + "\x2C\x2D\x2E\x2F" // ,-./
                + "\x30\x31\x32\x33\x34\x35\x36\x37\x38\x39" // 0-9
                + "\x3A" // :
                + "\x3B" // ;
                // + "\x3C" // < Escaped
                + "\x3D" // =
                // + "\x3E" Escaped
                + "\x3F" // >?
                + "\x40" // @
                + "\x41\x42\x43\x44\x45\x46\x47\x48\x49\x4A\x4B\x4C\x4D\x4E\x4F\x50\x51\x52\x53\x54\x55\x56\x57\x58\x59\x5A" // A-Z
                // + "\x5B" // [ Escaped in 4.0
                // + "\x5C" // \ Escaped
                // + "\x5D" // ] Escaped in 4.0
                // + "\x5E" // ^ Escaped
                + "\x5F" // _
                // + "\x60" // ` Escaped
                + "\x61\x62\x63\x64\x65\x66\x67\x68\x69\x6A\x6B\x6C\x6D\x6E\x6F\x70\x71\x72\x73\x74\x75\x76\x77\x78\x79\x7A" // a-z
                // + "\x7B\x7C\x7D" // {|} Escaped
                + "\x7E" // ~
                // + "\x7F" // del 400
                + string.Empty;

            RunTest(serverName, input);
            VerifyUriRoundTrip(input);
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        public void VerifyHashCharacterWorks(string serverName)
        {
            // "\x23" // # NOT Escaped, BREAKS System.Uri (It thinks this marks a fragment)
            RunTest(serverName, "query#notfragment");
        }

        private void RunTest(string serverName, string input)
        {
            int port = RunWebServer(serverName, EchoQuery);
            string response = SendRequest(port, input);
            CheckResponseStatusCode(response, 200);
            Assert.True(response.EndsWith(input), response);
        }

        private string SendRequest(int port, string query)
        {
            string request =
                "GET /?" + query + " HTTP/1.1\r\n"
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

        private void VerifyUriRoundTrip(string input)
        {
            Assert.Equal("?" + input, new Uri("http://localhost/?" + input).Query);
        }
    }
}
