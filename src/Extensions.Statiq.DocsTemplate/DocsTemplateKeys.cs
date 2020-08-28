using System.Collections.Generic;

namespace Grynwald.Extensions.Statiq.DocsTemplate
{
    public static class DocsTemplateKeys
    {
        /// <summary>
        /// The name of the theme to use for rendering documents as HTML.
        /// </summary>
        /// <seealso cref="DocsTemplateThemeNames"/>
        public const string DocsTemplateThemeName = nameof(DocsTemplateThemeName);

        /// <summary>
        /// When used as in a document's metadata, specifies the document's title. The value will be used as HTMl <c>title</c>.
        /// When used in a navigation bar item (see <see cref="NavbarItems"/>). specifies the display name of the navigation bar item.
        /// </summary>
        public const string Title = nameof(Title);

        /// <summary>
        /// The name of the site being generated.
        /// Value will be shown in the navigation bar and added to the output documents' HTML <c>title</c>.
        /// </summary>
        public const string SiteName = nameof(SiteName);

        /// <summary>
        /// The items to show in the navigation bar. Value must be <see cref="IEnumerable{Statiq.Common.IMetadata}"/>.
        /// </summary>
        public const string NavbarItems = nameof(NavbarItems);

        /// <summary>
        /// A boolean specifying whether to show items in the navigation bar.        
        /// Default: <c>true</c>
        /// </summary>
        /// <remarks>
        /// Set this metadata item to <c>false</c> to disable the navigation bar for a document, even if it has <see cref="NavbarItems"/> metadata.
        /// </remarks>
        public const string ShowNavbarItems = nameof(ShowNavbarItems);

        /// <summary>
        /// The link target of a navigation bar item.
        /// </summary>
        public const string Link = nameof(Link);

        /// <summary>
        /// A boolean specifying whether to include the version menu in the navigation bar.        
        /// Default: <c>true</c>
        /// </summary>
        public const string ShowVersionMenu = nameof(ShowVersionMenu);

        /// <summary>
        /// Specifies the highlighting of versions in the version menu (as <see cref="DocsTemplate.VersionHighlightingMode"/>).
        /// Default: <see cref="DocsTemplate.VersionHighlightingMode"/>
        /// </summary>
        public const string VersionHighlightingMode = nameof(VersionHighlightingMode);

        /// <summary>
        /// A boolean specifying whether to include the table-of-contents in the output.
        /// Default: <c>true</c>
        /// </summary>
        /// <remarks>
        /// Requires metadata for table-of-contents to be set (see <see cref="ToC"/>)
        /// </remarks>
        public const string ShowToc = nameof(ShowToc);

        /// <summary>
        /// Data for the table-of-contents.
        /// (as <see cref="global::Statiq.Common.IDocument[]"/>, each containing metadata for each table-of-contents entry)
        /// </summary>
        /// <remarks>
        /// Use the <see cref="Modules.LoadToc"/> module to populate this metadata item.
        /// </remarks>
        public const string ToC = nameof(ToC);

        /// <summary>
        /// The id of the heading a table-of-contents entry references.
        /// </summary>
        public const string TocHeadingId = nameof(TocHeadingId);

        /// <summary>
        /// The title of a table-of-contents entry.
        /// </summary>
        public const string TocTitle = nameof(TocTitle);

        /// <summary>
        /// A table-of-contents entry's child items.
        /// </summary>
        public const string TocItems = nameof(TocItems);
    }
}
