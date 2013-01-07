// -----------------------------------------------------------------------
// <copyright file="IDirectoryFormatProvider.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Owin.StaticFiles.DirectoryFormatters
{
    public interface IDirectoryFormatSelector
    {
        bool TryDetermineFormatter(IDictionary<string, object> environment, out IDirectoryInfoFormatter formatter);
    }
}
