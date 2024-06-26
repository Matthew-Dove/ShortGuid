using System;

namespace SrtGuid.Core
{
    /// <summary>
    /// A globally unique identifier (GUID) with a shorter string representation.
    /// The resulting string is a url safe, base64 encoded, with an optional flags value (up to 6 bits).
    /// </summary>
	public readonly struct ShortGuid : IEquatable<ShortGuid>
    {
        /// <summary>The underlying Guid for the ShortGuid.</summary>
        public Guid Guid { get; }

        /// <summary>The 6 bits available to set on the underlying Guid.</summary>
        public byte Flags { get; }

        /// <summary>The ShortGuid - a url safe, base64 encoded Guid.</summary>
        public string Value { get; }

        /// <summary>Creates a ShortGuid, generating a new Guid.</summary>
        public ShortGuid() : this(Guid.NewGuid()) { }

        /// <summary>Creates a ShortGuid, from a base64 encoded, url safe string.</summary>
        public ShortGuid(string value)
        {
            if (value?.Length != 22) throw new ArgumentOutOfRangeException(nameof(value), "Value must contain exactly 22 characters!");
            Value = value;
            Guid = Decode(value);
        }

        /// <summary>Creates a ShortGuid, from an existing Guid.</summary>
        public ShortGuid(Guid guid)
        {
            // TODO: Check if guid is empty.
            Value = Encode(guid);
            Guid = guid;
        }

        public void Deconstruct(out Guid guid, out byte flags, out string value)
        {
            guid = Guid;
            flags = Flags;
            value = Value;
        }

        public static ShortGuid NewGuid() => new ShortGuid(Guid.NewGuid());

        public static string Encode(string value) => Encode(new Guid(value));

        public static string Encode(Guid guid) => Convert.ToBase64String(guid.ToByteArray()).Replace("/", "_").Replace("+", "-").Substring(0, 22);

        public static Guid Decode(string value) => new Guid(Convert.FromBase64String(value.Replace("_", "/").Replace("-", "+") + "=="));

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
