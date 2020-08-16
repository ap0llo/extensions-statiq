using System.Diagnostics.CodeAnalysis;

namespace Grynwald.Extensions.Statiq.DocumentReferences.Model
{
    /// <summary>
    /// Represents a reference to the current document.
    /// For details on the different kinds of documentation references, see <see cref="DocumentReference"/>.
    /// </summary>
    public sealed class SelfDocumentReference : DocumentReference
    {
        /// <inheritdoc />
        public override bool Equals([AllowNull] object obj) => obj is SelfDocumentReference;

        /// <inheritdoc />
        public override bool Equals([AllowNull] DocumentReference other) => other is SelfDocumentReference;

        /// <inheritdoc />
        public override int GetHashCode() => typeof(SelfDocumentReference).GetHashCode();

        /// <inheritdoc />
        public override string ToString() => $"{s_Scheme}:this";

        /// <inheritdoc />
        public override TResult Accept<TResult>(IDocumentReferenceVisitor<TResult> visitor) => visitor.Visit(this);

        /// <inheritdoc />
        public override TResult Accept<TResult, TParameter>(IDocumentReferenceVisitor<TResult, TParameter> visitor, TParameter parameter) => visitor.Visit(this, parameter);
    }
}


