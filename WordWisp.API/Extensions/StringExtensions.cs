using System.Text.RegularExpressions;

namespace WordWisp.API.Extensions
{
    public static class StringExtensions
    {
        public static string ToSnakeCase(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return Regex.Replace(input, "(?<!^)([A-Z])", "_$1").ToLower();
        }
    }
}
