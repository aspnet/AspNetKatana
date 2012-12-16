using System;
using System.Collections.Generic;

namespace Microsoft.Owin.StaticFiles
{
    public class StaticFileOptions
    {
        public StaticFileOptions()
        {
            PathsAndDirectories = new List<KeyValuePair<string, string>>();
        }

        public IList<KeyValuePair<string, string>> PathsAndDirectories { get; set; }

        public StaticFileOptions AddPathAndDirectory(string path, string directory)
        {
            PathsAndDirectories.Add(new KeyValuePair<string, string>(path, directory));
            return this;
        }
    }
}