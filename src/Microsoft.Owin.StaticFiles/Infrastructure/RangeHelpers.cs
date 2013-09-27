// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Owin.StaticFiles.Infrastructure
{
    internal static class RangeHelpers
    {
        // Examples:
        // bytes=0-499
        // bytes=500-
        // bytes=-500
        // bytes=0-0,-1
        // bytes=500-600,601-999
        // Any individual bad range fails the whole parse and the header should be ignored.
        internal static bool TryParseRanges(string rangeHeader, out IList<Tuple<long?, long?>> parsedRanges)
        {
            parsedRanges = null;
            if (string.IsNullOrWhiteSpace(rangeHeader)
                || !rangeHeader.StartsWith("bytes=", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string[] subRanges = rangeHeader.Substring("bytes=".Length).Replace(" ", string.Empty).Split(',');

            List<Tuple<long?, long?>> ranges = new List<Tuple<long?, long?>>();

            for (int i = 0; i < subRanges.Length; i++)
            {
                long? first = null, second = null;
                string subRange = subRanges[i];
                int dashIndex = subRange.IndexOf('-');
                if (dashIndex < 0)
                {
                    return false;
                }
                else if (dashIndex == 0)
                {
                    // -500
                    string remainder = subRange.Substring(1);
                    if (!TryParseLong(remainder, out second))
                    {
                        return false;
                    }
                }
                else if (dashIndex == (subRange.Length - 1))
                {
                    // 500-
                    string remainder = subRange.Substring(0, subRange.Length - 1);
                    if (!TryParseLong(remainder, out first))
                    {
                        return false;
                    }
                }
                else
                {
                    // 0-499
                    string firstString = subRange.Substring(0, dashIndex);
                    string secondString = subRange.Substring(dashIndex + 1, subRange.Length - dashIndex - 1);
                    if (!TryParseLong(firstString, out first) || !TryParseLong(secondString, out second)
                        || first.Value > second.Value)
                    {
                        return false;
                    }
                }

                ranges.Add(new Tuple<long?, long?>(first, second));
            }

            if (ranges.Count > 0)
            {
                parsedRanges = ranges;
                return true;
            }
            return false;
        }

        private static bool TryParseLong(string input, out long? result)
        {
            int temp;
            if (!string.IsNullOrWhiteSpace(input)
                && int.TryParse(input, NumberStyles.None, CultureInfo.InvariantCulture, out temp))
            {
                result = temp;
                return true;
            }
            result = null;
            return false;
        }

        // 14.35.1 Byte Ranges - If a syntactically valid byte-range-set includes at least one byte-range-spec whose
        // first-byte-pos is less than the current length of the entity-body, or at least one suffix-byte-range-spec
        // with a non-zero suffix-length, then the byte-range-set is satisfiable.
        internal static IList<Tuple<long?, long?>> GetSatisfiableRanges(IList<Tuple<long?, long?>> ranges, long length)
        {
            IList<Tuple<long?, long?>> satisfiableRanges = new List<Tuple<long?, long?>>(ranges.Count);
            for (int i = 0; i < ranges.Count; i++)
            {
                Tuple<long?, long?> range = ranges[i];
                if (range.Item1.HasValue && range.Item1.Value < length)
                {
                    if (!range.Item2.HasValue || range.Item2.Value >= length)
                    {
                        range = new Tuple<long?, long?>(range.Item1, (length - 1));
                    }
                    satisfiableRanges.Add(range);
                }
                else if (!range.Item1.HasValue && range.Item2.Value > 0)
                {
                    satisfiableRanges.Add(range);
                }
            }
            return satisfiableRanges;
        }

        // TODO: What about overlapping ranges like 500-700,601-999?
        // http://tools.ietf.org/html/draft-ietf-httpbis-p5-range-24
        // " A server that supports range requests MAY ignore or reject a Range
        // header field that consists of more than two overlapping ranges, or a
        // set of many small ranges that are not listed in ascending order,
        // since both are indications of either a broken client or a deliberate
        // denial of service attack (Section 6.1).  A client SHOULD NOT request
        // multiple ranges that are inherently less efficient to process and
        // transfer than a single range that encompasses the same data."
        // " When multiple ranges are requested, a server MAY coalesce any of the
        // ranges that overlap or that are separated by a gap that is smaller
        // than the overhead of sending multiple parts, regardless of the order
        // in which the corresponding byte-range-spec appeared in the received
        // Range header field.  Since the typical overhead between parts of a
        // multipart/byteranges payload is around 80 bytes, depending on the
        // selected representation's media type and the chosen boundary
        // parameter length, it can be less efficient to transfer many small
        // disjoint parts than it is to transfer the entire selected
        // representation."
        //
        // Out-of-order ranges: "14.16 Content-Range - When a client requests multiple byte-ranges in one request,
        // the server SHOULD return them in the order that they appeared in the request."
        //
        // This logic assumes these ranges are satisfiable. Adjusts ranges to be absolute and within bounds.
        internal static IList<Tuple<long?, long?>> NormalizeRanges(IList<Tuple<long?, long?>> ranges, long length)
        {
            IList<Tuple<long?, long?>> normalizedRanges = new List<Tuple<long?, long?>>(ranges.Count);
            for (int i = 0; i < ranges.Count; i++)
            {
                Tuple<long?, long?> range = ranges[i];
                long? start = range.Item1, end = range.Item2;

                if (start.HasValue)
                {
                    // start has already been validated to be in range by GetSatisfiableRanges.
                    if (end.HasValue)
                    {
                        if (end.Value >= length)
                        {
                            end = length - 1;
                        }
                    }
                    else
                    {
                        end = length - 1;
                    }
                }
                else
                {
                    // suffix range "-X" e.g. the last X bytes, resolve
                    long bytes = Math.Min(end.Value, length);
                    start = length - bytes;
                    end = start + bytes - 1;
                }
                normalizedRanges.Add(new Tuple<long?, long?>(start, end));
            }
            return normalizedRanges;
        }
    }
}
