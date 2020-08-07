using System;
using System.Diagnostics.CodeAnalysis;
using NuGet.Versioning;

namespace Grynwald.Extensions.Statiq.DocumentReferences.Model
{
    /// <summary>
    /// Represents a reference to a document.
    /// </summary>
    /// <remarks>
    /// Documentation references can be converted to/from string using <see cref="ToString"/> respectively <see cref="Parse"/>.
    /// <para>
    ///     A reference's string representation can have the following formats:
    ///     <list type="bullet">
    ///         <item>
    ///             <term><c>ref:NAME@VERSION</c></term>
    ///             <description>
    ///                 A fully qualified reference to the document with name <c>NAME</c> in version <c>VERSION</c>.
    ///                 <c>NAME</c> must be a valid document name (see <see cref="DocumentName"/>) and
    ///                 <c>VERSION</c> must be a valid version (see <see cref="NuGetVersion"/>).
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term><c>ref:NAME</c></term>
    ///             <description>
    ///                 A reference to another document with an name of <c>NAME</c> in the same version as the current document.
    ///                 <c>NAME</c> must be a valid document name (see <see cref="DocumentName"/>).
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term><c>ref:this@VERSION</c></term>
    ///             <description>
    ///                 A reference to another document with the same name as the current document in version <c>VERSION</c>.
    ///                 <c>VERSION</c> must be a valid version (see <see cref="NuGetVersion"/>).
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term><c>ref:this</c></term>
    ///             <description>
    ///                 A self-reference to the current document (same name as the current document and version as the current document).
    ///             </description>
    ///         </item>
    ///     </list>
    /// </para>
    /// <para>
    ///     The different kinds of references are represented through different derived classes of <see cref="DocumentReference"/>.
    ///     <see cref="Parse(String)"/> will return instances of the appropriate types.
    ///     Use <see cref="IDocumentReferenceVisitor" /> implement logic handling the different types.
    /// </para>
    /// </remarks>
    /// <seealso cref="DocumentName" />
    /// <seealso cref="DocumentIdentity" />
    /// <seealso cref="FullyQualifiedDocumentReference"/>
    /// <seealso cref="SameVersionDocumentReference"/>
    /// <seealso cref="SameNameDocumentReference"/>
    /// <seealso cref="SelfDocumentReference"/>
    public abstract class DocumentReference : IEquatable<DocumentReference>
    {
        protected const string s_Scheme = "ref";


        protected internal DocumentReference()
        { }


        /// <summary>
        /// Gets the string-representation of the document reference.
        /// </summary>
        /// <seealso cref="Parse(String)"/>
        /// <seealso cref="TryParse(String, out DocsIdentity)"/>
        public abstract override string ToString();

        /// <inheritdoc />
        public abstract override int GetHashCode();

        /// <inheritdoc />
        public abstract override bool Equals([AllowNull] object obj);

        /// <inheritdoc />
        public abstract bool Equals([AllowNull] DocumentReference other);


        /// <inheritdoc />
        public static bool operator ==(DocumentReference? left, DocumentReference? right)
        {
            if (ReferenceEquals(left, right))
                return true;

            if (left is null || right is null)
                return false;

            return left.Equals(right);
        }

