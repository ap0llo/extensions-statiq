﻿using System.Threading.Tasks;
using AngleSharp.Dom.Html;
using Statiq.Common;

namespace Grynwald.Extensions.Statiq.DocsTemplate.Test
{
    internal static class DocumentExtensions
    {
        //TODO: Introduce a "common" project for extension method duplicated across projects
        internal static async Task<IHtmlDocument> ParseAsHtmlAsync(this IDocument document)
        {
            var html = await document.GetContentStringAsync();
            return html.ParseAsHtml();
        }
    }
}