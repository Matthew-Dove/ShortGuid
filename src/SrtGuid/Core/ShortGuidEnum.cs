using System;

namespace SrtGuid.Core
{
    /// <summary>
    /// A globally unique identifier (GUID) with a shorter string representation.
    /// <para>The resulting string is url safe, base64 encoded, with an optional flags value (up to 6 bits).</para>
    /// </summary>
	public readonly struct ShortGuid<TFlags> : IEquatable<ShortGuid<TFlags>>, IEquatable<ShortGuid>, IEquatable<Guid> where TFlags : Enum, IConvertible
    {
        /// <summary>
        /// The "empty" instance of a ShortGuid contains the version, and variant markers in their respective bytes.
        /// <para>i.e. "00000000-0000-4000-8000-000000000000".</para>
        /// </summary>
        public static readonly Guid Empty = ShortGuid.Empty;

        /// <summary>A ShortGuid instance to use when initialization fails.</summary>
        private static readonly ShortGuid<TFlags> _default = new ShortGuid<TFlags>(Guid.Empty, default(TFlags), default(string));

        /// <summary>The underlying Guid for the ShortGuid.</summary>
        public Guid Guid { get; }

        /// <summary>The 6 bits available to set on the underlying Guid (0 - 63).</summary>
        public TFlags Flags { get; }

        /// <summary>The ShortGuid - a url safe, base64 encoded Guid.</summary>
        public string Value { get; }

        /// <summary>Creates a ShortGuid, from a base64 encoded, url safe string.</summary>
        public ShortGuid(string value)
        {
            var sg = value.ToGuid<TFlags>();
            Guid = sg.Guid;
            Flags = sg.Flags;
            Value = value;
        }

        private ShortGuid(Guid guid, TFlags flags, string value)
        {
            Guid = guid;
            Flags = flags;
            Value = value;
        }

        /// <summary>Creates a ShortGuid, generating a new Guid.</summary>
        public ShortGuid() : this(Guid.NewGuid()) { }

        /// <summary>Creates a ShortGuid, from an existing Guid.</summary>
        public ShortGuid(Guid guid) : this(guid, default(TFlags)) { }

        /// <summary>Creates a ShortGuid, generating a new Guid, with flags.</summary>
        public ShortGuid(TFlags flags) : this(Guid.NewGuid(), flags) { }

        /// <summary>Creates a ShortGuid, from an existing Guid, with flags.</summary>
        public ShortGuid(Guid guid, TFlags flags)
        {
            var sg = guid.ToShortGuid(flags);
            Guid = guid;
            Flags = flags;
            Value = sg;
        }

        public void Deconstruct(out Guid guid, out TFlags flags, out string value)
        {
            guid = Guid;
            flags = Flags;
            value = Value;
        }

        /// <summary>Creates a ShortGuid, from a base64 encoded, url safe string.</summary>
        public static ShortGuid<TFlags> NewGuid(string value) => new ShortGuid<TFlags>(value);

        /// <summary>Creates a ShortGuid, generating a new Guid.</summary>
        public static ShortGuid<TFlags> NewGuid() => new ShortGuid<TFlags>();

        /// <summary>Creates a ShortGuid, from an existing Guid.</summary>
        public static ShortGuid<TFlags> NewGuid(Guid guid) => new ShortGuid<TFlags>(guid);

        /// <summary>Creates a ShortGuid, generating a new Guid, with flags.</summary>
        public static ShortGuid<TFlags> NewGuid(TFlags flags) => new ShortGuid<TFlags>(flags);

        /// <summary>Creates a ShortGuid, from an existing Guid, with flags.</summary>
        public static ShortGuid<TFlags> NewGuid(Guid guid, TFlags flags) => new ShortGuid<TFlags>(guid, flags);

        /// <summary>Parses a ShortGuid value.</summary>
        public static ShortGuid<TFlags> Parse(string value) => new ShortGuid<TFlags>(value);

        /// <summary>Parses a ShortGuid value, in a safe manner.</summary>
        public static bool TryParse(string value, out ShortGuid<TFlags> shortGuid)
        {
            shortGuid = _default;
            if (value.TryParseToGuid<TFlags>(out var guid))
            {
                shortGuid = new ShortGuid<TFlags>(guid.Guid, guid.Flags, value);
                return true;
            }
            return false;
        }

        public override string ToString() => Value;

        public string ToString(ShortGuidFormat format)
        {
            return format switch
            {
                ShortGuidFormat.ShortGuid => Value,
                ShortGuidFormat.N => Guid.ToString("N", null),
                ShortGuidFormat.D => Guid.ToString("D", null),
                ShortGuidFormat.B => Guid.ToString("B", null),
                ShortGuidFormat.P => Guid.ToString("P", null),
                ShortGuidFormat.X => Guid.ToString("X", null),
                _ => Value
            };
        }

        // The equals check is done only on the Guid, and does not include the Flags value.
        // This is because ShortGuid is a unique Id first, with optional flags second - as a nice to have.
        public override bool Equals(object obj) => obj != null && (obj is ShortGuid sg && Equals(sg) || obj is Guid g && Equals(g));
        public bool Equals(ShortGuid other) => Guid.Equals(other.Guid);
        public bool Equals(Guid other) => Guid.Equals(other);
        public bool Equals(ShortGuid<TFlags> other) => Guid.Equals(other);

        public override int GetHashCode() => Guid.GetHashCode();

        public static bool operator ==(ShortGuid<TFlags> x, ShortGuid<TFlags> y) => x.Guid.Equals(y.Guid);
        public static bool operator !=(ShortGuid<TFlags> x, ShortGuid<TFlags> y) => !(x == y);

        public static bool operator ==(ShortGuid<TFlags> x, Guid y) => x.Guid.Equals(y);
        public static bool operator !=(ShortGuid<TFlags> x, Guid y) => !(x == y);
        public static bool operator ==(Guid x, ShortGuid<TFlags> y) => y.Guid.Equals(x);
        public static bool operator !=(Guid x, ShortGuid<TFlags> y) => !(x == y);

        public static bool operator ==(ShortGuid<TFlags> x, ShortGuid y) => x.Guid.Equals(y.Guid);
        public static bool operator !=(ShortGuid<TFlags> x, ShortGuid y) => !(x == y);
        public static bool operator ==(ShortGuid x, ShortGuid<TFlags> y) => y.Guid.Equals(x.Guid);
        public static bool operator !=(ShortGuid x, ShortGuid<TFlags> y) => !(x == y);
    }
}
