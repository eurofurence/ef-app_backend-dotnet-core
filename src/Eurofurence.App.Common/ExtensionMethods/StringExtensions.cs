namespace Eurofurence.App.Common.ExtensionMethods
{
    public static class StringExtensions
    {
        public static string EscapeMarkdown(this string input)
        {
            return input
                .Replace("_", @"\_")
                .Replace("*", @"\*");
        }
    }
}
