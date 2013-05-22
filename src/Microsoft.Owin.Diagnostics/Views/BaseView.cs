// <copyright file="BaseView.cs" company="Microsoft Open Technologies, Inc.">
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

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Net;

namespace Microsoft.Owin.Diagnostics.Views
{
    /// <summary>
    /// Infrastructure
    /// </summary>
    [ExcludeFromCodeCoverage]
    public abstract class BaseView
    {
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Property value types don't work properly")]
        public OwinRequest Request;
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Property value types don't work properly")]
        public OwinResponse Response;
        public StreamWriter Output { get; set; }

        public void Execute(IDictionary<string, object> environment)
        {
            Request = new OwinRequest(environment);
            Response = new OwinResponse(environment);
            Output = new StreamWriter(Response.Body);
            Execute();
            Output.Dispose();
        }

        public abstract void Execute();

        protected void WriteLiteral(string value)
        {
            Output.Write(value);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Signature determined by code gen")]
        protected void WriteAttribute<T1>(
            string name,
            Tuple<string, int> leader,
            Tuple<string, int> trailer,
            Tuple<Tuple<string, int>, Tuple<T1, int>, bool> part1)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (leader == null)
            {
                throw new ArgumentNullException("leader");
            }
            if (trailer == null)
            {
                throw new ArgumentNullException("trailer");
            }
            if (part1 == null)
            {
                throw new ArgumentNullException("part1");
            }
            WriteLiteral(leader.Item1);
            WriteLiteral(part1.Item1.Item1);
            Write(part1.Item2.Item1);
            WriteLiteral(trailer.Item1);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Signature determined by code gen")]
        protected void WriteAttribute<T1, T2>(
            string name,
            Tuple<string, int> leader,
            Tuple<string, int> trailer,
            Tuple<Tuple<string, int>, Tuple<T1, int>, bool> part1,
            Tuple<Tuple<string, int>, Tuple<T2, int>, bool> part2)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (leader == null)
            {
                throw new ArgumentNullException("leader");
            }
            if (trailer == null)
            {
                throw new ArgumentNullException("trailer");
            }
            if (part1 == null)
            {
                throw new ArgumentNullException("part1");
            }
            if (part2 == null)
            {
                throw new ArgumentNullException("part2");
            }
            WriteLiteral(leader.Item1);
            WriteLiteral(part1.Item1.Item1);
            Write(part1.Item2.Item1);
            WriteLiteral(part2.Item1.Item1);
            Write(part2.Item2.Item1);
            WriteLiteral(trailer.Item1);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Signature determined by code gen")]
        protected void WriteAttribute<T1, T2, T3>(
            string name,
            Tuple<string, int> leader,
            Tuple<string, int> trailer,
            Tuple<Tuple<string, int>, Tuple<T1, int>, bool> part1,
            Tuple<Tuple<string, int>, Tuple<T2, int>, bool> part2,
            Tuple<Tuple<string, int>, Tuple<T3, int>, bool> part3)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (leader == null)
            {
                throw new ArgumentNullException("leader");
            }
            if (trailer == null)
            {
                throw new ArgumentNullException("trailer");
            }
            if (part1 == null)
            {
                throw new ArgumentNullException("part1");
            }
            if (part2 == null)
            {
                throw new ArgumentNullException("part2");
            }
            if (part3 == null)
            {
                throw new ArgumentNullException("part3");
            }
            WriteLiteral(leader.Item1);
            WriteLiteral(part1.Item1.Item1);
            Write(part1.Item2.Item1);
            WriteLiteral(part2.Item1.Item1);
            Write(part2.Item2.Item1);
            WriteLiteral(part3.Item1.Item1);
            Write(part3.Item2.Item1);
            WriteLiteral(trailer.Item1);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Signature determined by code gen")]
        protected void WriteAttribute<T1, T2, T3, T4>(
            string name,
            Tuple<string, int> leader,
            Tuple<string, int> trailer,
            Tuple<Tuple<string, int>, Tuple<T1, int>, bool> part1,
            Tuple<Tuple<string, int>, Tuple<T2, int>, bool> part2,
            Tuple<Tuple<string, int>, Tuple<T3, int>, bool> part3,
            Tuple<Tuple<string, int>, Tuple<T4, int>, bool> part4)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (leader == null)
            {
                throw new ArgumentNullException("leader");
            }
            if (trailer == null)
            {
                throw new ArgumentNullException("trailer");
            }
            if (part1 == null)
            {
                throw new ArgumentNullException("part1");
            }
            if (part2 == null)
            {
                throw new ArgumentNullException("part2");
            }
            if (part3 == null)
            {
                throw new ArgumentNullException("part3");
            }
            if (part4 == null)
            {
                throw new ArgumentNullException("part4");
            }
            WriteLiteral(leader.Item1);
            WriteLiteral(part1.Item1.Item1);
            Write(part1.Item2.Item1);
            WriteLiteral(part2.Item1.Item1);
            Write(part2.Item2.Item1);
            WriteLiteral(part3.Item1.Item1);
            Write(part3.Item2.Item1);
            WriteLiteral(part4.Item1.Item1);
            Write(part4.Item2.Item1);
            WriteLiteral(trailer.Item1);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Signature determined by code gen")]
        protected void WriteAttribute<T1, T2, T3, T4, T5>(
            string name,
            Tuple<string, int> leader,
            Tuple<string, int> trailer,
            Tuple<Tuple<string, int>, Tuple<T1, int>, bool> part1,
            Tuple<Tuple<string, int>, Tuple<T2, int>, bool> part2,
            Tuple<Tuple<string, int>, Tuple<T3, int>, bool> part3,
            Tuple<Tuple<string, int>, Tuple<T4, int>, bool> part4,
            Tuple<Tuple<string, int>, Tuple<T5, int>, bool> part5)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (leader == null)
            {
                throw new ArgumentNullException("leader");
            }
            if (trailer == null)
            {
                throw new ArgumentNullException("trailer");
            }
            if (part1 == null)
            {
                throw new ArgumentNullException("part1");
            }
            if (part2 == null)
            {
                throw new ArgumentNullException("part2");
            }
            if (part3 == null)
            {
                throw new ArgumentNullException("part3");
            }
            if (part4 == null)
            {
                throw new ArgumentNullException("part4");
            }
            if (part5 == null)
            {
                throw new ArgumentNullException("part5");
            }            
            WriteLiteral(leader.Item1);
            WriteLiteral(part1.Item1.Item1);
            Write(part1.Item2.Item1);
            WriteLiteral(part2.Item1.Item1);
            Write(part2.Item2.Item1);
            WriteLiteral(part3.Item1.Item1);
            Write(part3.Item2.Item1);
            WriteLiteral(part4.Item1.Item1);
            Write(part4.Item2.Item1);
            WriteLiteral(part5.Item1.Item1);
            Write(part5.Item2.Item1);
            WriteLiteral(trailer.Item1);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Signature determined by code gen")]
        protected void WriteAttribute<T1, T2, T3, T4, T5, T6>(
            string name,
            Tuple<string, int> leader,
            Tuple<string, int> trailer,
            Tuple<Tuple<string, int>, Tuple<T1, int>, bool> part1,
            Tuple<Tuple<string, int>, Tuple<T2, int>, bool> part2,
            Tuple<Tuple<string, int>, Tuple<T3, int>, bool> part3,
            Tuple<Tuple<string, int>, Tuple<T4, int>, bool> part4,
            Tuple<Tuple<string, int>, Tuple<T5, int>, bool> part5,
            Tuple<Tuple<string, int>, Tuple<T6, int>, bool> part6)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (leader == null)
            {
                throw new ArgumentNullException("leader");
            }
            if (trailer == null)
            {
                throw new ArgumentNullException("trailer");
            }
            if (part1 == null)
            {
                throw new ArgumentNullException("part1");
            }
            if (part2 == null)
            {
                throw new ArgumentNullException("part2");
            }
            if (part3 == null)
            {
                throw new ArgumentNullException("part3");
            }
            if (part4 == null)
            {
                throw new ArgumentNullException("part4");
            }
            if (part5 == null)
            {
                throw new ArgumentNullException("part5");
            }
            if (part6 == null)
            {
                throw new ArgumentNullException("part6");
            }
            WriteLiteral(leader.Item1);
            WriteLiteral(part1.Item1.Item1);
            Write(part1.Item2.Item1);
            WriteLiteral(part2.Item1.Item1);
            Write(part2.Item2.Item1);
            WriteLiteral(part3.Item1.Item1);
            Write(part3.Item2.Item1);
            WriteLiteral(part4.Item1.Item1);
            Write(part4.Item2.Item1);
            WriteLiteral(part5.Item1.Item1);
            Write(part5.Item2.Item1);
            WriteLiteral(part6.Item1.Item1);
            Write(part6.Item2.Item1);
            WriteLiteral(trailer.Item1);
        }

        private void WriteEncoded(string value)
        {
            WebUtility.HtmlEncode(value, Output);
        }

        protected void Write(object value)
        {
            WriteEncoded(Convert.ToString(value, CultureInfo.InvariantCulture));
        }

        protected void Write(string value)
        {
            WriteEncoded(value);
        }
    }
}
