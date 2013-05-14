using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;

namespace Microsoft.Owin.Diagnostics.Views
{
    public abstract class BaseView
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public OwinRequest Request;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public OwinResponse Response;
        public StreamWriter Output { get; set; }

        public void Execute(IDictionary<string, object> environment)
        {
            Request = new OwinRequest(environment);
            Response = new OwinResponse(environment);
            Output = new StreamWriter(Response.Body);
            Execute();
            Output.Dispose();
        }


        public abstract void Execute();

        protected void WriteLiteral(string value)
        {
            Output.Write(value);
        }

        protected void WriteAttribute<T1>(
            string start,
            Tuple<string, int> leader,
            Tuple<string, int> trailer,
            Tuple<Tuple<string, int>, Tuple<T1, int>, bool> p1)
        {
            WriteLiteral(leader.Item1);
            WriteLiteral(p1.Item1.Item1);
            Write(p1.Item2.Item1);
            WriteLiteral(trailer.Item1);
        }

        protected void WriteAttribute<T1, T2>(
            string start,
            Tuple<string, int> leader,
            Tuple<string, int> trailer,
            Tuple<Tuple<string, int>, Tuple<T1, int>, bool> p1,
            Tuple<Tuple<string, int>, Tuple<T2, int>, bool> p2)
        {
            WriteLiteral(leader.Item1);
            WriteLiteral(p1.Item1.Item1);
            Write(p1.Item2.Item1);
            WriteLiteral(p2.Item1.Item1);
            Write(p2.Item2.Item1);
            WriteLiteral(trailer.Item1);
        }
        protected void WriteAttribute<T1, T2, T3>(
            string start,
            Tuple<string, int> leader,
            Tuple<string, int> trailer,
            Tuple<Tuple<string, int>, Tuple<T1, int>, bool> p1,
            Tuple<Tuple<string, int>, Tuple<T2, int>, bool> p2,
            Tuple<Tuple<string, int>, Tuple<T3, int>, bool> p3)
        {
            WriteLiteral(leader.Item1);
            WriteLiteral(p1.Item1.Item1);
            Write(p1.Item2.Item1);
            WriteLiteral(p2.Item1.Item1);
            Write(p2.Item2.Item1);
            WriteLiteral(p3.Item1.Item1);
            Write(p3.Item2.Item1);
            WriteLiteral(trailer.Item1);
        }
        protected void WriteAttribute<T1, T2, T3, T4>(
            string start,
            Tuple<string, int> leader,
            Tuple<string, int> trailer,
            Tuple<Tuple<string, int>, Tuple<T1, int>, bool> p1,
            Tuple<Tuple<string, int>, Tuple<T2, int>, bool> p2,
            Tuple<Tuple<string, int>, Tuple<T3, int>, bool> p3,
            Tuple<Tuple<string, int>, Tuple<T4, int>, bool> p4)
        {
            WriteLiteral(leader.Item1);
            WriteLiteral(p1.Item1.Item1);
            Write(p1.Item2.Item1);
            WriteLiteral(p2.Item1.Item1);
            Write(p2.Item2.Item1);
            WriteLiteral(p3.Item1.Item1);
            Write(p3.Item2.Item1);
            WriteLiteral(p4.Item1.Item1);
            Write(p4.Item2.Item1);
            WriteLiteral(trailer.Item1);
        }
        protected void WriteAttribute<T1, T2, T3, T4, T5>(
            string start,
            Tuple<string, int> leader,
            Tuple<string, int> trailer,
            Tuple<Tuple<string, int>, Tuple<T1, int>, bool> p1,
            Tuple<Tuple<string, int>, Tuple<T2, int>, bool> p2,
            Tuple<Tuple<string, int>, Tuple<T3, int>, bool> p3,
            Tuple<Tuple<string, int>, Tuple<T4, int>, bool> p4,
            Tuple<Tuple<string, int>, Tuple<T5, int>, bool> p5)
        {
            WriteLiteral(leader.Item1);
            WriteLiteral(p1.Item1.Item1);
            Write(p1.Item2.Item1);
            WriteLiteral(p2.Item1.Item1);
            Write(p2.Item2.Item1);
            WriteLiteral(p3.Item1.Item1);
            Write(p3.Item2.Item1);
            WriteLiteral(p4.Item1.Item1);
            Write(p4.Item2.Item1);
            WriteLiteral(p5.Item1.Item1);
            Write(p5.Item2.Item1);
            WriteLiteral(trailer.Item1);
        }
        protected void WriteAttribute<T1, T2, T3, T4, T5, T6>(
            string start,
            Tuple<string, int> leader,
            Tuple<string, int> trailer,
            Tuple<Tuple<string, int>, Tuple<T1, int>, bool> p1,
            Tuple<Tuple<string, int>, Tuple<T2, int>, bool> p2,
            Tuple<Tuple<string, int>, Tuple<T3, int>, bool> p3,
            Tuple<Tuple<string, int>, Tuple<T4, int>, bool> p4,
            Tuple<Tuple<string, int>, Tuple<T5, int>, bool> p5,
            Tuple<Tuple<string, int>, Tuple<T6, int>, bool> p6)
        {
            WriteLiteral(leader.Item1);
            WriteLiteral(p1.Item1.Item1);
            Write(p1.Item2.Item1);
            WriteLiteral(p2.Item1.Item1);
            Write(p2.Item2.Item1);
            WriteLiteral(p3.Item1.Item1);
            Write(p3.Item2.Item1);
            WriteLiteral(p4.Item1.Item1);
            Write(p4.Item2.Item1);
            WriteLiteral(p5.Item1.Item1);
            Write(p5.Item2.Item1);
            WriteLiteral(p6.Item1.Item1);
            Write(p6.Item2.Item1);
            WriteLiteral(trailer.Item1);
        }

        private void WriteEncoded(string value)
        {
            WebUtility.HtmlEncode(value, Output);
        }

        protected void Write(object value)
        {
            WriteEncoded(Convert.ToString(value, CultureInfo.InvariantCulture));
        }

        protected void Write(string value)
        {
            WriteEncoded(value);
        }

    }
}
