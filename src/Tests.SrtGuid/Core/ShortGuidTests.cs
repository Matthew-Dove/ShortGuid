using SrtGuid.Core;

namespace Tests.SrtGuid.Core
{
    public class ShortGuidTests
    {
        private const int SG_VALUE_LEN = 22; // ShortGuids will always be the same length (in their string format), as Guids are always 16 bytes.

        // https://www.binaryhexconverter.com/decimal-to-binary-converter
        // https://byjus.com/maths/hexadecimal-number-system/#:~:text=The%20hexadecimal%20number%20system%20is,digit%20represents%20a%20decimal%20value.
        // https://tonystrains.com/download/DCC_DecBiHex_Chart.pdf

        // 2^6 = 64 (Just make this an int 0 - 64?)
        private enum Values : byte
        {
            A = 0,
            B = 1,
            C = 2,
            D = 3,
            E = 4,
            F = 5,
            G = 6
        }

        [Flags]
        private enum Flags
        {
            None = 0,
            A = 1,
            B = 2,
            C = 4
        }

        [Fact]
        public void Create_WithDefaults()
        {
            var sg1 = new ShortGuid();
            var sg2 = new ShortGuid(sg1.Value);

            Assert.NotEqual(Guid.Empty, sg2.Guid);
            Assert.NotEqual(ShortGuid.Empty, sg2.Guid);
            Assert.Equal(default(int), sg2.Flags);
            Assert.Equal(SG_VALUE_LEN, sg2.Value.Length);
        }

        [Fact]
        public void Create_WithGuid()
        {
            var guid = Guid.NewGuid();

            var sg1 = new ShortGuid(guid);
            var sg2 = new ShortGuid(sg1.Value);

            Assert.Equal(guid, sg2.Guid);
            Assert.Equal(default(int), sg2.Flags);
            Assert.Equal(SG_VALUE_LEN, sg2.Value.Length);
        }

        [Fact]
        public void Create_WithFlags()
        {
            var flags = 42;

            var sg1 = new ShortGuid(42);
            var sg2 = new ShortGuid(sg1.Value);

            Assert.NotEqual(Guid.Empty, sg2.Guid);
            Assert.NotEqual(ShortGuid.Empty, sg2.Guid);
            Assert.Equal(flags, sg2.Flags);
            Assert.Equal(SG_VALUE_LEN, sg2.Value.Length);
        }

        [Fact]
        public void Create_WithGuidAndFlags()
        {
            var guid = Guid.NewGuid();
            var flags = 42;

            var sg1 = new ShortGuid(guid, 42);
            var sg2 = new ShortGuid(sg1.Value);

            Assert.Equal(guid, sg2.Guid);
            Assert.Equal(flags, sg2.Flags);
            Assert.Equal(SG_VALUE_LEN, sg2.Value.Length);
        }

        [Fact]
        public void Can_Deconstruct_And_Reconstruct()
        {
            var guid = Guid.NewGuid();
            var flags = 42;

            var (sgGuid, sgFlags, sgValue) = ShortGuid.NewGuid(guid, flags);
            var sg = ShortGuid.NewGuid(sgValue);

            Assert.Equal(guid, sgGuid);
            Assert.Equal(flags, sgFlags);

            Assert.Equal(sgGuid, sg.Guid);
            Assert.Equal(sgFlags, sg.Flags);
            Assert.Equal(sgValue, sg.Value);

            Assert.Equal(sg, ShortGuid.NewGuid(sgGuid));
            Assert.Equal(sg, ShortGuid.NewGuid(sgGuid, sgFlags));
        }

        [Fact]
        public void Parse_ShortGuid()
        {
            var guid = Guid.NewGuid();
            var flags = 42;

            var sg1 = new ShortGuid(guid, 42);
            var sg2 = ShortGuid.Parse(sg1.Value);

            Assert.Equal(guid, sg2.Guid);
            Assert.Equal(flags, sg2.Flags);
        }

        [Fact]
        public void TryParse_ShortGuid_Ok()
        {
            var guid = Guid.NewGuid();
            var flags = 42;

            var sg1 = new ShortGuid(guid, 42);
            var isValid = ShortGuid.TryParse(sg1.Value, out ShortGuid sg2);

            Assert.True(isValid);
            Assert.Equal(guid, sg2.Guid);
            Assert.Equal(flags, sg2.Flags);
        }

        [Fact]
        public void TryParse_ShortGuid_EmptyFail()
        {
            var sg = "AAAAAAAAAAAAAAAAAAAAAA"; // An encoded empty guid.

            var isValid = ShortGuid.TryParse(sg, out ShortGuid _);

            Assert.False(isValid);
        }

        [Fact]
        public void Empty_ShortGuid()
        {
            var empty = ShortGuid.Empty.ToString("D");

            Assert.Equal("00000000-0000-4000-8000-000000000000", empty);
        }

        [Fact]
        public void Equals_ShortGuid_Guid()
        {
            var guid = Guid.NewGuid();

            var sg1 = new ShortGuid(guid);
            var sg2 = new ShortGuid(sg1.Value);

            Assert.True(sg1 == guid);
            Assert.True(sg1 == sg2);
            Assert.True(guid == sg2);
        }

        [Fact]
        public void ToString_ShortGuid()
        {
            var guid = Guid.NewGuid();
            var sg = ShortGuid.NewGuid(guid);

            Assert.Equal(guid.ToString("N"), sg.ToString(ShortGuidFormat.N));
            Assert.Equal(guid.ToString("D"), sg.ToString(ShortGuidFormat.D));
            Assert.Equal(guid.ToString("B"), sg.ToString(ShortGuidFormat.B));
            Assert.Equal(guid.ToString("P"), sg.ToString(ShortGuidFormat.P));
            Assert.Equal(guid.ToString("X"), sg.ToString(ShortGuidFormat.X));
        }
    }
}
