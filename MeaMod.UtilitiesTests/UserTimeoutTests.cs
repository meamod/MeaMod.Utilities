namespace MeaMod.Utilities.Tests
{
    [TestClass]
    public class UserTimeoutTests
    {
        [TestMethod]
        public void GetLastInputTimeIsNeverNegative()
        {
            int first = UserTimeout.GetLastInputTime();
            int second = UserTimeout.GetLastInputTime();

            Assert.IsTrue(first >= 0, first.ToString());
            Assert.IsTrue(second >= 0, second.ToString());
        }
    }
}
