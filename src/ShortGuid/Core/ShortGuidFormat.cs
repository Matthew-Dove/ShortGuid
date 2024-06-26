namespace ShortGuid.Core
{
    /// <summary>Options for controlling the string value of ShortGuid.</summary>
    public enum ShortGuidFormat
    {
        /// <summary>A 22 character base64 encoded, url-safe string.</summary>
        ShortGuid,

        /// <summary>The Guid ToString() format "D", 32 lowercase hexadecimal digits with dashes.</summary>
        D,

        /// <summary>The Guid ToString() format "N", 32 lowercase hexadecimal digits without dashes.</summary>
        N
    }
}