        /// <inheritdoc />
        public static bool operator !=(DocumentReference? left, DocumentReference? right)
        {
            if (left is null)
            {
                if (right is null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                if (right is null)
                {
                    return true;
                }
                else
                {
                    return !left.Equals(right);
                }
            }
        }

        /// <summary>
        /// Calls the matching <c>Visit</c> method of the specified visitor.
        /// </summary>
        public abstract TResult Accept<TResult>(IDocumentReferenceVisitor<TResult> matcher);

        /// <summary>
        /// Calls the matching <c>Visit</c> method of the specified visitor.
        /// </summary>
        public abstract TResult Accept<TResult, TParameter>(IDocumentReferenceVisitor<TResult, TParameter> matcher, TParameter parameter);

        /// <summary>
        /// Parses the specified value as document reference.
        /// </summary>
        /// <param name="value">The value to parse as <see cref="DocumentReference"/>.</param>
        /// <returns>
        /// Returns the created <see cref="DocumentReference"/> instance.
        /// The exact return type depends on the kind of reference and can be on of the following types:
        /// <list type="bullet">
        ///     <item><description><see cref="FullyQualifiedDocumentReference"/></description></item>
        ///     <item><description><see cref="SameVersionDocumentReference"/></description></item>
        ///     <item><description><see cref="SameNameDocumentReference"/></description></item>
        ///     <item><description><see cref="SelfDocumentReference"/></description></item>
        /// </list>
        /// Use <see cref="IDocumentReferenceVisitor" /> implement logic handling the different types.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="value"/> could not be parsed as documentation reference.
        /// </exception>
        /// <seealso cref="TryParse(String, out DocumentReference)"/>
        /// <seealso cref="ToString"/>
        public static DocumentReference Parse(string value) =>
            TryParse(value, out var reference)
                ? reference
                : throw new ArgumentException($"Value '{value}' is not a valid {nameof(DocumentReference)}", nameof(value));

        /// <summary>
        /// Attempts to parse the string-representation of a documentation reference.
        /// </summary>
        /// <param name="value">The value to parse as <see cref="DocumentReference"/>.</param>
        /// <param name="reference">
        /// When successful, contains a reference to the created <see cref="DocumentReference"/> instance.
        /// The exact return type depends on the kind of reference and can be on of the following types:
        /// <list type="bullet">
        ///     <item><description><see cref="FullyQualifiedDocumentReference"/></description></item>
        ///     <item><description><see cref="SameVersionDocumentReference"/></description></item>
        ///     <item><description><see cref="SameNameDocumentReference"/></description></item>
        ///     <item><description><see cref="SelfDocumentReference"/></description></item>
        /// </list>
        /// Use <see cref="IDocumentReferenceVisitor" /> to implement logic handling the different types.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> is <paramref name="value"/> was successfully parsed.
        /// Otherwise returns <c>false</c>.
        /// </returns>
        /// <seealso cref="Parse(String)"/>
        /// <seealso cref="ToString"/>
        public static bool TryParse(string value, [NotNullWhen(true)] out DocumentReference? reference)
        {
            reference = default;

            // reference must not be null of whitespace
            if (String.IsNullOrWhiteSpace(value))
                return false;

            // references must not contains leading or trailing whitespace
            if (Char.IsWhiteSpace(value[0]) || Char.IsWhiteSpace(value[^1]))
                return false;

            // references must be valid URIs
            if (!Uri.TryCreate(value, UriKind.Absolute, out var referenceUri))
                return false;

            // URI scheme must be "ref:"
            if (referenceUri.Scheme != s_Scheme)
                return false;

            var path = Uri.UnescapeDataString(referenceUri.PathAndQuery);

            // value is parsable as identity (contains both id and version, e.g. 'someid@1.2.3')
            // => fully qualified id
            if (DocumentIdentity.TryParse(path, out var identity))
            {
                reference = new FullyQualifiedDocumentReference(identity);
                return true;
            }

            // value starts with 'this@' followed by a versions => "Same Id reference"
            // (references the same document in a different version)
            if (path.StartsWith("this@", StringComparison.OrdinalIgnoreCase))
            {
                var versionString = path.Substring("this@".Length);
                if (NuGetVersion.TryParse(versionString, out var version))
                {
                    reference = new SameNameDocumentReference(version);
                    return true;
                }
            }

            // if reference is just 'this', return a "self-refernce" (reference to the document itself)
            if (path.Equals("this", StringComparison.OrdinalIgnoreCase))
            {
                reference = new SelfDocumentReference();
                return true;
            }

            // if reference is a valid DocsId, return a "same version reference"
            // (reference to a different document but in the same version as the current document)
            if (DocumentName.TryCreate(path, out var id))
            {
                reference = new SameVersionDocumentReference(id);
                return true;
            }

            // Version not recognized => invalid value
            return false;
        }
    }
}


