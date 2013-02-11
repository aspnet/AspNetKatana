// <copyright file="CompilationFailedException.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.AspNet.Razor.Owin.Compilation;

namespace Microsoft.AspNet.Razor.Owin
{
    [Serializable]
    public class CompilationFailedException : Exception, IMultiMessageException, IProvidesCompilationSource
    {
        public CompilationFailedException()
            : base(Resources.CompilationFailedException_DefaultMessage)
        {
            Messages = new List<CompilationMessage>();
        }

        public CompilationFailedException(IEnumerable<CompilationMessage> messages, string generatedCode)
            : base(FormatMessage(messages))
        {
            Messages = messages.ToList();
            GeneratedCode = generatedCode;
        }

        public CompilationFailedException(string message) : base(message)
        {
            Messages = new List<CompilationMessage>();
        }

        public CompilationFailedException(string message, Exception innerException) : base(message, innerException)
        {
            Messages = new List<CompilationMessage>();
        }

        public CompilationFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            GeneratedCode = info.GetString("GeneratedCode");
            var messages = new CompilationMessage[info.GetInt32("Messages.Count")];
            for (int i = 0; i < messages.Length; i++)
            {
                messages[i] = (CompilationMessage)info.GetValue("Messages[" + i + "]", typeof(CompilationMessage));
            }
            Messages = messages.ToList();
        }

        public string GeneratedCode { get; private set; }
        public IList<CompilationMessage> Messages { get; private set; }

        IEnumerable<IErrorMessage> IMultiMessageException.Messages
        {
            get { return Messages.Select(cm => new ErrorMessage(cm)); }
        }

        public string MessageListTitle
        {
            get { return "Compilation Errors"; }
        }

        public string CompilationSource
        {
            get { return GeneratedCode; }
        }

        private static string FormatMessage(IEnumerable<CompilationMessage> messages)
        {
            Tuple<int, int> counts = messages.Aggregate(Tuple.Create(0, 0), (last, current) =>
                Tuple.Create(
                    last.Item1 + (current.Level == MessageLevel.Error ? 1 : 0),
                    last.Item2 + (current.Level == MessageLevel.Warning ? 1 : 0)));

            return String.Format(Resources.CompilationFailedException_MessageWithErrorCounts, counts.Item1, counts.Item2);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("GeneratedCode", GeneratedCode);
            info.AddValue("Messages.Count", Messages.Count);
            for (int i = 0; i < Messages.Count; i++)
            {
                info.AddValue("Messages[" + i + "]", Messages[i]);
            }
        }
    }
}
