using System.Threading.Tasks;
using AngleSharp.Dom.Html;
using Statiq.Common;

namespace Grynwald.Extensions.Statiq.TestHelpers
{
    public static class DocumentExtensions
    {
        public static async Task<IHtmlDocument> ParseAsHtmlAsync(this IDocument document)
        {
            var html = await document.GetContentStringAsync();
            return html.ParseAsHtml();
        }
    }
}
