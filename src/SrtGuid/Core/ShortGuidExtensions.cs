using SrtGuid.Internal;
using System.Buffers.Text;
using System.Text;
using System;

namespace SrtGuid.Core
{
    public static class ShortGuidExtensions
    {
        // Url targets, and their safe replacements.
        private const byte FORWARD_SLASH = 47; // Not url safe.
        private const byte PLUS = 43; // Not url safe.
        private const byte MINUS = 45; // Url safe.
        private const byte UNDERSCORE = 95; // Url safe.

        // GUID conversions.
        private const int GUID_LENGTH = 16; // The number of bytes in a GUID.
        private const int BASE64_LENGTH = 22; // The length of the GUID bytes in base64 format, with the padding dropped.
        private const int BASE64_PADDING_LENGTH = 24; // The value's length with the base64 padding suffix included i.e. "==".
        private const byte PADDING = 61; // The equals sign i.e. "=".

        // Bit masks: (0 - 63) or (0b00000000 - 0b00111111).
        private const byte MASK_LAST_FOUR = 0b00001111; // Get the last 4 bits in a byte.
        private const byte MASK_LAST_SIX = 0b00111111; // Get the last 6 bits in a byte.
        private const byte MASK_FIVE_AND_SIX = 0b00110000; // Get positions 5, and 6 of a byte (reading from the lowest bit - right to left).
        private const byte MASK_FIRST_FOUR = 0b11110000; // Get the first 4 bits in a byte.
        private const byte MASK_FIRST_TWO = 0b11000000; // Get the first 2 bits in a byte.
        private const byte MASK_VERSION = 0b01000000; // The first 4 bits of the version byte are: "0100".
        private const byte MASK_VARIANT = 0b10000000; // The first 2 bits of the variant byte are: "10".

        // Flag constraints.
        private const int FLAGS_MIN = 0; // The lower limit for the flags value.
        private const int FLAGS_MAX = 63; // The upper limit we can store in the flags value - 4 bits from version, and 2 bits from variant i.e. 2^6.
        private const byte FLAGS_DEFAULT = 0; // The default value for flags, when not explicitly set.

        /// <summary>Creates a ShortGuid using the provided Guid, and Flags.</summary>
        public static string ToShortGuid(this Guid guid, int flags)
        {
            if (Guid.Empty.Equals(guid)) Throw.ArgumentOutOfRangeException(nameof(guid), guid, "The guid cannot be empty.");
            if (ShortGuid.Empty.Equals(guid)) Throw.ArgumentOutOfRangeException(nameof(guid), guid, "The guid cannot be an empty ShortGuid.");
            if (flags < FLAGS_MIN || flags > FLAGS_MAX) Throw.ArgumentOutOfRangeException(nameof(flags), flags, "Value must exist between [0, 63] (inclusive).");
            return CreateShortGuid(guid, (byte)flags);
        }

        /// <summary>Creates a ShortGuid using the provided Guid, with default flags.</summary>
        public static string ToShortGuid(this Guid guid)
        {
            return ToShortGuid(guid, FLAGS_DEFAULT);
        }

        /// <summary>Creates a ShortGuid using the provided Guid, and Flags.</summary>
        public static string ToShortGuid<TFlags>(this Guid guid, TFlags flags) where TFlags : Enum, IConvertible
        {
            return ToShortGuid(guid, flags.ToInt32(null));
        }

        /**
         * 1) Find the 4 bits of the version flag (13th hex digit).
         * 2) Find the 2 bits (10XX) of the variant flag (17th hex digit).
         * 3) Replace the 6 available bits with a provided flags value.
         * 4) Encode the guid (and flags), into a base64, url-safe string.
        **/

        // Example version (4), and variant (8) positions in a hex formatted guid: "00000000-0000-4000-8000-000000000000".

