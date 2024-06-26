using SrtGuid.Core;

namespace Tests.SrtGuid
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var sp = new ShortGuid();

            var (x, y, z) = ShortGuid.NewGuid();
        }
    }
}