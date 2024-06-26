using System;

namespace SrtGuid.Internal
{
    // Jit will keep these methods cold, until they are used (as long as they only throw exceptions as a single instruction).
    internal static class Throw
    {
        public static void ArgumentOutOfRangeException(string name, object value, string message) => throw new ArgumentOutOfRangeException(name, value, message);
    }
}
