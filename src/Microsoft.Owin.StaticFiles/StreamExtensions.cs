// -----------------------------------------------------------------------
// <copyright file="StreamExtensions.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
// </copyright>
// -----------------------------------------------------------------------

using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Owin.StaticFiles
{
    internal static class StreamExtensions
    {
        internal static Task WriteAsync(this Stream output, byte[] body, int offset, int length, CancellationToken cancel)
        {
            cancel.ThrowIfCancellationRequested();
            return Task.Factory.FromAsync(output.BeginWrite, output.EndWrite, body, offset, length, null);
        }
    }
}
