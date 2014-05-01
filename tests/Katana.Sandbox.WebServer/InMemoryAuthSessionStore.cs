// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;

namespace Katana.Sandbox.WebServer
{
    public class InMemoryAuthSessionStore : IAuthenticationSessionStore
    {
        private ConcurrentDictionary<string, AuthenticationTicket> _store;
        private Timer _gcTimer;

        public InMemoryAuthSessionStore()
        {
            _store = new ConcurrentDictionary<string, AuthenticationTicket>();
            _gcTimer = new Timer(new TimerCallback(GarbageCollect), null, TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(15));
        }

        // Remove any expired entries
        private void GarbageCollect(object state)
        {
            DateTimeOffset now = DateTimeOffset.Now.ToUniversalTime();
            foreach (var pair in _store)
            {
                var expiresAt = pair.Value.Properties.ExpiresUtc;
                if (expiresAt < now)
                {
                    AuthenticationTicket value;
                    _store.TryRemove(pair.Key, out value);
                }
            }
        }

        public Task<string> StoreAsync(AuthenticationTicket ticket)
        {
            string key = Guid.NewGuid().ToString();
            _store.AddOrUpdate(key, ticket, (k, t) => ticket);
            return Task.FromResult(key);
        }

        public Task RenewAsync(string key, AuthenticationTicket ticket)
        {
            _store.AddOrUpdate(key, ticket, (k, t) => ticket);
            return Task.FromResult(0);
        }

        public Task<AuthenticationTicket> RetrieveAsync(string key)
        {
            AuthenticationTicket ticket;
            _store.TryGetValue(key, out ticket);
            return Task.FromResult(ticket);
        }

        public Task RemoveAsync(string key)
        {
            AuthenticationTicket value;
            _store.TryRemove(key, out value);
            return Task.FromResult(0);
        }
    }
}