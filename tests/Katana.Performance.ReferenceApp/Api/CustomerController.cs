// <copyright file="CustomerController.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2011-2013 Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System.Web.Http;

namespace Katana.Performance.ReferenceApp
{
    public class CustomerController : ApiController
    {
        public static Customer CachedCustomerReply = GetCustomer(999);

        public CustomerController()
        {
        }

        // Gets
        [HttpGet]
        public Customer Get(string customerId)
        {
            return CachedCustomerReply;
        }

        private static Customer GetCustomer(int id)
        {
            return new Customer()
            {
                Id = id,
                LastName = "Smith",
                FirstName = "Mary",
                HouseNumber = "333",
                Street = "Main Street NE",
                City = "Redmond",
                State = "WA",
                ZipCode = "98053"
            };
        }
    }
}