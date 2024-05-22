using System.Text;
using System;
using System.Buffers.Text;

namespace ShortGuid.Core
{
    public class SuperGuid
    {
        public string GetSuperGuid() => ZeroAllocationShortGuid();

        private static string ZeroAllocationShortGuid()
        {
            const byte FORWARD_SLASH = 47;
            const byte PLUS = 43;
            const byte MINUS = 45;
            const byte UNDERSCORE = 95;

            // New guid bytes.
            Span<byte> guidBytes = stackalloc byte[16];
            Guid.NewGuid().TryWriteBytes(guidBytes);

            // Convert bytes to base64.
            Span<byte> encodedBytes = stackalloc byte[24];
            Base64.EncodeToUtf8(guidBytes, encodedBytes, out _, out _);

            // Url encode the base64.
            for (int i = 0; i < 22; i++)
            {
                var b = encodedBytes[i];
                if (b == FORWARD_SLASH) encodedBytes[i] = MINUS; // Replace("/", "-") 
                else if (b == PLUS) encodedBytes[i] = UNDERSCORE; // Replace("+", "_")
            }

            // Get the bytes in string format, drop the end padding.
            string base64 = Encoding.UTF8.GetString(encodedBytes.Slice(0, 22));
            return base64;
        }
    }
}
