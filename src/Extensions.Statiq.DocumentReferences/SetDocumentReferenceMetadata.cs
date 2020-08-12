using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grynwald.Extensions.Statiq.DocumentReferences.Model;
using NuGet.Versioning;
using Statiq.Common;

namespace Grynwald.Extensions.Statiq.DocumentReferences
{
    /// <summary>
    /// Module that adds <see cref="DocumentIdentity"/> metadata to all input documents.
    /// </summary>
    /// <remarks>
    /// The module sets the following metadata for each input document:
    /// <list type="bullet">
    ///     <item>
    ///         <term><see cref="DocumentReferenceKeys.DocumentName"/></term>
    ///         <description>The document's name as instance <see cref="DocumentName"/></description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="DocumentReferenceKeys.DocumentVersion"/></term>
    ///         <description>The document's version as instance of <see cref="NuGetVersion"/></description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="DocumentReferenceKeys.DocumentIdentity"/></term>
    ///         <description>The document's identity as instance of <see cref="DocumentIdentity"/></description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="DocumentReferenceKeys.DocumentReference"/></term>
    ///         <description>The reference to the document as instance of <see cref="DocumentReference"/></description>
    ///     </item>
    /// </list>
    /// </remarks>
    public sealed class SetDocumentReferenceMetadata : Module
    {
        private readonly Config<DocumentName>? m_DocumentName;
        private readonly Config<NuGetVersion>? m_DocumentVersion;

        /// <summary>
        /// Adds document reference metadata to all documents
        /// </summary>
        /// <param name="documentName">The config to determine a document's name. Value must be parsable as <see cref="DocumentName"/>.</param>
        /// <param name="documentVersion">The config to determine a document's version. Value must be parsable as <see cref="NuGetVersion"/>.</param>
        public SetDocumentReferenceMetadata(Config<string> documentName, Config<string> documentVersion)
        {
            m_DocumentName = Config.FromDocument(async (document, context) =>
            {
                var nameString = await documentName.GetValueAsync(document, context);
                return new DocumentName(nameString);
            });

            m_DocumentVersion = Config.FromDocument(async (document, context) =>
            {
                var versionString = await documentVersion.GetValueAsync(document, context);
                return NuGetVersion.Parse(versionString);
            });
        }

        /// <summary>
        /// Adds document reference metadata to all documents
        /// </summary>
        /// <param name="documentName">The config to determine a document's name.</param>
        /// <param name="documentVersion">The config to determine a document's version.</param>
        public SetDocumentReferenceMetadata(Config<DocumentName> documentName, Config<NuGetVersion> documentVersion)
        {
            m_DocumentName = documentName ?? throw new ArgumentNullException(nameof(documentName));
            m_DocumentVersion = documentVersion ?? throw new ArgumentNullException(nameof(documentVersion));
        }


        /// <inheritdoc />
        protected override async Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context)
        {
            var outputs = new List<IDocument>();
            var identities = new HashSet<DocumentIdentity>();

            foreach (var input in context.Inputs)
            {
                var name = await m_DocumentName.GetValueAsync(input, context);
                var version = await m_DocumentVersion.GetValueAsync(input, context);

                var identity = new DocumentIdentity(name, version);

                if (identities.Contains(identity))
                {
                    throw new DuplicateDocumentIdentityException($"Multiple documents have the same identity '{identity}'");
                }
                else
                {
                    identities.Add(identity);
                }

                var metadata = new Dictionary<string, object>()
                {
                    { DocumentReferenceKeys.DocumentName, name },
                    { DocumentReferenceKeys.DocumentVersion, version },
                    { DocumentReferenceKeys.DocumentIdentity, identity },
                    { DocumentReferenceKeys.DocumentReference, new FullyQualifiedDocumentReference(identity) }
                };

                var output = input.Clone(metadata);
                outputs.Add(output);
            }

            return outputs;
        }
    }
}
