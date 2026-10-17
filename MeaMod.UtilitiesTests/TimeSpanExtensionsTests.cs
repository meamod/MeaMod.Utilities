namespace MeaMod.Utilities.Tests
{
    [TestClass()]
    [OSCondition(OperatingSystems.Windows)]
    public class TimeSpanExtensionsTests
    {
        [TestMethod()]
        public void ToReadableStringMsTestDays()
        {
            var testTimeSpan = new TimeSpan(4, 3, 2, 1);
            Console.WriteLine(testTimeSpan.ToReadableStringDaysHoursMS());
            Assert.AreEqual("4 days", testTimeSpan.ToReadableStringDaysHoursMS());
        }

        [TestMethod()]
        public void ToReadableStringMsTestDay()
        {
            var testTimeSpan = new TimeSpan(1, 3, 2, 1);
            Console.WriteLine(testTimeSpan.ToReadableStringDaysHoursMS());
            Assert.AreEqual("1 day", testTimeSpan.ToReadableStringDaysHoursMS());
        }

        [TestMethod()]
        public void ToReadableStringMsTestHours()
        {
            var testTimeSpan = new TimeSpan(0, 3, 2, 1);
            Console.WriteLine(testTimeSpan.ToReadableStringDaysHoursMS());
            Assert.AreEqual("3 hours", testTimeSpan.ToReadableStringDaysHoursMS());
        }

        [TestMethod()]
        public void ToReadableStringMsTestHour()
        {
            var testTimeSpan = new TimeSpan(0, 1, 2, 1);
            Console.WriteLine(testTimeSpan.ToReadableStringDaysHoursMS());
            Assert.AreEqual("1 hour", testTimeSpan.ToReadableStringDaysHoursMS());
        }

        [TestMethod()]
        public void ToReadableStringMsTestMinute()
        {
            var testTimeSpan = new TimeSpan(0, 0, 1, 1);
            Console.WriteLine(testTimeSpan.ToReadableStringDaysHoursMS());
            Assert.AreEqual("01:01", testTimeSpan.ToReadableStringDaysHoursMS());
        }

        [TestMethod()]
        public void ToReadableStringMsTestMinutes()
        {
            var testTimeSpan = new TimeSpan(0, 0, 55, 30);
            Console.WriteLine(testTimeSpan.ToReadableStringDaysHoursMS());
            Assert.AreEqual("55:30", testTimeSpan.ToReadableStringDaysHoursMS());
        }

        [TestMethod()]
        public void ToReadableStringMsTestSeconds()
        {
            var testTimeSpan = new TimeSpan(0, 0, 0, 45);
            Console.WriteLine(testTimeSpan.ToReadableStringDaysHoursMS());
            Assert.AreEqual("00:45", testTimeSpan.ToReadableStringDaysHoursMS());
        }
    }
}
