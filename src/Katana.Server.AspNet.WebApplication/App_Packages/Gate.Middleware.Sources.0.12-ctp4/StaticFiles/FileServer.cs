using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Gate.Middleware.Utils;
using Owin;
using System.Threading.Tasks;

namespace Gate.Middleware.StaticFiles
{
    internal class FileServer
    {
        private const int OK = 200;
        private const int PartialContent = 206;
        private const int NotFound = 404;
        private const int Forbidden = 403;
        private const int RequestedRangeNotSatisfiable = 416;

        private readonly string root;
        private string pathInfo;
        private Tuple<long, long> range;

        // Note: Path should be exposed when implementing Sendfile middleware.
        private string path;

        public FileServer(string root)
        {
            this.root = root;
        }

        public Task<ResultParameters> Invoke(CallParameters call)
        {
            pathInfo = call.Environment[OwinConstants.RequestPath].ToString();

            if (pathInfo.StartsWith("/"))
            {
                pathInfo = pathInfo.Substring(1);
            }

            if (pathInfo.Contains(".."))
            {
                return Fail(Forbidden, "Forbidden").Invoke(call);
            }

            path = Path.Combine(root ?? string.Empty, pathInfo);

            if (!File.Exists(path))
            {
                return Fail(NotFound, "File not found: " + pathInfo).Invoke(call);
            }

            try
            {
                return Serve(call);
            }
            catch (UnauthorizedAccessException)
            {
                return Fail(Forbidden, "Forbidden").Invoke(call);
            }
        }

        private static AppDelegate Fail(int status, string body, IDictionary<string, string[]> headers = null)
        {
            return call =>
                TaskHelpers.FromResult(
                    new ResultParameters
                    {
                        Status = status,
                        Headers = Headers.New(headers)
                            .SetHeader("Content-Type", "text/plain")
                            .SetHeader("Content-Length", body.Length.ToString(CultureInfo.InvariantCulture))
                            .SetHeader("X-Cascade", "pass"),
                        Body = TextBody.Create(body, Encoding.UTF8),
                        Properties = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                    });
        }

        private Task<ResultParameters> Serve(CallParameters call)
        {
            var fileInfo = new FileInfo(path);
            var size = fileInfo.Length;

            int status;
            var headers = Headers.New()
                .SetHeader("Last-Modified", fileInfo.LastWriteTimeUtc.ToHttpDateString())
                .SetHeader("Content-Type", Mime.MimeType(fileInfo.Extension, "text/plain"));

            if (!RangeHeader.IsValid(call.Headers))
            {
                status = OK;
                range = new Tuple<long, long>(0, size - 1);
            }
            else
            {
                var ranges = RangeHeader.Parse(call.Headers, size);

                if (ranges == null)
                {
                    // Unsatisfiable.  Return error and file size.
                    return Fail(
                        RequestedRangeNotSatisfiable,
                        "Byte range unsatisfiable",
                        Headers.New().SetHeader("Content-Range", "bytes */" + size))
                        .Invoke(call);
                }

                if (ranges.Count() > 1)
                {
                    // TODO: Support multiple byte ranges.
                    status = OK;
                    range = new Tuple<long, long>(0, size - 1);
                }
                else
                {
                    // Partial content
                    range = ranges.First();
                    status = PartialContent;
                    headers.SetHeader("Content-Range", "bytes " + range.Item1 + "-" + range.Item2 + "/" + size);
                    size = range.Item2 - range.Item1 + 1;
                }
            }

            headers.SetHeader("Content-Length", size.ToString());

            return TaskHelpers.FromResult(new ResultParameters
                {
                    Status = status,
                    Headers = headers,
                    Body = FileBody.Create(path, range),
                    Properties = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                });
        }
    }
}