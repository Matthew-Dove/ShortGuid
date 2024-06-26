using System.Text;
using System;
using System.Buffers.Text;
using ShortGuid.Internal;

namespace ShortGuid.Core
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

        // Bit masks: (0 - 63) or (0b00000000 - 0b00111111).
        private const byte VERSION_CLEAR_MASK = 0b00001111; // Clear the version data (first 4 bits of the 8th byte).
        private const byte VARIANT_CLEAR_MASK = 0b00111111; // Clear the variant data (first 2 bits of the 9th byte).
        private const byte VERSION_EXTRACT_MASK = 0b00001111; // Get the last 4 bits of the flag byte.
        private const byte VARIANT_EXTRACT_MASK = 0b00110000; // Get positions 5, and 6 of a byte (reading from the lowest bit - right to left).

        // Other
        private const byte PADDING = 61; // The equals sign i.e. "=".
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

            var version = guidBytes[7]; // The first 4 bits of the 8th byte in a V4 Guid are: 0100 (or 4 in hex).
            var variant = guidBytes[8]; // The first 2 bits of the 9th byte in a V4 Guid are: 10 (or between 8, and b in hex - depending on the last 2 bits).

            // Pack 4 bits from flags into the version byte.
            var versionLast4 = (byte)(version & VERSION_CLEAR_MASK); // Get the last 4 bits of the version byte.
            var versionFlags = (byte)(flags & VERSION_EXTRACT_MASK); // Get the last 4 bits of the flags byte.
            var versionBitShift = (byte)(versionFlags << 4); // Shift the flag bits all the way to the left.
            var versionResult = (byte)(versionLast4 | versionBitShift); // Put the flag bits into the first 4 bits of the version byte.


            // Pack 2 bits from flags into the variant byte.



            var x = "Source Byte: " + Convert.ToString(flags, 2).PadLeft(8, '0');



            guidBytes[7] = versionResult;
            // guidBytes[8] = variantResult;


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

            // TODO: Replace version, and variant bits.

            // Convert the bytes into a GUID.
            return new Guid(guidBytes);
        }
    }
}
