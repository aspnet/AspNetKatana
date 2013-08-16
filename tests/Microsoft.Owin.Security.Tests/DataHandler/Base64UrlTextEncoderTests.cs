// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Owin.Security.DataHandler.Encoder;
using Shouldly;
using Xunit;

namespace Microsoft.Owin.Security.Tests.DataHandler
{
    public class Base64UrlTextEncoderTests
    {
        [Fact]
        public void DataOfVariousLengthRoundTripCorrectly()
        {
            var encoder = new Base64UrlTextEncoder();
            for (int length = 0; length != 256; ++length)
            {
                var data = new byte[length];
                for (int index = 0; index != length; ++index)
                {
                    data[index] = (byte)(5 + length + index * 23);
                }
                string text = encoder.Encode(data);
                byte[] result = encoder.Decode(text);

                for (int index = 0; index != length; ++index)
                {
                    result[index].ShouldBe(data[index]);
                }
            }
        }
    }
}
