// Licensed to Monkey Square, Inc. under one or more contributor 
// license agreements.  See the NOTICE file distributed with 
// this work or additional information regarding copyright 
// ownership.  Monkey Square, Inc. licenses this file to you 
// under the Apache License, Version 2.0 (the "License"); you 
// may not use this file except in compliance with the License.
// You may obtain a copy of the License at 
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.

using System;

namespace Owin.Types
{
    internal partial struct OwinWebSocketReceiveMessage
    {
        private readonly Tuple<int, bool, int> _tuple;

        public OwinWebSocketReceiveMessage(Tuple<int, bool, int> tuple)
        {
            _tuple = tuple;
        }

        public int MessageType { get { return _tuple.Item1; } }
        public bool EndOfMessage { get { return _tuple.Item2; } }
        public int Count { get { return _tuple.Item3; } }

        #region Value-type equality
        public bool Equals(OwinWebSocketReceiveMessage other)
        {
            return Equals(_tuple, other._tuple);
        }

        public override bool Equals(object obj)
        {
            return obj is OwinWebSocketReceiveMessage && Equals((OwinWebSocketReceiveMessage)obj);
        }

        public override int GetHashCode()
        {
            return (_tuple != null ? _tuple.GetHashCode() : 0);
        }

        public static bool operator ==(OwinWebSocketReceiveMessage left, OwinWebSocketReceiveMessage right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(OwinWebSocketReceiveMessage left, OwinWebSocketReceiveMessage right)
        {
            return !left.Equals(right);
        }
        #endregion
    }
}
