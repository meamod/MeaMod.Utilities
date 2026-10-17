using MeaMod.Utilities.BaseEncoding;

namespace MeaMod.Utilities.Tests
{
    [TestClass]
    public class Base36Tests
    {
        private const string LongMax = "1Y2P0IJ32E8E7";
        private const string LongMin = "-1Y2P0IJ32E8E8";

        [TestMethod]
        public void CompareUsesStringCompareSemantics()
        {
            // The old implementation returned the opposite sign.
            Assert.IsTrue(Base36.Compare("1", "2") < 0);
            Assert.IsTrue(Base36.Compare("2", "1") > 0);
            Assert.AreEqual(0, Base36.Compare("A", "a"));
            Assert.IsTrue(Base36.Compare("Z", "10") < 0, "35 < 36");
            Assert.IsTrue(Base36.Compare("10", "Z") > 0);
        }

        [TestMethod]
        public void CompareHandlesNegativeValues()
        {
            Assert.IsTrue(Base36.Compare("-1", "1") < 0);
            Assert.IsTrue(Base36.Compare("1", "-1") > 0);
            Assert.IsTrue(Base36.Compare("-2", "-1") < 0);
            Assert.IsTrue(Base36.Compare("-1", "-2") > 0);
            Assert.IsTrue(Base36.Compare("-10", "-Z") < 0);
        }

        [TestMethod]
        public void CompareIgnoresLeadingZeros()
        {
            Assert.AreEqual(0, Base36.Compare("007", "7"));
            Assert.IsTrue(Base36.Compare("0010", "9") > 0);
            Assert.AreEqual(0, Base36.Compare("-0", "0"));
        }

        [TestMethod]
        public void EncodeDecodeRoundTrips()
        {
            foreach (long value in new[] { 0L, 1L, -1L, 35L, 36L, 1295L, 123456789L, -987654321L, long.MaxValue, long.MinValue })
            {
                Assert.AreEqual(value, Base36.Decode(Base36.Encode(value)), Base36.Encode(value));
            }
        }

        [TestMethod]
        public void DecodeHandlesTheLongLimits()
        {
            Assert.AreEqual(long.MaxValue, Base36.Decode(LongMax));
            Assert.AreEqual(long.MinValue, Base36.Decode(LongMin));
            Assert.AreEqual(LongMax, Base36.Encode(long.MaxValue));
            Assert.AreEqual(LongMin, Base36.Encode(long.MinValue));
        }

        [TestMethod]
        public void DecodeRejectsOverflow()
        {
            Assert.ThrowsExactly<ArgumentException>(() => Base36.Decode("1Y2P0IJ32E8E8"));
            Assert.ThrowsExactly<ArgumentException>(() => Base36.Decode("-1Y2P0IJ32E8E9"));
            Assert.ThrowsExactly<ArgumentException>(() => Base36.Decode("ZZZZZZZZZZZZZZ"));
        }

        [TestMethod]
        public void WouldOverflowMatchesDecode()
        {
            Assert.IsFalse(Base36.WouldOverflow(LongMax));
            Assert.IsFalse(Base36.WouldOverflow(LongMin));
            Assert.IsFalse(Base36.WouldOverflow("0"));
            Assert.IsTrue(Base36.WouldOverflow("1Y2P0IJ32E8E8"));
            Assert.IsTrue(Base36.WouldOverflow("-1Y2P0IJ32E8E9"));
        }

        [TestMethod]
        public void DecodeAcceptsLowerCaseWhitespaceAndLeadingZeros()
        {
            Assert.AreEqual(35, Base36.Decode(" z "));
            Assert.AreEqual(36, Base36.Decode("0010"));
            Assert.AreEqual(-36, Base36.Decode("-0010"));
        }

        [TestMethod]
        public void DecodeRejectsInvalidInput()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => Base36.Decode(null!));
            Assert.ThrowsExactly<ArgumentException>(() => Base36.Decode(" "));
            Assert.ThrowsExactly<ArgumentException>(() => Base36.Decode("-"));
            Assert.ThrowsExactly<ArgumentException>(() => Base36.Decode("1.5"));
        }
    }
}
