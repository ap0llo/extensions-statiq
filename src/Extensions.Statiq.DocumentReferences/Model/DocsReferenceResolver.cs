using System;
using System.Collections.Generic;
using Grynwald.Utilities.Collections;

namespace Grynwald.Extensions.Statiq.DocumentReferences.Model
{
    internal class DocumentReferenceResolver<TDocument> : IDocumentReferenceVisitor<TDocument?, DocumentIdentity?> where TDocument : class
    {
        private readonly ReversibleDictionary<DocumentIdentity, TDocument> m_Documents = new ReversibleDictionary<DocumentIdentity, TDocument>();


        public TDocument? TryResolveDocument(DocumentReference reference, TDocument currentDocument)
        {
            if (reference is null)
                throw new ArgumentNullException(nameof(reference));

            if (currentDocument is null)
                throw new ArgumentNullException(nameof(currentDocument));

            var currentIdentity = TryGetIdentity(currentDocument);
            return reference.Accept(this, currentIdentity);
        }


        public bool ContainsIdentity(DocumentIdentity identity) => m_Documents.ContainsKey(identity);

        public void Add(DocumentIdentity identity, TDocument document) => m_Documents.Add(identity, document);

        public TDocument? Visit(FullyQualifiedDocumentReference reference, DocumentIdentity? currentDocumentIdentity)
        {
            return m_Documents.GetValueOrDefault(reference.Identity);
        }

        public TDocument? Visit(SameNameDocumentReference reference, DocumentIdentity? currentDocumentIdentity)
        {
            if (currentDocumentIdentity is null)
                return null;

            var targetIdentity = currentDocumentIdentity.WithVersion(reference.Version);
            return m_Documents.GetValueOrDefault(targetIdentity);
        }

        public TDocument? Visit(SelfDocumentReference reference, DocumentIdentity? currentDocumentIdentity)
        {
            if (currentDocumentIdentity is null)
                return null;

            return m_Documents.GetValueOrDefault(currentDocumentIdentity);
        }

        public TDocument? Visit(SameVersionDocumentReference reference, DocumentIdentity? currentDocumentIdentity)
        {
            if (currentDocumentIdentity is null)
                return null;

            var targetIdentity = currentDocumentIdentity.WithName(reference.Name);
            return m_Documents.GetValueOrDefault(targetIdentity);
        }


        private DocumentIdentity? TryGetIdentity(TDocument document) => m_Documents.ReversedDictionary.GetValueOrDefault(document);
    }
}
