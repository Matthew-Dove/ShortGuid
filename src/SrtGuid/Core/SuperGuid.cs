using SrtGuid.Internal;
using System.Buffers.Text;
using System.Text;
using System;

namespace SrtGuid.Core
{
    public class SuperGuid
    {
        // Url targets, and their safe replacements.
        private const byte FORWARD_SLASH = 47; // Not url safe.
        private const byte PLUS = 43; // Url safe.
        private const byte MINUS = 45; // Not url safe.
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
        private const byte MASK_VERSION = 0b01000000; // The first 4 bits of the version byte is: "0100".
        private const byte MASK_VARIANT = 0b10000000; // The first 2 bits of the variant byte is: "10".

        // Other.
        private const int MAX_FLAGS = 63; // The upper limit we can store in the flags value, 4 bits from version, and 2 bits from variant (2^6).

        public string GetSuperGuid(int flags = 0)
        {
            if (flags < 0 || flags > MAX_FLAGS) Throw.ArgumentOutOfRangeException(nameof(flags), flags, "Value must exist between [0, 63] (inclusive).");
            return CreateShortGuid((byte)flags);
        }

        private static string CreateShortGuid(byte flags)
        {
            // New guid bytes.
            Span<byte> guidBytes = stackalloc byte[GUID_LENGTH];
            Guid.NewGuid().TryWriteBytes(guidBytes);

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

        public Guid ConvertToGuid(string superGuid)
        {
            if (superGuid?.Length != BASE64_LENGTH) Throw.ArgumentOutOfRangeException(nameof(superGuid), superGuid?.Length ?? 0, "Length must be exactly 22.");

            // String to UTF-8 bytes.
            var utf8Bytes = Encoding.UTF8.GetBytes(superGuid);
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
            return new Guid(guidBytes);
        }
    }
}
