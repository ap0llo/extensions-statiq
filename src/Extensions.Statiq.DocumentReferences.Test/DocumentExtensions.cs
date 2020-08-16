using System.Threading.Tasks;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using Statiq.Common;

namespace Grynwald.Extensions.Statiq.DocumentReferences.Test
{
    public static class DocumentExtensions
    {
        public static async Task<IHtmlDocument> ParseAsHtmlAsync(this IDocument document)
        {
            var html = await document.GetContentStringAsync();
            var htmlDocument = new HtmlParser().Parse(html);
            return htmlDocument;
        }
    }
}
