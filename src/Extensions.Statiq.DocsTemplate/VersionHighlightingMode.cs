using Grynwald.Extensions.Statiq.DocumentReferences;

namespace Grynwald.Extensions.Statiq.DocsTemplate
{
    public enum VersionHighlightingMode
    {
        /// <summary>
        /// Do not highlight any version
        /// </summary>
        None,

        /// <summary>
        /// Highlight the latest version (of all document versions).
        /// </summary>
        /// <seealso cref="DocumentReferenceKeys.LatestVersion"/>
        LatestVersion,

        /// <summary>
        /// Highlight the latest version of the current document.
        /// </summary>
        /// <seealso cref="DocumentReferenceKeys.LatestDocumentVersion"/>
        LatestDocumentVersion
    }
}
