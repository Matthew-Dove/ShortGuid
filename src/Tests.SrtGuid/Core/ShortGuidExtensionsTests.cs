using SrtGuid.Core;

namespace Tests.SrtGuid.Core
{
    public class ShortGuidExtensionsTests
    {
        [Fact]
        public void ToShortGuid_Guid()
        {
            var guid = Guid.NewGuid();

            var sg = guid.ToShortGuid();
            var (g, f) = sg.ToGuid();

            Assert.Equal(guid, g);
            Assert.Equal(default, f);
        }

        [Fact]
        public void ToShortGuid_Guid_FlagsMin()
        {
            var guid = Guid.NewGuid();
            var flags = 0;

            var sg = guid.ToShortGuid(flags);
            var (g, f) = sg.ToGuid();

            Assert.Equal(guid, g);
            Assert.Equal(flags, f);
        }

        [Fact]
        public void ToShortGuid_Guid_FlagsMax()
        {
            var guid = Guid.NewGuid();
            var flags = 63;

            var sg = guid.ToShortGuid(flags);
            var (g, f) = sg.ToGuid();

            Assert.Equal(guid, g);
            Assert.Equal(flags, f);
        }

        [Fact]
        public void Validate_ToShortGuid_GuidEmpty()
        {
            var guid = Guid.Empty;

            Assert.Throws<ArgumentOutOfRangeException>(() => guid.ToShortGuid());
        }

        [Fact]
        public void Validate_ToShortGuid_ShortGuidEmpty()
        {
            var guid = ShortGuid.Empty;

            Assert.Throws<ArgumentOutOfRangeException>(() => guid.ToShortGuid());
        }

        [Fact]
        public void Validate_ToShortGuid_FlagsMin()
        {
            var guid = Guid.NewGuid();
            var flags = -1;

            Assert.Throws<ArgumentOutOfRangeException>(() => guid.ToShortGuid(flags));
        }

        [Fact]
        public void Validate_ToShortGuid_FlagsMax()
        {
            var guid = Guid.NewGuid();
            var flags = 64;

            Assert.Throws<ArgumentOutOfRangeException>(() => guid.ToShortGuid(flags));
        }

        [Fact]
        public void TryParseToGuid_LengthFail()
        {
            var sg = "AAAAAAAAAAAAAAAAAAAAAA==";

            var isValid = sg.TryParseToGuid(out _);

            Assert.False(isValid);
        }

        [Fact]
        public void TryParseToGuid_Base64Fail()
        {
            var sg = "AAAAAAAAAAAAAAAAAAAAA%";

            var isValid = sg.TryParseToGuid(out _);

            Assert.False(isValid);
        }

        [Fact]
        public void TryParseToGuid_EmptyGuidFail()
        {
            var sg = "AAAAAAAAAAAAAAAAAAAAAA";

            var isValid = sg.TryParseToGuid(out _);

            Assert.False(isValid);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("AAAAAAAAAAAAAAAAAAAAA")] // To short.
        [InlineData("AAAAAAAAAAAAAAAAAAAAAA")] // Empty Guid.
        [InlineData("AAAAAAAAAAAAAAAAAAAAAAA")] // Too Long.
        [InlineData("AAAAAAAAAAAAAAAAAAAAA%")] // Invalid base64 character.
        public void ToGuid_Guid(string sg) => Assert.Throws<ArgumentOutOfRangeException>(() => sg.ToGuid());
    }
}
