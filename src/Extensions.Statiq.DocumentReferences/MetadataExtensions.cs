using Grynwald.Extensions.Statiq.DocumentReferences.Model;
using NuGet.Versioning;
using Statiq.Common;

namespace Grynwald.Extensions.Statiq.DocumentReferences
{
    public static class MetadataExtensions
    {
        /// <summary>
        /// Gets the value for the <see cref="DocumentReferenceKeys.DocumentName"/> key.
        /// </summary>
        public static DocumentName GetDocumentName(this IMetadata metadata) => metadata.Get<DocumentName>(DocumentReferenceKeys.DocumentName);

        /// <summary>
        /// Gets the value for the <see cref="DocumentReferenceKeys.DocumentVersion"/> key.
        /// </summary>
        public static NuGetVersion GetDocumentVersion(this IMetadata metadata) => metadata.Get<NuGetVersion>(DocumentReferenceKeys.DocumentVersion);

        /// <summary>
        /// Gets the value for the <see cref="DocumentReferenceKeys.DocumentIdentity"/> key.
        /// </summary>
        public static DocumentIdentity GetDocumentIdentity(this IMetadata metadata) => metadata.Get<DocumentIdentity>(DocumentReferenceKeys.DocumentIdentity);
    }
}
