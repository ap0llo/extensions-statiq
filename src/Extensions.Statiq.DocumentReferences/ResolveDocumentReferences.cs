using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AngleSharp.Dom.Html;
using Grynwald.Extensions.Statiq.DocumentReferences.Model;
using Microsoft.Extensions.Logging;
using Statiq.Common;
using Statiq.Html;

namespace Grynwald.Extensions.Statiq.DocumentReferences
{
    /// <summary>
    /// Resolves document references in HTML documents.
    /// </summary>
    /// <remarks>
    /// This module replaces all document references (in the format <c>ref:name@version</c>) in HTML input documents with relative links to the document being referenced.
    /// This assumes that all referenced documents are part of the module's input.
    /// <para>
    /// By default, the module loads the documents' identities from the <see cref="DocumentReferenceKeys.DocumentIdentity"/> metadata.
    /// This can be customized using <see cref="WithDocumentIdentity(Config{DocumentIdentity})"/>.
    /// </para>
    /// <para>
    /// The links are replaced by relative paths between the documents.
    /// By default, this uses the documents' destinations paths (so this module should be executed after setting the destination).
    /// To generated links between the documents' source path, set <see cref="ResolutionMode"/> to <see cref="LinkResolutionMode.Source"/> using <see cref="WithResolutionMode(LinkResolutionMode)"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="SetDocumentReferenceMetadata"/>
    public sealed class ResolveDocumentReferences : Module
    {
        private Config<DocumentIdentity> m_Identity = Config.FromDocument(d => d.Get<DocumentIdentity>(DocumentReferenceKeys.DocumentIdentity));

        /// <summary>
        /// Gets the currently configured link resolution mode.
        /// </summary>
        /// <seealso cref="WithResolutionMode(LinkResolutionMode)"/>
        public LinkResolutionMode ResolutionMode { get; private set; } = LinkResolutionMode.Destination;


        /// <summary>
        /// Sets the module's link resolution mode.
        /// </summary>
        /// <remarks>
        /// The resolution mode controls whether links are replaced with relative paths between the documents' destination or source paths.
        /// </remarks>
        /// <seealso cref="ResolutionMode"/>
        public ResolveDocumentReferences WithResolutionMode(LinkResolutionMode resolutionMode)
        {
            if (!Enum.IsDefined(typeof(LinkResolutionMode), resolutionMode))
                throw new ArgumentException($"Value is not defined in enum {nameof(LinkResolutionMode)}", nameof(resolutionMode));

            ResolutionMode = resolutionMode;
            return this;
        }

        /// <summary>
        /// Sets the <see cref="Config{DocumentIdentity}"/> being used to retrieve a document's identity.
        /// By default, a document's <see cref="DocumentReferenceKeys.DocumentIdentity"/> metadata is used.
        /// </summary>
        public ResolveDocumentReferences WithDocumentIdentity(Config<DocumentIdentity> identity)
        {
            m_Identity = identity ?? throw new ArgumentNullException(nameof(identity));
            return this;
        }


        protected override async Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context)
        {
            // For all documents, load the identity
            var resolver = new DocumentReferenceResolver<IDocument>();
            foreach (var input in context.Inputs)
            {
                var identity = await m_Identity.GetValueAsync(input, context);

                if (identity is null)
                {
                    context.LogWarning($"Failed to determine document identity for document {input.Id}. Ignoring input document");
                }
                else if (resolver.ContainsIdentity(identity))
                {
                    throw new DuplicateDocumentIdentityException($"Multiple documents have the same identity '{identity}'");
                }
                else
                {
                    resolver.Add(identity, input);
                }
            }

            // Process all links in the input documents
            return await context.ExecuteModulesAsync(new ModuleList()
            {
                new ProcessHtml("a[href]", (document, ctx, element) =>
                {
                    if(element is IHtmlAnchorElement anchorElement)
                        TryResolveLink(ctx, document, resolver, anchorElement);
                })
            },
            context.Inputs);
        }


        private void TryResolveLink(IExecutionContext context, IDocument document, DocumentReferenceResolver<IDocument> resolver, IHtmlAnchorElement element)
        {
            var href = element.GetAttribute("href");

            if (DocumentReference.TryParse(href, out var reference))
            {
                var targetDocument = resolver.TryResolveDocument(reference, document);

                if (targetDocument != null)
                {
                    var relativePath = GetRelativePath(context, document, targetDocument);
                    element.SetAttribute("href", relativePath.ToString());
                }
                else
                {
                    context.LogWarning($"Failed to resolve reference '{reference}'");
                }
            }
        }

        private NormalizedPath GetRelativePath(IExecutionContext context, IDocument from, IDocument to)
        {
            NormalizedPath linkSourcePath;
            NormalizedPath linkTargetPath;

            switch (ResolutionMode)
            {
                case LinkResolutionMode.Destination:
                    linkSourcePath = from.Destination.ToAbsolutePath(context.FileSystem.OutputPath);
                    linkTargetPath = to.Destination.ToAbsolutePath(context.FileSystem.OutputPath);
                    break;

                case LinkResolutionMode.Source:
                    linkSourcePath = from.Source;
                    linkTargetPath = to.Source;
                    break;

                default:
                    throw new NotImplementedException();
            }

            return linkSourcePath.Parent.GetRelativePath(linkTargetPath);
        }
    }
}
