using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;

namespace Grynwald.Extensions.Statiq.DocsTemplate.Test
{
    internal static class StringExtensions
    {
        internal static IHtmlDocument ParseAsHtml(this string content)
        {
            var htmlDocument = new HtmlParser().Parse(content);
            return htmlDocument;
        }
    }
}
