using System.Collections.Generic;

namespace Microsoft.Owin.Diagnostics.Views
{
    public class StackFrame
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public string Function { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public string File { get; set; }
        public int Line { get; set; }

        public int PreContextLine { get; set; }
        public IEnumerable<string> PreContextCode { get; set; }
        public string ContextCode { get; set; }
        public IEnumerable<string> PostContextCode { get; set; }
    }
}