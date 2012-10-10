using System.Collections.Generic;
using System.IO;

namespace Katana.Performance.ReferenceApp
{
    public static class Util
    {
        public static IEnumerable<byte> AlphabetCRLF(int length)
        {
            while (true)
            {
                for (var ch = 'a'; ch != 'z' + 1; ++ch)
                {
                    if (length-- == 0)
                    {
                        yield break;
                    }
                    yield return (byte)ch;
                }
                if (length-- == 0)
                {
                    yield break;
                }
                yield return (byte)' ';
                for (var ch = 'A'; ch != 'Z' + 1; ++ch)
                {
                    if (length-- == 0)
                    {
                        yield break;
                    }
                    yield return (byte)ch;
                }
                if (length-- == 0)
                {
                    yield break;
                }
                yield return (byte)'\r';
                if (length-- == 0)
                {
                    yield break;
                }
                yield return (byte)'\n';
            }
        }

        public static T Get<T>(IDictionary<string, object> env, string key) where T : class
        {
            object value;
            return env.TryGetValue(key, out value) ? value as T : default(T);
        }

        public static Stream ResponseBody(IDictionary<string, object> env)
        {
            return Get<Stream>(env, "owin.ResponseBody");
        }

        public static IDictionary<string, string[]> ResponseHeaders(IDictionary<string, object> env)
        {
            return Get<IDictionary<string, string[]>>(env, "owin.ResponseHeaders");
        }

        public static string RequestPath(IDictionary<string, object> env)
        {
            return Get<string>(env, "owin.RequestPath");
        }
    }
}