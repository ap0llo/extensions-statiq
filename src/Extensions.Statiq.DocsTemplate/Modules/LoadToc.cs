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
    /// Adds <see cref="DocsTemplateKeys.ToC"/> metadata to the input documents so a table-of-contents can be rendered by the template.
    /// </summary>
    public sealed class LoadToc : Module
    {
        private class TocEntry
        {
            public string Title { get; set; } = "";

            public string? HeadingId { get; set; }

            public List<TocEntry> Items { get; set; } = new List<TocEntry>();


            public IDocument ToDocument(IExecutionContext context)
            {
                return context.CreateDocument(new Dictionary<string, object?>()
                {
                    { DocsTemplateKeys.TocTitle, Title },
                    { DocsTemplateKeys.TocHeadingId, HeadingId },
                    { DocsTemplateKeys.TocItems, Items.Select(x => x.ToDocument(context)).ToArray() },
                });
            }
        }


        protected override async Task<IEnumerable<IDocument>> ExecuteInputAsync(IDocument input, IExecutionContext context)
        {
            var html = await HtmlHelper.ParseHtmlAsync(input);

            if (html is null)
            {
                return input.Yield();
            }

            // Assumes there is at least on he heading in the document.
            // All h2 headings become entries in the table-of-contents, all h3 headings between two h2 elements
            // are output as children of the preceding h2

            // if the document starts with a h3 heading, return unchanged input document
            var elements = html.QuerySelectorAll("h2, h3");
            if ((elements.FirstOrDefault() as IHtmlHeadingElement)?.NodeName?.Equals("h2", StringComparison.OrdinalIgnoreCase) != true)
            {
                return input.Yield();
            }

            var toc = new List<TocEntry>();
            var currentEntry = default(TocEntry);

            foreach (var element in elements)
            {
                if (element is IHtmlHeadingElement heading)
                {
                    var level = Int32.Parse(element.NodeName.Substring(1));

                    if (level == 2)
                    {
                        currentEntry = GetTocEntry(heading);
                        toc.Add(currentEntry);
                    }
                    else if (level == 3)
                    {
                        if (currentEntry == null)
                            throw new InvalidOperationException();

                        var childEntry = GetTocEntry(heading);
                        currentEntry.Items.Add(childEntry);
                    }
                    else
                    {
                        throw new InvalidOperationException();
                    }
                }
            }

            var newMetadata = new Dictionary<string, object>()
            {
                { DocsTemplateKeys.ToC, toc.Select(x => x.ToDocument(context)).ToArray() }
            };

            return input.Clone(newMetadata).Yield();
        }


        private TocEntry GetTocEntry(IHtmlHeadingElement heading)
        {
            return new TocEntry()
            {
                Title = heading.TextContent,
                HeadingId = heading.Id
            };
        }
    }
}
