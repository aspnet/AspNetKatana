namespace Microsoft.Owin.Compression.Storage
{
    public struct CompressedKey
    {
        // TODO: should storage key vary by less-than this info?
        // should static file middleware kill querystring to improve hit efficiency?
        public string RequestPath { get; set; }
        public string RequestQueryString { get; set; }
        public string RequestMethod { get; set; }

        public string ETag { get; set; }
        public long ContentLength { get; set; }
        public string Compression { get; set; }

    }
}