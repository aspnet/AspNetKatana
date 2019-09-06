namespace Microsoft.Owin
{
    /// <summary>
    /// Indicates if the client should include a cookie on "same-site" or "cross-site" requests.
    /// </summary>
    public enum SameSiteMode
    {
        /// <summary>
        /// Indicates the client should send the cookie with every requests coming from any origin.
        /// </summary>
        None = 0,
        /// <summary>
        /// Indicates the client should send the cookie with "same-site" requests, and with "cross-site" top-level navigations.
        /// </summary>
        Lax,
        /// <summary>
        /// Indicates the client should only send the cookie with "same-site" requests.
        /// </summary>
        Strict
    }
}
