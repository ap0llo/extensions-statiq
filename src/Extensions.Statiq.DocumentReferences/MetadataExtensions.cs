using System.Collections.Generic;
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

        /// <summary>
        /// Gets the value for the <see cref="DocumentReferenceKeys.LatestDocumentVersion"/> key.
        /// </summary>
        public static NuGetVersion GetLatestDocumentVersion(this IMetadata metadata) => metadata.Get<NuGetVersion>(DocumentReferenceKeys.LatestDocumentVersion);

        /// <summary>
        /// Gets the value for the <see cref="DocumentReferenceKeys.LatestVersion"/> key.
        /// </summary>
        public static NuGetVersion GetLatestVersion(this IMetadata metadata) => metadata.Get<NuGetVersion>(DocumentReferenceKeys.LatestVersion);

        /// <summary>
        /// Gets the value for the <see cref="DocumentReferenceKeys.AllDocumentVersions"/> key.
        /// </summary>
        public static IReadOnlyList<IMetadata> GetAllDocumentVersions(this IMetadata metadata) => metadata.Get<IMetadata[]>(DocumentReferenceKeys.AllDocumentVersions);

        /// <summary>
        /// Gets the value for the <see cref="DocumentReferenceKeys.DocumentReference"/> key.
        /// </summary>
        public static DocumentReference GetDocumentReference(this IMetadata metadata) => metadata.Get<DocumentReference>(DocumentReferenceKeys.DocumentReference);
    }
}
