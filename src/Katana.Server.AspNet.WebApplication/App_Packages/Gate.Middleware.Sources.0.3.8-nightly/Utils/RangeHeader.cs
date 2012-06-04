using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Gate.Middleware.Utils
{
    internal class RangeHeader
    {
        const string ValuePrefix = "bytes=";

        public static bool IsValid(IDictionary<string, object> env)
        {
            var headers = new Environment(env).Headers;

            var httpRange = headers.GetHeader("Range");

            var isValid = (httpRange != null && httpRange.StartsWith(ValuePrefix, StringComparison.InvariantCultureIgnoreCase)) && httpRange.Length > ValuePrefix.Length;

            return isValid;
        }

        public static IEnumerable<Tuple<long, long>> Parse(IDictionary<string, object> env, long size)
        {
            if (!IsValid(env))
            {
                throw new InvalidOperationException("Validate the Range header prior to parsing.");
            }

            var httpRange = new Environment(env).Headers.GetHeader("Range");

            var ranges = new List<Tuple<long, long>>();

            var rangeSpecs = httpRange.Substring(ValuePrefix.Length)
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(range => range.Trim());

            var hasValidRanges = true;
            foreach (var rangeSpec in rangeSpecs)
            {
                Tuple<long, long> range;
                if (TryParseRange(rangeSpec, size, out range))
                {
                    ranges.Add(range);
                }
                else
                {
                    hasValidRanges = false;
                    break;
                }

            }

            return hasValidRanges ? ranges : null;
        }

        private static bool TryParseRange(string rangeSpec, long size, out Tuple<long, long> arange)
        {
            arange = null;
            var range = rangeSpec.Split(new[] { '-' });

            if (!range.Any() || range.Count() < 2)
            {
                return false;
            }

            var start = range[0];
            var end = range[1];

            long startValue;
            long endValue;

            if (string.IsNullOrEmpty(start))
            {
                if (string.IsNullOrEmpty(end))
                {
                    return false;
                }

                // suffix-byte-range-spec, represents trailing suffix of file
                startValue = new[] { size - long.Parse(end), 0 }.Max();
                endValue = size - 1;
            }
            else
            {
                startValue = long.Parse(start);

                if (string.IsNullOrEmpty(end))
                {
                    endValue = size - 1;
                }
                else
                {
                    endValue = long.Parse(end);

                    if (endValue < startValue)
                    {
                        // backwards range is syntactically invalid;
                        return false;
                    }

                    if (endValue >= size)
                    {
                        endValue = size - 1;
                    }

                }
            }

            if (startValue <= endValue)
            {
                arange = new Tuple<long, long>(startValue, endValue);
                return true;
            }

            return false;
        }
    }
}