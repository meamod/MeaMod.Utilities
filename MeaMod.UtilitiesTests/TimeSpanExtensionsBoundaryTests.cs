namespace MeaMod.Utilities.Tests
{
    /// <summary>
    /// Exactly one day / hour / minute used to fall through to the next smaller unit and print "0 hours", "00:00" etc.
    /// </summary>
    [TestClass]
    public class TimeSpanExtensionsBoundaryTests
    {
        [TestMethod]
        public void ToReadableStringDaysHoursMSExactlyOneDay()
        {
            Assert.AreEqual("1 day", TimeSpan.FromDays(1).ToReadableStringDaysHoursMS());
        }

        [TestMethod]
        public void ToReadableStringDaysHoursMSExactlyOneHour()
        {
            Assert.AreEqual("1 hour", TimeSpan.FromHours(1).ToReadableStringDaysHoursMS());
        }

        [TestMethod]
        public void ToReadableStringDaysHoursMSExactlyTwoDays()
        {
            Assert.AreEqual("2 days", TimeSpan.FromDays(2).ToReadableStringDaysHoursMS());
        }

        [TestMethod]
        public void ToReadableStringDaysHoursMSJustUnderOneHour()
        {
            Assert.AreEqual("59:59", new TimeSpan(0, 0, 59, 59).ToReadableStringDaysHoursMS());
        }

        [TestMethod]
        public void ToReadableAgoStringExactlyOneDay()
        {
            Assert.AreEqual("1 day ago", TimeSpan.FromDays(1).ToReadableAgoString(10));
        }

        [TestMethod]
        public void ToReadableAgoStringExactlyOneHour()
        {
            Assert.AreEqual("1 hour ago", TimeSpan.FromHours(1).ToReadableAgoString(10));
        }

        [TestMethod]
        public void ToReadableAgoStringExactlyOneMinute()
        {
            Assert.AreEqual("1 minute ago", TimeSpan.FromMinutes(1).ToReadableAgoString(10));
        }

        [TestMethod]
        public void ToReadableAgoStringSeconds()
        {
            Assert.AreEqual("30 seconds ago", TimeSpan.FromSeconds(30).ToReadableAgoString(10));
        }

        [TestMethod]
        public void ToReadableAgoStringJustNow()
        {
            Assert.AreEqual("just now", TimeSpan.FromSeconds(5).ToReadableAgoString(10));
        }
    }
}
