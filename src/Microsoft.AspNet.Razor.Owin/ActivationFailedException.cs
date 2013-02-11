// <copyright file="ActivationFailedException.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Runtime.Serialization;

namespace Microsoft.AspNet.Razor.Owin
{
    [Serializable]
    public class ActivationFailedException : Exception
    {
        public ActivationFailedException(Type attemptedToActivate)
            : base(FormatMessage(attemptedToActivate))
        {
            AttemptedToActivate = attemptedToActivate;
        }

        public ActivationFailedException(string message) : base(message)
        {
        }

        public ActivationFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public ActivationFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            if (info.GetBoolean("AttemptedToActivate.HasValue"))
            {
                AttemptedToActivate = (Type)info.GetValue("AttemptedToActivate.Value", typeof(Type));
            }
        }

        public Type AttemptedToActivate { get; private set; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("AttemptedToActivate.HasValue", AttemptedToActivate != null);
            if (AttemptedToActivate != null)
            {
                info.AddValue("AttemptedToActivate.Value", AttemptedToActivate);
            }
        }

        private static string FormatMessage(Type attemptedToActivate)
        {
            return String.Format(
                Resources.ActivationFailedException_DefaultMessage,
                attemptedToActivate.AssemblyQualifiedName);
        }
    }
}
