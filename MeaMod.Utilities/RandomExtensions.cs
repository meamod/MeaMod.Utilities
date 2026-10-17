using System;
using System.Numerics;

namespace MeaMod.Utilities
{
    /// <summary>
    /// Extensions to Random Class
    /// </summary>
    public class RandomExtensions : Random
    {
        // NextDouble() carries at most 53 bits of precision.
        private const int FractionBits = 53;

        /// <summary>
        /// Create Random BigInteger with range
        /// </summary>
        /// <param name="min">Lower Limit</param>
        /// <param name="max">Upper Limit</param>
        /// <returns>A value in the range <paramref name="min"/> to <paramref name="max"/>.</returns>
        public BigInteger CreateRandomInRange(BigInteger min, BigInteger max)
        {
            double fraction = this.NextDouble();

            // Guard against a subclass returning something outside [0, 1).
            if (fraction < 0d) fraction = 0d;
            if (fraction > 1d) fraction = 1d;

            // Scale the fraction to an integer numerator over 2^53. This keeps all of the precision
            // NextDouble() provides and avoids formatting the double as a string, which switches to
            // scientific notation for very small values and is culture dependent.
            var numerator = new BigInteger((long)(fraction * (1L << FractionBits)));
            var denominator = BigInteger.One << FractionBits;

            return (max - min) * numerator / denominator + min;
        }
    }
}
