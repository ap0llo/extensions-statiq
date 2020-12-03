using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using Statiq.Common;
using Statiq.Html;

namespace Grynwald.Extensions.Statiq.DocsTemplate.Modules
{
    /// <summary>
    /// Replaces HTML links between input files with links between the corresponding output files.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Note: Because this module accesses the input document's destination, this module must be run *after* setting the <see cref="IDocument.Destination"/> of all documents.
    /// </para>
    /// <para>
    /// The module processes all <c>href</c> attributes in the input documents and attempts to find the target document based in the document's <see cref="IDocument.Source"/> path.
    /// When a link could be resolved, it is replaced with a relative path between the documents' <see cref="IDocument.Destination"/>.
    /// </para>
    /// </remarks>
    public class ConvertSourceLinksToDestinationLinks : Module
    {
        protected override async Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context)
        {
            return await context.ExecuteModulesAsync(
                 new ModuleList()
                 {
                    new ProcessHtml("[href]", (document , executionContext, element) =>
                    {
                        if(element is IHtmlElement htmlElement)
                            ResolveLink(document, executionContext, htmlElement);
                    })
                 },
                 context.Inputs);
        }

        private static void ResolveLink(IDocument document, IExecutionContext context, IHtmlElement anchorElement)
        {
            var href = anchorElement.GetAttribute("href");

            // skip absolute URIs
            if (Uri.TryCreate(href, UriKind.Absolute, out _))
            {
                return;
            }

            // remove anchor targets from the href
            var (relativeLinkTarget, anchor) = ParseLink(href);

            if (String.IsNullOrEmpty(relativeLinkTarget))
                return;

            var targetDocument = context.Inputs.FirstOrDefault(
                x => x.Source.Equals(document.Source.Parent.Combine(relativeLinkTarget))
            );

            if (targetDocument != null)
            {
                var absoluteDestination = context.FileSystem.OutputPath.Combine(document.Destination);
                var linkTargetAbsoluteDestination = context.FileSystem.OutputPath.Combine(targetDocument.Destination);

                var relativePath = absoluteDestination.Parent.GetRelativePath(linkTargetAbsoluteDestination);

                if (!String.IsNullOrEmpty(anchor))
                {
                    anchor = $"#{anchor}";
                }

                anchorElement.SetAttribute("href", $"{relativePath}{anchor}");
            }
        }


        private static (string relativePath, string anchor) ParseLink(string link)
        {
            var index = link.IndexOf('#');
            if (index >= 0)
            {
                var anchor = link.Substring(index).TrimStart('#');
                var relativePath = link.Substring(0, index);

                return (relativePath, anchor);
            }
            else
            {
                return (link, "");
            }
        }
    }
}