        private static string CreateShortGuid(Guid guid, byte flags)
        {
            // Create new guid bytes.
            Span<byte> guidBytes = stackalloc byte[GUID_LENGTH];
            guid.TryWriteBytes(guidBytes);

            // Pack 6 bits from flags (0 - 63), into the version, and variant guid bytes.
            var version = guidBytes[7]; // The first 4 bits of the 8th byte in a V4 Guid are: 0100 (or 4 in hex).
            var variant = guidBytes[8]; // The first 2 bits of the 9th byte in a V4 Guid are: 10 (or between 8, and b in hex - depending on the last 2 bits).

            // Pack 4 bits from flags into the version byte.
            var versionLast4 = (byte)(version & MASK_LAST_FOUR); // Get the last 4 bits of the version byte.
            var versionFlags = (byte)(flags & MASK_LAST_FOUR); // Get the last 4 bits of the flags byte.
            var versionBitShift = (byte)(versionFlags << 4); // Shift the flag bits all the way to the left.
            var versionResult = (byte)(versionLast4 | versionBitShift); // Put the flag bits into the first 4 bits of the version byte.

            // Pack 2 bits from flags into the variant byte.
            var variantLast6 = (byte)(variant & MASK_LAST_SIX); // Get the last 6 bits of the variant byte.
            var variantFlags = (byte)(flags & MASK_FIVE_AND_SIX); // Get the last 5th, and 5th bits of the flags byte (reading right to left).
            var variantBitShift = (byte)(variantFlags << 2); // Shift the flag bits all the way to the left.
            var variantResult = (byte)(variantLast6 | variantBitShift); // Put the flag bits into the first 4 bits of the version byte.

            // Replace the version, and variant guid bytes with the packed flag values.
            guidBytes[7] = versionResult;
            guidBytes[8] = variantResult;

            // Convert bytes to base64.
            Span<byte> encodedBytes = stackalloc byte[BASE64_PADDING_LENGTH];
            Base64.EncodeToUtf8(guidBytes, encodedBytes, out _, out _);

            // Url encode the base64.
            for (int i = 0; i < BASE64_LENGTH; i++)
            {
                if (encodedBytes[i] == FORWARD_SLASH) encodedBytes[i] = MINUS; // Replace("/", "-") 
                else if (encodedBytes[i] == PLUS) encodedBytes[i] = UNDERSCORE; // Replace("+", "_")
            }

            // Get the bytes in string format, trim the end padding.
            return Encoding.UTF8.GetString(encodedBytes.Slice(0, BASE64_LENGTH));
        }

        /// <summary>Extracts the Guid, and Flags from the ShortGuid.</summary>
        public static bool TryParseToGuid(this string shortGuid, out (Guid Guid, int Flags) guid)
        {
            guid = (Guid.Empty, default(int));

            // Assert the length is correct.
            if ((shortGuid?.Length ?? 0) != 22) return false;

            /**
             * Base64 encoded url characters:
             * A to Z
             * a to z
             * 0 to 9
             * - & _ (minus and underscore)
            **/

            // Assert the characters are in the url-safe, base64 range.
            // We can skip mod, and padding checks; since they aren't part of a ShortGuid's format.

            foreach (char c in shortGuid)
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

            try { guid = shortGuid.ToGuid(); }
            catch { return false; }

            return true;
        }

        public static bool TryParseToGuid<TFlags>(this string shortGuid, out (Guid Guid, TFlags Flags) guid) where TFlags : Enum, IConvertible
        {
            guid = (Guid.Empty, default(TFlags));
            if (TryParseToGuid(shortGuid, out var result))
            {
                var flags = ConvertToFlagsEnum<TFlags>(result.Flags);
                guid = (result.Guid, flags);
                return true;
            }
            return false;
        }

        /// <summary>Extracts the Guid, and Flags from the ShortGuid.</summary>
        public static (Guid Guid, int Flags) ToGuid(this string shortGuid)
        {
            if (shortGuid?.Length != BASE64_LENGTH) Throw.ArgumentOutOfRangeException(nameof(shortGuid), shortGuid?.Length ?? 0, "Length must be exactly 22.");
            return ExtractGuid(shortGuid);
        }

        /// <summary>Extracts the Guid, and Flags from the ShortGuid.</summary>
        public static (Guid Guid, TFlags Flags) ToGuid<TFlags>(this string shortGuid) where TFlags : Enum, IConvertible
        {
            var (g, f) = ToGuid(shortGuid);
            var flags = ConvertToFlagsEnum<TFlags>(f);
            return (g, flags);
        }

