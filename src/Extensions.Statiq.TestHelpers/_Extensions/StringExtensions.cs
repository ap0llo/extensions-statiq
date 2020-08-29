using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;

namespace Grynwald.Extensions.Statiq.TestHelpers
{
    public static class StringExtensions
    {
        public static IHtmlDocument ParseAsHtml(this string content)
        {
            var htmlDocument = new HtmlParser().Parse(content);
            return htmlDocument;
        }
    }
}
