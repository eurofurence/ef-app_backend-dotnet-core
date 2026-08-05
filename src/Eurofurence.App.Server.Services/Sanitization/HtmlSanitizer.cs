using Ganss.Xss;

namespace Eurofurence.App.Server.Services.Sanitization
{
    public class GanssHtmlSanitizer : Abstractions.Sanitization.IHtmlSanitizer
    {
        private static readonly HtmlSanitizer _htmlSanitizer = new();

        public GanssHtmlSanitizer()
        {
            _htmlSanitizer.AllowedTags.Clear();
            _htmlSanitizer.KeepChildNodes = true;
        }
        public string Sanitize(string html)
        {
            return _htmlSanitizer.Sanitize(html);
        }
    }
}