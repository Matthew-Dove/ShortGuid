namespace SrtGuid.Core
{
    /// <summary>Options for controlling the string value of ShortGuid.</summary>
    public enum ShortGuidFormat
    {
        /// <summary>
        /// A 22 character base64 encoded, url-safe string.
        /// <para>9uc7gy-GTgUJy8rdoiXSTw</para>
        /// </summary>
        ShortGuid,

        /// <summary>
        /// The Guid ToString() format "N", 32 lowercase hexadecimal digits without dashes.
        /// <para>833be7f6c62f454e89cbcadda225d24f</para>
        /// </summary>
        N,

        /// <summary>
        /// The Guid ToString() format "D", 32 lowercase hexadecimal digits with dashes.
        /// <para>833be7f6-c62f-454e-89cb-cadda225d24f</para>
        /// </summary>
        D,

        /// <summary>
        /// The Guid ToString() format "B", 32 lowercase hexadecimal digits with dashes, enclosed in braces.
        /// <para>{833be7f6-c62f-454e-89cb-cadda225d24f}</para>
        /// </summary>
        B,

        /// <summary>
        /// The Guid ToString() format "P", 32 lowercase hexadecimal digits with dashes, enclosed in parentheses.
        /// <para>(833be7f6-c62f-454e-89cb-cadda225d24f)</para>
        /// </summary>
        P,

        /// <summary>
        /// The Guid ToString() format "X", four hexadecimal values enclosed in braces, where the fourth value is a subset of eight hexadecimal values that is also enclosed in braces.
        /// <para>{0x833be7f6,0xc62f,0x454e,{0x89,0xcb,0xca,0xdd,0xa2,0x25,0xd2,0x4f}}</para>
        /// </summary>
        X
    }
}
