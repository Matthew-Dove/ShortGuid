using ShortGuid.Core;

namespace Tests.ShortGuid
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var sp = new SG();

            var (x, y, z) = SG.NewGuid();
        }
    }
}