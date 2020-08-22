using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;
using Statiq.Html;

namespace Grynwald.Extensions.Statiq.DocsTemplate.Modules
{
    /// <summary>
    /// Adds <see cref="DocsTemplateKeys.Title"/> metadata to documents that have no title yet.
    /// The title is loaded from the first (and highest-level) HTML heading in the document.
    /// </summary>
    public class InferTitle : Module
    {
        private const string s_HeadingKey = "HeadingContent";


        protected override async Task<IEnumerable<IDocument>> ExecuteInputAsync(IDocument input, IExecutionContext context)
        {
            if (input.ContainsKey(DocsTemplateKeys.Title))
            {
                return input.Yield();
            }

            var title = await TryGetTitleFromHeadingsAsync(input, context);
            if (title != null)
            {
                var newMetadata = new Dictionary<string, object>()
                {
                    { DocsTemplateKeys.Title, title }
                };
                return input.Clone(newMetadata).Yield();
            }
            else
            {
                return input.Yield();
            }
        }


        private async Task<IDocument> GatherHeadingAsync(IDocument input, IExecutionContext context)
        {
            var documents = await context.ExecuteModulesAsync(new ModuleList()
            {
                new GatherHeadings(6).WithHeadingKey(s_HeadingKey)
            },
            input.Yield());

            return documents.Single();
        }

        private async Task<string?> TryGetTitleFromHeadingsAsync(IDocument document, IExecutionContext context)
        {
            var headingsDocument = await GatherHeadingAsync(document, context);

            return headingsDocument
                .Get(HtmlKeys.Headings, Enumerable.Empty<IDocument>())
                ?.OrderBy(x => x.GetInt(HtmlKeys.Level))
                ?.FirstOrDefault()
                ?.GetString(s_HeadingKey);
        }
    }
}
