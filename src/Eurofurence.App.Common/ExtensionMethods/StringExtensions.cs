namespace Eurofurence.App.Common.ExtensionMethods
{
    public static class StringExtensions
    {
        public static string EscapeMarkdown(this string input)
        {
            return input
                .Replace("_", @"\_")
                .Replace("*", @"\*")
                .Replace("`", @"``");
        }

        public static string RemoveMarkdown(this string input)
        {
            return input
                .Replace("_", "")
                .Replace("*", "")
                .Replace("`", "");
        }

        public static string UppercaseFirst(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }
            return char.ToUpper(input[0]) + input.Substring(1);
        }
    }
}
