using System;
using System.Globalization;
using System.Linq;
using System.Numerics;

namespace MeaMod.Utilities.BaseEncoding
{
    /// <summary>
    /// https://github.com/tallesl/net-36
    /// Base 10 to base 36 and vice versa.
    /// </summary>
    public static class Base36
    {
        private const string Digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        private const string Min = "-1Y2P0IJ32E8E8";

        private const string Max = "1Y2P0IJ32E8E7";

        /// <summary>
        /// Checks if the given value would cause a overflow (by being out of the long.MinValue to long.MaxValue range).
        /// </summary>
        /// <param name="value">Value to check</param>
        /// <returns>True if the value would cause an overflow, false otherwise</returns>
        public static bool WouldOverflow(string value)
        {
            value = Sanitize(value);
            return CompareSanitized(value, Min) < 0 || CompareSanitized(value, Max) > 0;
        }

        /// <summary>
        /// Compare two specified base 36 values and returns an integer that indicates their relative position in the
        /// sort order, similar to the string.Compare method.
        /// </summary>
        /// <param name="valueA">First value of the comparison</param>
        /// <param name="valueB">Second value of the comparison</param>
        /// <returns>
        /// Less than zero if <paramref name="valueA"/> is less than <paramref name="valueB"/>, zero if they are equal,
        /// greater than zero if <paramref name="valueA"/> is greater than <paramref name="valueB"/>.
        /// </returns>
        public static int Compare(string valueA, string valueB)
        {
            return CompareSanitized(Sanitize(valueA), Sanitize(valueB));
        }

        /// <summary>
        /// Converts from base 36 to base 10.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <returns>Value in base 10</returns>
        public static long Decode(string value)
        {
            value = Sanitize(value);

            CheckOverflow(value);

            var negative = value[0] == '-';

            value = Abs(value);

            // Accumulate in BigInteger so long.MinValue (whose magnitude does not fit in a long) decodes without
            // relying on unchecked wrap-around.
            var decoded = BigInteger.Zero;

            for (var i = 0; i < value.Length; ++i)
                decoded = decoded * Digits.Length + Digits.IndexOf(value[i]);

            return (long)(negative ? -decoded : decoded);
        }

        /// <summary>
        /// Converts from base 10 to base 36.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <returns>Value in base 36</returns>
        public static string Encode(long value)
        {
            // hard coded value due to "Negating the minimum value of a twos complement number is invalid."
            if (value == long.MinValue)
                return Min;

            var negative = value < 0;

            value = Math.Abs(value);

            var encoded = string.Empty;

            do
                encoded = Digits[(int)(value % Digits.Length)] + encoded;
            while ((value /= Digits.Length) != 0);

            return negative ? "-" + encoded : encoded;
        }

        private static string Abs(string value)
        {
            return value[0] == '-' ? value.Substring(1) : value;
        }

        private static void CheckOverflow(string value)
        {
            if (CompareSanitized(value, Min) < 0)
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture,
                    "Value \"{0}\" would overflow since it's less than long.MinValue.", value));

            if (CompareSanitized(value, Max) > 0)
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture,
                    "Value \"{0}\" would overflow since it's greater than long.MaxValue.", value));
        }

        /// <summary>
        /// Validates the input and returns it in canonical form: upper case, trimmed, no leading zeros,
        /// and "0" rather than "-0" for zero.
        /// </summary>
        private static string Sanitize(string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value), "A null string was passed.");

            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("An empty string was passed.", nameof(value));

            value = value.Trim().ToUpperInvariant();

            var negative = value[0] == '-';
            var magnitude = Abs(value);

            if (magnitude.Length == 0 || magnitude.Any(c => !Digits.Contains(c)))
                throw new ArgumentException(
                    string.Format(CultureInfo.InvariantCulture, "Invalid value: \"{0}\".", value), nameof(value));

            magnitude = magnitude.TrimStart('0');
            if (magnitude.Length == 0)
                return "0";

            return negative ? "-" + magnitude : magnitude;
        }

        /// <summary>
        /// Compares two sanitized values. Returns a negative number when <paramref name="valueA"/> is less than
        /// <paramref name="valueB"/>, zero when equal and a positive number when greater (string.Compare semantics).
        /// </summary>
        private static int CompareSanitized(string valueA, string valueB)
        {
            if (valueA == valueB)
                return 0;

            var negativeA = valueA[0] == '-';
            var negativeB = valueB[0] == '-';

            if (negativeA != negativeB)
                return negativeA ? -1 : 1;

            valueA = Abs(valueA);
            valueB = Abs(valueB);

            int magnitudeComparison = 0;

            if (valueA.Length != valueB.Length)
            {
                magnitudeComparison = valueA.Length < valueB.Length ? -1 : 1;
            }
            else
            {
                for (var i = 0; i < valueA.Length; ++i)
                {
                    var digitA = Digits.IndexOf(valueA[i]);
                    var digitB = Digits.IndexOf(valueB[i]);

                    if (digitA != digitB)
                    {
                        magnitudeComparison = digitA < digitB ? -1 : 1;
                        break;
                    }
                }
            }

            // For negative numbers the larger magnitude is the smaller number.
            return negativeA ? -magnitudeComparison : magnitudeComparison;
        }
    }
}
