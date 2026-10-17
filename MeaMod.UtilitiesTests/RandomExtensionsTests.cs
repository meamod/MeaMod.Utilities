using System.Numerics;

namespace MeaMod.Utilities.Tests
{
    [TestClass]
    public class RandomExtensionsTests
    {
        /// <summary>Returns a fixed fraction so the scaling arithmetic can be checked exactly.</summary>
        private sealed class FixedRandom(double fraction) : RandomExtensions
        {
            public override double NextDouble() => fraction;
        }

        [TestMethod]
        public void TinyFractionNoLongerThrows()
        {
            // 1.234E-06 formats with "G17" as "1.234E-06"; the old string-slicing approach then failed to parse it.
            var random = new FixedRandom(1.234e-6);

            BigInteger result = random.CreateRandomInRange(0, 1_000_000_000);

            // 1.234e-6 is not exactly representable, so the scaled integer division may floor to 1233.
            Assert.IsTrue(result == 1233 || result == 1234, result.ToString());
        }

        [TestMethod]
        public void ZeroFractionReturnsMinimum()
        {
            Assert.AreEqual(new BigInteger(-5), new FixedRandom(0d).CreateRandomInRange(-5, 5));
        }

        [TestMethod]
        public void HalfFractionReturnsMidpoint()
        {
            Assert.AreEqual(new BigInteger(50), new FixedRandom(0.5).CreateRandomInRange(0, 100));
        }

        [TestMethod]
        public void FractionJustBelowOneStaysBelowMaximum()
        {
            BigInteger result = new FixedRandom(0.99999999999999989).CreateRandomInRange(0, 100);

            Assert.IsTrue(result < 100, result.ToString());
            Assert.IsTrue(result >= 99, result.ToString());
        }

        [TestMethod]
        public void ResultsStayInRange()
        {
            var random = new RandomExtensions();
            BigInteger min = BigInteger.Parse("-1000000000000000000000000000000");
            BigInteger max = BigInteger.Parse("1000000000000000000000000000000");

            for (int i = 0; i < 10_000; i++)
            {
                BigInteger value = random.CreateRandomInRange(min, max);
                Assert.IsTrue(value >= min && value <= max, value.ToString());
            }
        }
    }
}
