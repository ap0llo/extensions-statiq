using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

namespace Grynwald.Extensions.Statiq.TestHelpers
{
    public static class StringExtensions
    {
        public static IHtmlDocument ParseAsHtml(this string content)
        {
            var htmlDocument = new HtmlParser().ParseDocument(content);
            return htmlDocument;
        }
    }
}
