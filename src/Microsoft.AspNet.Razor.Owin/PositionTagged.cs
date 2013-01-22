// <copyright file="PositionTagged.cs" company="Katana contributors">
//   Copyright 2011-2013 Katana contributors
// </copyright>
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

using System;
using System.Diagnostics;

namespace Microsoft.AspNet.Razor.Owin
{
    [DebuggerDisplay("({Position})\"{Value}\"")]
    public class PositionTagged<T>
    {
        private PositionTagged()
        {
            Position = 0;
            Value = default(T);
        }

        public PositionTagged(T value, int offset)
        {
            Position = offset;
            Value = value;
        }

        public int Position { get; private set; }
        public T Value { get; private set; }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static implicit operator T(PositionTagged<T> value)
        {
            return value.Value;
        }

        public static implicit operator PositionTagged<T>(Tuple<T, int> value)
        {
            return new PositionTagged<T>(value.Item1, value.Item2);
        }
    }
}
