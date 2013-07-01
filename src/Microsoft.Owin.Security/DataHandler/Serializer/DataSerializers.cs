// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Owin.Security.DataHandler.Serializer
{
    public static class DataSerializers
    {
        static DataSerializers()
        {
            Extra = new ExtraSerializer();
            Ticket = new TicketSerializer();
        }

        public static IDataSerializer<AuthenticationExtra> Extra { get; set; }

        public static IDataSerializer<AuthenticationTicket> Ticket { get; set; }
    }
}
