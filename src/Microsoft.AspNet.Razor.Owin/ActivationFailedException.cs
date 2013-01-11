// -----------------------------------------------------------------------
// <copyright file="ActivationFailedException.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

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
