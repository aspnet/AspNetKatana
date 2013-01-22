// <copyright file="ErrorMessage.cs" company="Katana contributors">
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

using Microsoft.AspNet.Razor.Owin.Compilation;

namespace Microsoft.AspNet.Razor.Owin
{
    public class ErrorMessage : IErrorMessage
    {
        public ErrorMessage(string message)
        {
            Message = message;
            Location = new FileLocation(null);
        }

        public ErrorMessage(string message, FileLocation location)
        {
            Message = message;
            Location = location;
        }

        public ErrorMessage(CompilationMessage cm)
        {
            Message = cm.Message;
            Location = cm.Location;
        }

        public FileLocation Location { get; set; }
        public string Message { get; set; }
    }
}
