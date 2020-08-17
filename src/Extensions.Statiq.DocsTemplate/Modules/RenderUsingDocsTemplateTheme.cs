using System.Collections.Generic;
using System.Threading.Tasks;
using Statiq.Common;
using Statiq.Razor;

namespace Grynwald.Extensions.Statiq.DocsTemplate.Modules
{
    /// <summary>
    /// Renders the input documents using the configured theme (see <see cref="DocsTemplateKeys.DocsTemplateThemeName"/>).
    /// </summary>
    public class RenderUsingDocsTemplateTheme : Module
    {
        /// <summary>
        /// The default Razor layout file name being used for rendering.
        /// </summary>
        public const string DefaultLayoutName = "page";


        protected override async Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context)
        {
            var themeName = context.GetString(DocsTemplateKeys.DocsTemplateThemeName, DocsTemplateThemeNames.Default);

            return await context.ExecuteModulesAsync(
                new ModuleList()
                {
                    new RenderRazor().WithLayout($"~/theme/{themeName}/{DefaultLayoutName}.cshtml")
                },
                context.Inputs);
        }
    }
}
