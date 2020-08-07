using System;
using System.Diagnostics.CodeAnalysis;
using NuGet.Versioning;

namespace Grynwald.Extensions.Statiq.DocumentReferences.Model
{
    /// <summary>
    /// Represents a reference to a different version of a document with the same name as the current document.
    /// For details on the different kinds of documentation references, see <see cref="DocumentReference"/>.
    /// </summary>
    public sealed class SameNameDocumentReference : DocumentReference
    {
        /// <summary>
        /// Gets the referenced version.
        /// </summary>
        public NuGetVersion Version { get; }


        /// <summary>
        /// Initializes a new instance of <see cref="SameNameDocumentReference"/>.
        /// </summary>
        /// <param name="version">The referenced version.</param>
        public SameNameDocumentReference(NuGetVersion version)
        {
            Version = version ?? throw new ArgumentNullException(nameof(version));
        }


        /// <inheritdoc />
        public override string ToString() => $"{s_Scheme}:this@{Version}";

        /// <inheritdoc />
        public override int GetHashCode() => Version.GetHashCode();

        /// <inheritdoc />
        public override bool Equals([AllowNull] object obj) =>
            obj is SameNameDocumentReference sameIdReference &&
            Version.Equals(sameIdReference.Version);

        /// <inheritdoc />
        public override bool Equals([AllowNull] DocumentReference other) =>
            other is SameNameDocumentReference sameIdReference &&
            Version.Equals(sameIdReference.Version);

        /// <inheritdoc />
        public override TResult Accept<TResult>(IDocumentReferenceVisitor<TResult> visitor) => visitor.Visit(this);

        /// <inheritdoc />
        public override TResult Accept<TResult, TParameter>(IDocumentReferenceVisitor<TResult, TParameter> visitor, TParameter parameter) => visitor.Visit(this, parameter);
    }
}


