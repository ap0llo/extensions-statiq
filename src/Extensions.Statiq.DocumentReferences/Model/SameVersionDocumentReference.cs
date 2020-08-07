using System;
using System.Diagnostics.CodeAnalysis;

namespace Grynwald.Extensions.Statiq.DocumentReferences.Model
{
    /// <summary>
    /// Represents a reference to a different document in the same version.
    /// For details on the different kinds of documentation references, see <see cref="DocumentReference"/>.
    /// </summary>
    public sealed class SameVersionDocumentReference : DocumentReference
    {
        /// <summary>
        /// Gets the referenced id.
        /// </summary>
        public DocumentName Name { get; }


        /// <summary>
        /// Initializes a new instance of <see cref="SameVersionDocumentReference"/>.
        /// </summary>
        /// <param name="name">The referenced name.</param>
        public SameVersionDocumentReference(DocumentName name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }


        /// <inheritdoc />
        public override string ToString() => $"{s_Scheme}:{Name}";

        /// <inheritdoc />
        public override int GetHashCode() => Name.GetHashCode();

        /// <inheritdoc />
        public override bool Equals([AllowNull] object obj) =>
            obj is SameVersionDocumentReference sameVersionReference &&
            Name.Equals(sameVersionReference.Name);

        /// <inheritdoc />
        public override bool Equals([AllowNull] DocumentReference other) =>
            other is SameVersionDocumentReference sameVersionReference &&
            Name.Equals(sameVersionReference.Name);

        /// <inheritdoc />
        public override TResult Accept<TResult>(IDocumentReferenceVisitor<TResult> visitor) => visitor.Visit(this);

        /// <inheritdoc />
        public override TResult Accept<TResult, TParameter>(IDocumentReferenceVisitor<TResult, TParameter> visitor, TParameter parameter) => visitor.Visit(this, parameter);
    }
}


