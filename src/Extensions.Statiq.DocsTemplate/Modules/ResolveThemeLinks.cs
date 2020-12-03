using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using Statiq.Common;
using Statiq.Html;

namespace Grynwald.Extensions.Statiq.DocsTemplate.Modules
{
    /// <summary>
    /// Replaces links with a <c>theme:</c> uri scheme with a relative file path.
    /// Because this module accesses the documents' destination, this module must be called *after* the destination path has been set.
    /// </summary>
    public sealed class ResolveThemeLinks : Module
    {
        public const string Scheme = "theme";


        protected override async Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context)
        {
            return await context.ExecuteModulesAsync(
                new ModuleList()
                {
                    new ProcessHtml("script[src], [href]", (document, executionContext, element) =>
                    {
                        switch(element)
                        {
                            case IHtmlScriptElement scriptElement:
                                ResolveLink(document, context, scriptElement, "src");
                                break;

                            case IHtmlElement htmlElement:
                                ResolveLink(document, context, htmlElement, "href");
                                break;
                        }
                    })
                },
                context.Inputs);
        }


        private void ResolveLink(IDocument document, IExecutionContext context, IHtmlElement element, string linkAttribute)
        {
            var link = element.GetAttribute(linkAttribute);

            // only process "theme:" links
            if (Uri.TryCreate(link, UriKind.Absolute, out var uri) && uri.Scheme == Scheme)
            {
                NormalizedPath linkTagetDestinationPath = null;

                // Resolve links to assets (js, css..).
                if (uri.PathAndQuery.StartsWith("assets/"))
                {
                    linkTagetDestinationPath = uri.PathAndQuery;
                }

                if (linkTagetDestinationPath != null)
                {
                    var relativePath = document.Destination.ToAbsolutePath(context.FileSystem.OutputPath)
                        .Parent
                        .GetRelativePath(linkTagetDestinationPath.ToAbsolutePath(context.FileSystem.OutputPath));

                    element.SetAttribute(linkAttribute, $"./{relativePath}");
                }
            }
        }
    }
}
