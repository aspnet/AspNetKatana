// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Microsoft.Owin.Security.Cookies
{
    public interface IAuthenticationSessionStore
    {
        Task<string> StoreAsync(AuthenticationTicket ticket);
        Task RenewAsync(string key, AuthenticationTicket ticket);
        Task<AuthenticationTicket> RetrieveAsync(string key);
        Task RemoveAsync(string key);
    }
}
