namespace MeaMod.Utilities.Tests
{
    [TestClass]
    public class TimeSpanExtensionsFormatTests
    {
        [TestMethod]
        public void ToReadableAgeStringIsWholeYears()
        {
            Assert.AreEqual("2", TimeSpan.FromDays(731).ToReadableAgeString());
            Assert.AreEqual("0", TimeSpan.FromDays(100).ToReadableAgeString());
        }

        [TestMethod]
        public void ToReadableStringListsEveryNonZeroPart()
        {
            Assert.AreEqual("1 day, 1 hour, 1 minute, 1 second", new TimeSpan(1, 1, 1, 1).ToReadableString());
            Assert.AreEqual("2 days, 3 hours, 4 minutes, 5 seconds", new TimeSpan(2, 3, 4, 5).ToReadableString());
            Assert.AreEqual("2 hours", new TimeSpan(0, 2, 0, 0).ToReadableString());
            Assert.AreEqual("5 minutes", new TimeSpan(0, 0, 5, 0).ToReadableString());
            Assert.AreEqual("0 seconds", TimeSpan.Zero.ToReadableString());
        }

        [TestMethod]
        public void ToReadableShortStringOmitsLeadingZeroParts()
        {
            Assert.AreEqual("34:12", new TimeSpan(0, 0, 34, 12).ToReadableShortString());
            Assert.AreEqual("01:34:12", new TimeSpan(0, 1, 34, 12).ToReadableShortString());
            Assert.AreEqual("12", new TimeSpan(0, 0, 0, 12).ToReadableShortString());
            Assert.AreEqual("02", new TimeSpan(0, 0, 0, 2).ToReadableShortString());
            Assert.AreEqual("00", TimeSpan.Zero.ToReadableShortString());
            Assert.AreEqual("01", TimeSpan.FromDays(1).ToReadableShortString());
        }

        [TestMethod]
        public void ToReadableShortStringMSAlwaysShowsMinutesAndSeconds()
        {
            Assert.AreEqual("34:12", new TimeSpan(0, 0, 34, 12).ToReadableShortStringMS());
            Assert.AreEqual("01:34:12", new TimeSpan(0, 1, 34, 12).ToReadableShortStringMS());
            Assert.AreEqual("00:12", new TimeSpan(0, 0, 0, 12).ToReadableShortStringMS());
            Assert.AreEqual("00:02", new TimeSpan(0, 0, 0, 2).ToReadableShortStringMS());
            Assert.AreEqual("02:00:00", TimeSpan.FromDays(2).ToReadableShortStringMS());
            Assert.AreEqual("00:00", TimeSpan.Zero.ToReadableShortStringMS());
        }

        [TestMethod]
        public void ToReadableShortStringHMSAlwaysShowsHours()
        {
            Assert.AreEqual("00:34:12", new TimeSpan(0, 0, 34, 12).ToReadableShortStringHMS());
            Assert.AreEqual("01:34:12", new TimeSpan(0, 1, 34, 12).ToReadableShortStringHMS());
            Assert.AreEqual("00:00:12", new TimeSpan(0, 0, 0, 12).ToReadableShortStringHMS());
            Assert.AreEqual("01:02:03:04", new TimeSpan(1, 2, 3, 4).ToReadableShortStringHMS());
            Assert.AreEqual("00:00:00", TimeSpan.Zero.ToReadableShortStringHMS());
        }

        [TestMethod]
        public void ToReadableShortStringDHMSAlwaysShowsDays()
        {
            Assert.AreEqual("1:00:34:12", new TimeSpan(1, 0, 34, 12).ToReadableShortStringDHMS());
            Assert.AreEqual("0:01:34:12", new TimeSpan(0, 1, 34, 12).ToReadableShortStringDHMS());
            Assert.AreEqual("12:00:00:00", TimeSpan.FromDays(12).ToReadableShortStringDHMS());
            Assert.AreEqual("0:00:00:00", TimeSpan.Zero.ToReadableShortStringDHMS());
        }

        [TestMethod]
        public void ToReadableAgoStringPluralises()
        {
            Assert.AreEqual("2 days ago", TimeSpan.FromDays(2).ToReadableAgoString(10));
            Assert.AreEqual("2 hours ago", TimeSpan.FromHours(2).ToReadableAgoString(10));
            Assert.AreEqual("2 minutes ago", TimeSpan.FromMinutes(2).ToReadableAgoString(10));
            Assert.AreEqual("1 second ago", TimeSpan.FromSeconds(1).ToReadableAgoString(0));
        }
    }
}
