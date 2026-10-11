//Written by bboyle1234 with thanks to John Alexiou @ http://stackoverflow.com/questions/16083666/make-big-and-small-numbers-human-readable/16091580#16091580
//fixed line 55 for TurboHUD

namespace Turbo.Plugins.Razor.Util
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using static System.Math;

    public static class HumanReadableDoubles
	{
        static readonly string[] humanReadableSuffixes = { "f", "a", "p", "n", "μ", "m", "", "k", "M", "B", "T", "Q", "E" };
        public static string ToHumanReadable(this double value, int numSignificantDigits) {

            // Deal with special values
            if (double.IsInfinity(value) || double.IsNaN(value) || value == 0 || numSignificantDigits <= 0)
                return value.ToString();
			
			//Razorfish - fix error System.ArgumentOutOfRangeException: 'count' must be non-negative - seems to be caused by values with leading zero
			if ((value > 0 && value < 1) || (value > -1 && value < 0))
				return value.ToString("0." + new String('#', numSignificantDigits));

            // We deal only with positive values in the code below
            var isNegative = Sign(value) < 0;
            value = Abs(value);

            // Calculate the exponent as a multiple of 3, ie -6, -3, 0, 3, 6, etc
            var exponent = (int)Floor(Log10(value) / 3) * 3;

            // Find the correct suffix for the exponent, or fall back to scientific notation
            var indexOfSuffix = exponent / 3 + 6;
            var suffix = indexOfSuffix >= 0 && indexOfSuffix < humanReadableSuffixes.Length
                ? humanReadableSuffixes[indexOfSuffix]
                : "·10^" + exponent;

            // Scale the value to the exponent, then format it to the correct number of significant digits and add the suffix
            value = value * Pow(10, -exponent);
            var numIntegerDigits = (int)Floor(Log(value, 10)) + 1;
            var numFractionalDigits = Min(numSignificantDigits - numIntegerDigits, 15);
            var format = $"{new string('0', numIntegerDigits)}.{new string('0', numFractionalDigits)}";
            var result = value.ToString(format) + suffix;

            // Handle negatives
            if (isNegative)
                result = "-" + result;

            return result;
        }

        public static double ParseHumanReadableDouble(this string expression) {
            var multiplier = 1.0;
            if (expression.Contains("·10^")) {
                var indexOfCaret = expression.LastIndexOf('^');
                multiplier = Pow(10, int.Parse(expression.Substring(indexOfCaret + 1)));
                expression = expression.Substring(0, indexOfCaret - 3);
            } else {
                var suffix = humanReadableSuffixes.SingleOrDefault(s => s.Length > 0 && expression.EndsWith(s, StringComparison.InvariantCulture)) ?? "";
                var suffixIndex = Array.IndexOf(humanReadableSuffixes, suffix); //humanReadableSuffixes.IndexOf(suffix);
                multiplier = Pow(10, 3 * (suffixIndex - 6));
                expression = expression.Replace(suffix, string.Empty);
            }
            return double.Parse(expression) * multiplier;
        }
    }
}