        /// <summary>
        /// Flags only exists the range of 0 - 63, but the underlying enum implementation could be any numeric data type.
        /// <para>i.e. byte, sbyte, short, ushort, int, uint, long, or ulong.</para>
        /// </summary>
        private static TFlags ConvertToFlagsEnum<TFlags>(int flags) where TFlags : Enum, IConvertible
        {
            return default(TFlags).GetTypeCode() switch
            {
                TypeCode.Byte => (TFlags)(object)(byte)flags,
                TypeCode.Int16 => (TFlags)(object)(short)flags,
                TypeCode.Int32 => (TFlags)(object)flags,
                TypeCode.Int64 => (TFlags)(object)(long)flags,
                TypeCode.SByte => (TFlags)(object)(sbyte)flags,
                TypeCode.UInt16 => (TFlags)(object)(ushort)flags,
                TypeCode.UInt32 => (TFlags)(object)(uint)flags,
                TypeCode.UInt64 => (TFlags)(object)(ulong)flags,
                _ => Throw.ArgumentOutOfRangeException<TFlags>(nameof(flags), flags, $"Unable to convert underlying flags type: {default(TFlags).GetTypeCode()}.")
            };
        }

        /**
         * 1) Decode the base64, url-safe string back into their original bytes.
         * 2) Extract the 4 bits of the flags value from the version byte.
         * 3) Extract the 2 bits of the flags value from the variant byte.
         * 4) Combine the flags data back into a single byte, and re-create the guid.
        **/

        private static (Guid Guid, int Flags) ExtractGuid(string shortGuid)
        {
            // String to UTF-8 bytes.
            var utf8Bytes = Encoding.UTF8.GetBytes(shortGuid);
            Array.Resize(ref utf8Bytes, BASE64_PADDING_LENGTH);
            Span<byte> encodedBytes = utf8Bytes;

            // Decode the url-safe characters to standard Base64.
            for (int i = 0; i < encodedBytes.Length; i++)
            {
                if (encodedBytes[i] == MINUS) encodedBytes[i] = FORWARD_SLASH; // Replace("-", "/") 
                else if (encodedBytes[i] == UNDERSCORE) encodedBytes[i] = PLUS; // Replace("_", "+")
            }

            // Add the padding back.
            encodedBytes[BASE64_PADDING_LENGTH - 2] = PADDING;
            encodedBytes[BASE64_PADDING_LENGTH - 1] = PADDING;

            // UTF-8 bytes into Base64.
            Span<byte> guidBytes = stackalloc byte[GUID_LENGTH];
            Base64.DecodeFromUtf8(encodedBytes, guidBytes, out _, out _);

            // Extract flags from the version, and variant guid bytes.
            var version = guidBytes[7];
            var variant = guidBytes[8];

            // Extract 4 flag bits from the version byte.
            var versionFirst4 = (byte)(version & MASK_FIRST_FOUR); // Get the first 4 bits of the version byte.
            var versionBitShift = (byte)(versionFirst4 >> 4); // Shift the flag bits all the way to the right.
            var versionLast4 = (byte)(version & MASK_LAST_FOUR); // Get the last 4 bits of the version byte.
            var versionResult = (byte)(versionLast4 | MASK_VERSION); // Set the first 4 bits of the version byte to "0100".

            // Extract 2 flag bits from the variant byte.
            var variantFirst2 = (byte)(variant & MASK_FIRST_TWO); // Get the first 2 bits of the variant byte.
            var variantBitShift = (byte)(variantFirst2 >> 2); // Shift the flag bits over to positions 5, and 6 (reading right to left).
            var variantLast6 = (byte)(variant & MASK_LAST_SIX); // Get the last 6 bits of the variant byte.
            var variantResult = (byte)(variantLast6 | MASK_VARIANT); // Set the first 2 bits of the variant byte to "10".

            var flags = (byte)(variantBitShift | versionBitShift); // The original flags value (0 - 63).

            // Set the version, and variant bits back to their constant values.
            guidBytes[7] = versionResult;
            guidBytes[8] = variantResult;

            // Convert the bytes into a GUID.
            var guid = new Guid(guidBytes);
            if (ShortGuid.Empty.Equals(guid)) Throw.ArgumentOutOfRangeException(nameof(shortGuid), guid, "ShortGuid cannot be empty.");
            return (guid, flags);
        }
    }
}
