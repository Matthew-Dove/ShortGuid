using System;

namespace ShortGuid.Core
{
    /// <summary>
	///		Represents a globally unique identifier (GUID) with a shorter string value.
	/// </summary>
	public class ShortGuid
    {
        #region Static

        /// <summary>
        ///		A read-only instance of the <see cref="ShortGuid"/> class whose value is guaranteed to be all zeroes. 
        /// </summary>
        public static readonly ShortGuid Empty = new ShortGuid(Guid.Empty);

        #endregion

        #region Contructors

        /// <summary>
        ///		Creates a <see cref="ShortGuid"/> from a base64 encoded string.
        /// </summary>
        /// <param name="value">The encoded Guid as a base64 string.</param>
        public ShortGuid(string value)
        {
            if (value?.Length != 22) throw new ArgumentOutOfRangeException(nameof(value), "Value must contain exactly 22 characters!");
            Value = value;
            Guid = Decode(value);
        }

        /// <summary>
        ///		Creates a <see cref="ShortGuid"/> from a <see cref="Guid"/>.
        /// </summary>
        /// <param name="guid">The <see cref="Guid"/> to encode.</param>
        public ShortGuid(Guid guid)
        {
            Value = Encode(guid);
            Guid = guid;
        }

        /// <inheritdoc />
        /// <summary>
        ///		Creates a <see cref="ShortGuid"/> with a new generated <see cref="Guid"/>.
        /// </summary>
        public ShortGuid() : this(Guid.NewGuid()) { }

        #endregion

        #region Properties

        /// <summary>
        ///		Gets/sets the underlying <see cref="Guid"/>.
        /// </summary>
        public Guid Guid { get; }

        /// <summary>
        ///		Gets/sets the underlying base64 encoded string.
        /// </summary>
        public string Value { get; }

        #endregion

        #region ToString

        /// <summary>
        ///		Returns the base64 encoded guid as a string.
        /// </summary>
        /// <returns>The Value.</returns>
        public override string ToString() => Value;

        #endregion

        #region Equals

        /// <summary>
        ///		Returns a value indicating whether this instance and a specified Object represent the same type and value.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns>True if equal, false if not.</returns>
        public override bool Equals(object obj)
        {
            var equal = false;

            switch (obj)
            {
                case ShortGuid sguid:
                    equal = Guid.Equals(sguid.Guid);
                    break;
                case Guid guid:
                    equal = Guid.Equals(guid);
                    break;
                case string str:
                    equal = Guid.Equals(new ShortGuid(str).Guid);
                    break;
            }
            return equal;
        }

        #endregion

        #region GetHashCode

        /// <summary>
        ///		Returns the HashCode for underlying <see cref="Guid"/>.
        /// </summary>
        /// <returns>HashCode for underlying <see cref="Guid"/>.</returns>
        public override int GetHashCode() => Guid.GetHashCode();

        #endregion

        #region NewGuid

        /// <summary>
        ///		Initialises a new instance of the <see cref="ShortGuid"/> class.
        /// </summary>
        /// <returns>A new instance of the <see cref="ShortGuid"/> class.</returns>
        public static ShortGuid NewGuid() => new ShortGuid(Guid.NewGuid());

        #endregion

        #region Encode

        /// <summary>
        ///		Creates a new instance of a <see cref="Guid"/> using the string value, then returns the base64 encoded version of the <see cref="Guid"/>.
        /// </summary>
        /// <param name="value">An actual <see cref="Guid"/> string (i.e. not a <see cref="ShortGuid"/>).</param>
        /// <returns>The base64 encoded version of the <see cref="Guid"/>.</returns>
        public static string Encode(string value) => Encode(new Guid(value));

        /// <summary>
        ///		Encodes the given <see cref="Guid"/> as a base64 string that is 22 characters long.
        /// </summary>
        /// <param name="guid">The <see cref="Guid"/> to encode.</param>
        /// <returns>The given <see cref="Guid"/> as a base64 string.</returns>
        public static string Encode(Guid guid) => Convert.ToBase64String(guid.ToByteArray()).Replace("/", "_").Replace("+", "-").Substring(0, 22);

        #endregion

        #region Decode

        /// <summary>
        ///		Decodes the given base64 string.
        /// </summary>
        /// <param name="value">The base64 encoded string of a <see cref="Guid"/>.</param>
        /// <returns>A new <see cref="Guid"/>.</returns>
        public static Guid Decode(string value) => new Guid(Convert.FromBase64String(value.Replace("_", "/").Replace("-", "+") + "=="));

        #endregion

        #region Operators

        /// <summary>
        ///		Determines if both <see cref="ShortGuid"/>s have the same underlying <see cref="Guid"/> value.
        /// </summary>
        /// <param name="x">A <see cref="ShortGuid"/> to be compared.</param>
        /// <param name="y">A <see cref="ShortGuid"/> to be compared.</param>
        /// <returns>A boolean corresponding to the equality of the two given <see cref="ShortGuid"/>s.</returns>
        public static bool operator ==(ShortGuid x, ShortGuid y) => x == null ? y == null : x.Guid == y?.Guid;

        /// <summary>
        ///		Determines if both <see cref="ShortGuid"/>s do not have the same underlying Guid value.
        /// </summary>
        /// <param name="x">A <see cref="ShortGuid"/> to be compared.</param>
        /// <param name="y">A <see cref="ShortGuid"/> to be compared.</param>
        /// <returns>A boolean corresponding to the inequality of the two given <see cref="ShortGuid"/>s.</returns>
        public static bool operator !=(ShortGuid x, ShortGuid y) => !(x == y);

        /// <summary>
        ///		Implicitly converts the <see cref="ShortGuid"/> to it's string equivilent.
        /// </summary>
        /// <param name="shortGuid">A <see cref="ShortGuid"/> to be cast.</param>
        /// <returns>The string representation of the <see cref="ShortGuid"/>.</returns>
        public static implicit operator string(ShortGuid shortGuid) => shortGuid.Value;

        /// <summary>
        ///		Implicitly converts the <see cref="ShortGuid"/> to it's <see cref="Guid"/> equivilent.
        /// </summary>
        /// <param name="shortGuid">A <see cref="ShortGuid"/> to be cast.</param>
        /// <returns>The <see cref="Guid"/> representation of the <see cref="ShortGuid"/>.</returns>
        public static implicit operator Guid(ShortGuid shortGuid) => shortGuid.Guid;

        /// <summary>
        /// Implicitly converts the string to a <see cref="ShortGuid"/>.
        /// </summary>
        /// <param name="shortGuid">The string represenation of the <see cref="ShortGuid"/>.</param>
        /// <returns>A <see cref="ShortGuid"/> with the equivilent value of the given string.</returns>
        public static implicit operator ShortGuid(string shortGuid) => new ShortGuid(shortGuid);

        /// <summary>
        /// Implicitly converts the <see cref="Guid"/> to a <see cref="ShortGuid"/>.
        /// </summary>
        /// <param name="guid">A <see cref="Guid"/> to be cast.</param>
        /// <returns>A <see cref="ShortGuid"/> with the equivilent value of the given <see cref="Guid"/>.</returns>
        public static implicit operator ShortGuid(Guid guid) => new ShortGuid(guid);

        #endregion
    }
}
