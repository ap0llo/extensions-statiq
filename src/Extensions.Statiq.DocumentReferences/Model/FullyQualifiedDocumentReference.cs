using System;
using System.Diagnostics.CodeAnalysis;

namespace Grynwald.Extensions.Statiq.DocumentReferences.Model
{
    /// <summary>
    /// Represents a fully-qualified document reference.
    /// For details on the different kinds of documentation references, see <see cref="DocumentReference"/>.
    /// </summary>
    public sealed class FullyQualifiedDocumentReference : DocumentReference
    {
        /// <summary>
        /// Gets the identity of the referenced document.
        /// </summary>
        public DocumentIdentity Identity { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="FullyQualifiedDocumentReference"/>.
        /// </summary>
        /// <param name="identity">The referenced identity.</param>
        public FullyQualifiedDocumentReference(DocumentIdentity identity)
        {
            Identity = identity ?? throw new ArgumentNullException(nameof(identity));
        }

        /// <inheritdoc />
        public override string ToString() => $"{s_Scheme}:{Identity}";

        /// <inheritdoc />
        public override int GetHashCode() => Identity.GetHashCode();

        /// <inheritdoc />
        public override bool Equals([AllowNull] object obj) =>
            obj is FullyQualifiedDocumentReference fullyQualifiedReference &&
            Identity.Equals(fullyQualifiedReference.Identity);

        /// <inheritdoc />
        public override bool Equals([AllowNull] DocumentReference other) =>
            other is FullyQualifiedDocumentReference fullyQualifiedReference &&
            Identity.Equals(fullyQualifiedReference.Identity);

        /// <inheritdoc />
        public override TResult Accept<TResult>(IDocumentReferenceVisitor<TResult> visitor) => visitor.Visit(this);

        /// <inheritdoc />
        public override TResult Accept<TResult, TParameter>(IDocumentReferenceVisitor<TResult, TParameter> visitor, TParameter parameter) => visitor.Visit(this, parameter);
    }


}


