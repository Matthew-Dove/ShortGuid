using System;

namespace SrtGuid.Core
{
    /// <summary>
    /// A globally unique identifier (GUID) with a shorter string representation.
    /// The resulting string is a url safe, base64 encoded, with an optional flags value (up to 6 bits).
    /// </summary>
	public readonly struct ShortGuid : IEquatable<ShortGuid>
    {
        /// <summary>
        /// The "empty" instance of a ShortGuid contains the version, and variant markers in their respective bytes.
        /// <para>i.e. "00000000-0000-4000-8000-000000000000".</para>
        /// </summary>
        public static readonly Guid Empty = EmptyShortGuid();

        // A ShortGuid instance to use when initialization fails.
        public static readonly ShortGuid _default = new ShortGuid(Guid.Empty, default(int), default(string));

        /// <summary>The underlying Guid for the ShortGuid.</summary>
        public Guid Guid { get; }

        /// <summary>The 6 bits available to set on the underlying Guid (0 - 63).</summary>
        public int Flags { get; }

        /// <summary>The ShortGuid - a url safe, base64 encoded Guid.</summary>
        public string Value { get; }

        /// <summary>Creates a ShortGuid, from a base64 encoded, url safe string.</summary>
        public ShortGuid(string value)
        {
            var sg = value.ToGuid();
            Guid = sg.Guid;
            Flags = sg.Flags;
            Value = value;
        }

        private ShortGuid(Guid guid, int flags, string value)
        {
            Guid = guid;
            Flags = flags;
            Value = value;
        }

        /// <summary>Creates a ShortGuid, generating a new Guid.</summary>
        public ShortGuid() : this(Guid.NewGuid()) { }

        /// <summary>Creates a ShortGuid, from an existing Guid.</summary>
        public ShortGuid(Guid guid) : this(guid, default(int)) { }

        /// <summary>Creates a ShortGuid, generating a new Guid, with flags.</summary>
        public ShortGuid(int flags) : this(Guid.NewGuid(), flags) { }

        /// <summary>Creates a ShortGuid, from an existing Guid, with flags.</summary>
        public ShortGuid(Guid guid, int flags)
        {
            var sg = guid.ToShortGuid(flags);
            Guid = guid;
            Flags = flags;
            Value = sg;
        }

        public void Deconstruct(out Guid guid, out int flags, out string value)
        {
            guid = Guid;
            flags = Flags;
            value = Value;
        }

        private static Guid EmptyShortGuid()
        {
            const int GUID_LENGTH = 16; // The number of bytes in a GUID.

            Span<byte> emptyBytes = stackalloc byte[GUID_LENGTH];
            Guid.Empty.TryWriteBytes(emptyBytes);

            emptyBytes[7] = 0b01000000; // The first 4 bits of the version byte are: "0100".
            emptyBytes[8] = 0b10000000; // The first 2 bits of the variant byte are: "10".

            return new Guid(emptyBytes);
        }

        /// <summary>Creates a ShortGuid, generating a new Guid.</summary>
        public static ShortGuid NewGuid() => new ShortGuid(Guid.NewGuid());

        /// <summary>Parses a ShortGuid value.</summary>
        public static ShortGuid Parse(string value) => new ShortGuid(value);

        /// <summary>Parses a ShortGuid value, in a safe manner.</summary>
        public static bool TryParse(string value, out ShortGuid shortGuid)
        {
            shortGuid = _default;

            // Assert the length is correct.
            if ((value?.Length ?? 0) != 22) return false;

            /**
             * Base64 encoded url characters:
             * A to Z
             * a to z
             * 0 to 9
             * - & _ (minus and underscore)
            **/

            // Assert the characters are in the url-safe, base64 range.
            // We can skip mod, and padding checks; since they aren't part of a ShortGuid's format.

            foreach (char c in value)
            {
                if (!(
                    (c >= 'A' && c <= 'Z') || // A to Z
                    (c >= 'a' && c <= 'z') || // a to z
                    (c >= '0' && c <= '9') || // 0 to 9
                    (c == '-') || // minus
                    (c == '_') // underscore
                ))
                {
                    return false;
                }
            }

            // If we get this far, the only likely remaining error would be having an empty ShortGuid value (which would have to be crafted maliciously).
            // We need to parse it to find that out though.

            try { shortGuid = Parse(value); }
            catch { return false; }

            return true;
        }

        public override string ToString() => Value;

        public string ToString(ShortGuidFormat format)
        {
            return format switch
            {
                ShortGuidFormat.ShortGuid => Value,
                ShortGuidFormat.D => Guid.ToString("D", null),
                ShortGuidFormat.N => Guid.ToString("N", null),
                _ => Value
            };
        }

        public override bool Equals(object obj) => obj != null && obj is ShortGuid sg && Equals(sg);
        public bool Equals(ShortGuid other) => Guid.Equals(other.Guid);

        public override int GetHashCode() => Guid.GetHashCode();

        public static bool operator ==(ShortGuid x, ShortGuid y) => x.Guid.Equals(y.Guid);
        public static bool operator !=(ShortGuid x, ShortGuid y) => !(x == y);
    }
}
