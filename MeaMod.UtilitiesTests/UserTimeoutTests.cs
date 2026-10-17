namespace MeaMod.Utilities.Tests
{
    [TestClass]
    [OSCondition(OperatingSystems.Windows)]
    public class UserTimeoutTests
    {
        [TestMethod]
        public void GetLastInputTimeIsNeverNegative()
        {
            int first = UserTimeout.GetLastInputTime();
            int second = UserTimeout.GetLastInputTime();

            Assert.IsGreaterThanOrEqualTo(0, first, first.ToString());
            Assert.IsGreaterThanOrEqualTo(0, second, second.ToString());
        }
    }
}
