using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grynwald.Extensions.Statiq.DocumentReferences.Model;
using NuGet.Versioning;
using Statiq.Common;

namespace Grynwald.Extensions.Statiq.DocumentReferences
{
    /// <summary>
    /// Adds metadata about other document versions to the input documents.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The module requires every document to have an "identity" (e.g. set by <see cref="DocumentIdentity"/>).
    /// By default, the identity is read from the <see cref="DocumentReferenceKeys.DocumentIdentity"/> metadata,
    /// but this can be customized by setting a custom <see cref="Config{DocumentIdentity}"/> using <see cref="WithDocumentIdentity(Config{DocumentIdentity})"/>
    /// </para>
    /// <para>
    /// For every input document, the following metadata is added
    /// <list type="bullet">
    ///     <item>
    ///         <term><see cref="DocumentReferenceKeys.LatestDocumentVersion"/></term>
    ///         <description>The latest version of the current document as <see cref="NuGetVersion"/>.</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="DocumentReferenceKeys.LatestVersion"/></term>
    ///         <description>
    ///         The latest version of any document as <see cref="NuGetVersion"/>.
    ///         This will be different from <see cref="DocumentReferenceKeys.LatestDocumentVersion"/> when not all documents exist in all versions.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="DocumentReferenceKeys.AllDocumentVersions"/></term>
    ///         <description>
    ///         Document reference metadata about the other versions of the current document as <see cref="IDocument[]"/>.
    ///         Each child-document will contain the following metadata:
    ///         <list type="bullet">
    ///             <item>
    ///                 <term><see cref="DocumentReferenceKeys.DocumentName"/></term>
    ///                 <description>
    ///                     The other document's name as instance <see cref="DocumentName"/>.
    ///                     This value will always be the same as the current document.
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <term><see cref="DocumentReferenceKeys.DocumentVersion"/></term>
    ///                 <description>The other document's version as instance of <see cref="NuGetVersion"/></description>
    ///             </item>
    ///             <item>
    ///                 <term><see cref="DocumentReferenceKeys.DocumentIdentity"/></term>
    ///                 <description>The other document's identity as instance of <see cref="DocumentIdentity"/></description>
    ///             </item>
    ///             <item>
    ///                 <term><see cref="DocumentReferenceKeys.DocumentReference"/></term>
    ///                 <description>A reference to the document as <see cref="DocumentReference"/>.</description>
    ///             </item>
    ///         </list>
    ///         </description>
    ///     </item>
    /// </list>
    /// </para>
    /// </remarks>
    public sealed class GatherVersions : Module
    {
        private Config<DocumentIdentity> m_Identity = Config.FromDocument(d => d.Get<DocumentIdentity>(DocumentReferenceKeys.DocumentIdentity));


        /// <summary>
        /// Sets the <see cref="Config{DocumentIdentity}"/> being used to retrieve a document's identity.
        /// By default, a document's <see cref="DocumentReferenceKeys.DocumentIdentity"/> metadata is used.
        /// </summary>
        public GatherVersions WithDocumentIdentity(Config<DocumentIdentity> identity)
        {
            m_Identity = identity ?? throw new ArgumentNullException(nameof(identity));
            return this;
        }


        protected override async Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context)
        {
            if (!context.Inputs.Any())
                return Enumerable.Empty<IDocument>();

            var allVersions = new HashSet<NuGetVersion>();
            var allIdentities = new HashSet<DocumentIdentity>();
            var identitiesByDocument = new Dictionary<IDocument, DocumentIdentity>();

            foreach (var input in context.Inputs)
            {
                var identity = await m_Identity.GetValueAsync(input, context);

                if (identity == null)
                    throw new MissingDocumentIdentityException($"Failed to determine identity for document '{input.Id}'");

                allVersions.Add(identity.Version);
                allIdentities.Add(identity);
                identitiesByDocument.Add(input, identity);
            }

            var latestVersion = allVersions.OrderByDescending(x => x).First();

            return context.Inputs.Select(input =>
            {
                var inputIdentity = identitiesByDocument[input];

                var allDocumentVersions = allIdentities
                    .Where(identitiy => identitiy.Name == inputIdentity.Name)
                    .OrderBy(identity => identity.Version)
                    .Select(identity =>
                        new Metadata(new Dictionary<string, object>()
                        {
                            { DocumentReferenceKeys.DocumentName, identity.Name },
                            { DocumentReferenceKeys.DocumentVersion, identity.Version },
                            { DocumentReferenceKeys.DocumentIdentity, identity },
                            { DocumentReferenceKeys.DocumentReference, new FullyQualifiedDocumentReference(identity) },
                        })
                    )
                    .ToArray();

                var latestDocumentVersion = allIdentities
                    .Where(identitiy => identitiy.Name == inputIdentity.Name)
                    .Select(x => x.Version)
                    .Distinct()
                    .OrderByDescending(x => x)
                    .First();

                var metadata = new Dictionary<string, object>()
                {
                    {  DocumentReferenceKeys.LatestVersion, latestVersion },
                    {  DocumentReferenceKeys.LatestDocumentVersion, latestDocumentVersion },
                    {  DocumentReferenceKeys.AllDocumentVersions, allDocumentVersions },
                };

                return input.Clone(metadata);
            });
        }
    }

}
