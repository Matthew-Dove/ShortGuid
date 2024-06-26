using SrtGuid.Core;

namespace Tests.SrtGuid.Core
{
    public class ShortGuidTests
    {
        // https://www.binaryhexconverter.com/decimal-to-binary-converter
        // https://byjus.com/maths/hexadecimal-number-system/#:~:text=The%20hexadecimal%20number%20system%20is,digit%20represents%20a%20decimal%20value.
        // https://tonystrains.com/download/DCC_DecBiHex_Chart.pdf

        /**
         * 1) Find the 4 bits of the version flag (13th hex digit).
         * 2) Find the 2 bits (10XX) of the variant flag (17th hex digit).
         * 3) Allow a flag containing 6 bits to be set.
         * 4) Decode the guid, and flags.
        **/

        [Fact]
        public void Test1()
        {
            var sg = new ShortGuid(63);

            var guid = sg.Value.ToGuid();
        }

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
    }
}
