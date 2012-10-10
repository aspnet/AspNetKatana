using System;

namespace Katana.Performance.ReferenceApp
{
    public class CanonicalRequestAttribute : Attribute
    {
        public string Path { get; set; }
        public string Description { get; set; }
    }
}