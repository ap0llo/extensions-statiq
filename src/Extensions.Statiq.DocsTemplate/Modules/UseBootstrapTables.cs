using System.Collections.Generic;
using System.Threading.Tasks;
using AngleSharp.Dom.Html;
using Statiq.Common;
using Statiq.Html;

namespace Grynwald.Extensions.Statiq.DocsTemplate.Modules
{
    /// <summary>
    /// Adds Bootstrap table classes to all HTML table elements.
    /// </summary>
    /// <seealso href="https://getbootstrap.com/docs/4.5/content/tables/">Tables (Bootstrap Documentation)</seealso>
    public sealed class UseBootstrapTables : Module
    {
        private BootstrapTableOptions m_TableOptions = BootstrapTableOptions.None;

        /// <summary>
        /// Set <see cref="BootstrapTableOptions"/> to use for processing tables
        /// </summary>
        public UseBootstrapTables WithTableOptions(BootstrapTableOptions tableOptions)
        {
            m_TableOptions = tableOptions;
            return this;
        }


        protected override async Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context)
        {
            return await context.ExecuteModulesAsync(new ModuleList()
            {
                new ProcessHtml("table", element =>
                {
                    if(element is IHtmlTableElement tableElement)
                    {
                        tableElement.ClassList.Add("table");

                        if(m_TableOptions.HasFlag(BootstrapTableOptions.Striped))
                            tableElement.ClassList.Add("table-striped");

                        if(m_TableOptions.HasFlag(BootstrapTableOptions.Bordered))
                            tableElement.ClassList.Add("table-bordered");

                        if(m_TableOptions.HasFlag(BootstrapTableOptions.Small))
                            tableElement.ClassList.Add("table-sm");
                    }
                })
            },
            context.Inputs);
        }
    }
}
