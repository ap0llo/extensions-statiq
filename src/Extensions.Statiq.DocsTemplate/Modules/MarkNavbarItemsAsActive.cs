using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using Statiq.Common;
using Statiq.Html;

namespace Grynwald.Extensions.Statiq.DocsTemplate.Modules
{
    /// <summary>
    /// Adds the <c>active</c> class to all navigation bar items with a link to the current document.
    /// </summary>
    /// <remarks>
    /// The module inspects all links in the navigation bar.
    /// If a link's <c>href</c> refers to the current document, the <c>href</c> value is set to <c>#</c> and <c>active</c> is added to the links class list.
    /// If the link is in a dropdown menu, both the dropdown item as well as the dropdown toggle is marked as <see cref="active"/>.
    /// </remarks>
    /// <seealso href="https://getbootstrap.com/docs/4.5/components/navbar/">Navbar (Bootstrap Documentation)</seealso>
    public sealed class MarkNavbarItemsAsActive : Module
    {
        /// <summary>
        /// Gets the currently configured link mode.
        /// </summary>
        /// <seealso cref="WithLinkMode(LinkResolutionMode)"/>
        public LinkMode LinkMode { get; private set; } = LinkMode.Destination;


        /// <summary>
        /// Sets the module's link mode
        /// </summary>
        /// <seealso cref="LinkMode"/>
        public MarkNavbarItemsAsActive WithLinkMode(LinkMode linkMode)
        {
            LinkMode = linkMode;
            return this;
        }


        protected override async Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context)
        {
            return await context.ExecuteModulesAsync(new ModuleList()
            {
                new ProcessHtml(".navbar .navbar-nav .nav-item a.nav-link[href]", (document, ctx, element) =>
                {
                    if (element is IHtmlAnchorElement anchorElement)
                    {
                        var href = anchorElement.GetAttribute("href");
                        if (IsActive(document, href))
                        {
                            anchorElement.ClassList.Add("active");
                            anchorElement.SetAttribute("href", "#");
                        }
                    }
                }),
                new ProcessHtml(".navbar .navbar-nav li.nav-item.dropdown", (document, ctx, element) =>
                {
                    if(element is IHtmlListItemElement listElement)
                    {
                        var dropdownToggle = element.QuerySelector("a.nav-link.dropdown-toggle");

                        var dropdownItems = element.QuerySelectorAll("a.dropdown-item[href]").Cast<IHtmlAnchorElement>();
                        foreach (var item in dropdownItems)
                        {
                            var href = item.GetAttribute("href");

                            // if any of the dropdown items links to the current document
                            // mark both the dropdown item and the dropdown toggle as active.
                            if (IsActive(document, href))
                            {
                                item.ClassList.Add("active");
                                item.SetAttribute("href", "#");
                                dropdownToggle.ClassList.Add("active");
                            }
                        }
                    }
                })
            },
            context.Inputs);
        }


        private bool IsActive(IDocument document, string href)
        {
            if (LinkMode == LinkMode.Destination)
            {
                return document.Destination.Parent.Combine(href) == document.Destination;
            }
            else if (LinkMode == LinkMode.Source)
            {
                return document.Source.Parent.Combine(href) == document.Source;

            }
            return false;
        }
    }
}
