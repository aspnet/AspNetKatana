// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.Owin.Security.Infrastructure
{
    public interface IAuthenticationTicketProvider
    {
        void Creating(AuthenticationTicketProviderContext context);
        Task CreatingAsync(AuthenticationTicketProviderContext context);
        void Consuming(AuthenticationTicketProviderContext context);
        Task ConsumingAsync(AuthenticationTicketProviderContext context);
    }
}
