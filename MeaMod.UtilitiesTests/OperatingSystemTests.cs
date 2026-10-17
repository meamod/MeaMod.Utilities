using MmOperatingSystem = MeaMod.Utilities.OperatingSystem;

namespace MeaMod.Utilities.Tests
{
    [TestClass]
    [OSCondition(OperatingSystems.Windows)]
    public class OperatingSystemTests
    {
        [TestMethod]
        public void DetectsWindows()
        {
            Assert.IsTrue(MmOperatingSystem.IsWindows());
            Assert.IsFalse(MmOperatingSystem.IsMacOS());
            Assert.IsFalse(MmOperatingSystem.IsLinux());
        }
    }
}
