using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Statiq.Common;

namespace Grynwald.Extensions.Statiq.DocsTemplate
{
    public static class ViewDataDictionaryExtensions
    {
        public static IDocument GetStatiqDocument(this ViewDataDictionary viewData) => (IDocument)viewData["StatiqDocument"];
    }
}
