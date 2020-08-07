using System;
using System.Diagnostics.CodeAnalysis;
using NuGet.Versioning;

namespace Grynwald.Extensions.Statiq.DocumentReferences.Model
{
    /// <summary>
    /// The identity of a document.
    /// </summary>
    /// <remarks>
    /// Different versions of the same content share an name, while all documents with the same version represent a component's documentation at a specific version.
    /// </remarks>
    /// <seealso cref="DocumentName"/>
    /// <seealso cref="NuGetVersion"/>
    public sealed class DocumentIdentity : IEquatable<DocumentIdentity>
    {
        /// <summary>
        /// Gets the document's name.
        /// </summary>
        public DocumentName Name { get; }

        /// <summary>
        /// Gets the document's version.
        /// </summary>
        public NuGetVersion Version { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="DocumentIdentity"/>.
        /// </summary>
        /// <param name="name">The document's name.</param>
        /// <param name="version">The document's version.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> or <paramref name="version"/> is <c>null</c>.</exception>
        public DocumentIdentity(DocumentName name, NuGetVersion version)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Version = version ?? throw new ArgumentNullException(nameof(version));
        }


        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hash = Name.GetHashCode() * 397;
                hash ^= Version.GetHashCode();
                return hash;
            }
        }

        /// <inheritdoc />
        public override bool Equals([AllowNull] object obj) => Equals(obj as DocumentIdentity);

        /// <inheritdoc />
        public bool Equals([AllowNull] DocumentIdentity other)
        {
            return !(other is null) &&
                Name.Equals(other.Name) &&
                Version.Equals(other.Version);
        }

        /// <summary>
        /// Determines whether two <see cref="DocumentIdentity"/> instances are equal.
        /// Two instances are considered equal if both <see cref="Name"/> and <see cref="Version"/> are equal.
        /// </summary>
        public static bool operator ==(DocumentIdentity? left, DocumentIdentity? right)
        {
            if (ReferenceEquals(left, right))
                return true;

            if (left is null || right is null)
                return false;

            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two <see cref="DocumentIdentity"/> instances are different.
        /// Two instances are considered different if either <see cref="Name"/> or <see cref="Version"/> are different (or both).
        /// </summary>
        public static bool operator !=(DocumentIdentity? left, DocumentIdentity? right)
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
        /// Gets the string-representation of the document identity.
        /// </summary>
        /// <seealso cref="Parse(String)"/>
        /// <seealso cref="TryParse(String, out DocumentIdentity)"/>
        public override string ToString() => $"{Name}@{Version}";

        /// <summary>
        /// Gets a a new <see cref="DocumentIdentity"/> with the specified name and the current instance's version
        /// </summary>
        public DocumentIdentity WithName(DocumentName name) => new DocumentIdentity(name, Version);

        /// <summary>
        /// Gets a a new <see cref="DocumentIdentity"/> with the specified version and the current instance's name.
        /// </summary>
        public DocumentIdentity WithVersion(NuGetVersion version) => new DocumentIdentity(Name, version);

        /// <summary>
        /// Attempts to parse the string-representation of a document identity.
        /// Expects input to be in the format <c>NAME@VERSION</c> where <c>NAME</c> is a valid <see cref="DocumentName"/> and <c>VERSION</c> is a valid <see cref="NuGetVersion"/>.
        /// </summary>
        /// <param name="value">The value to parse as <see cref="DocumentIdentity"/>.</param>
        /// <returns>Returns the parsed <see cref="DocumentIdentity"/>.</returns>
        /// <exception cref="ArgumentException">Thrown if the specified value is not a valid <see cref="DocumentIdentity"/>.</exception>
        /// <seealso cref="ToString"/>
        public static DocumentIdentity Parse(string value) =>
            TryParse(value, out var identity)
                ? identity
                : throw new ArgumentException($"Value '{value}' is not a valid {nameof(DocumentIdentity)}", nameof(value));

        /// <summary>
        /// Attempts to parse the specified value as document identity.
        /// Expects input to be in the format <c>NAME@VERSION</c> where <c>NAME</c> is a valid <see cref="DocumentName"/> and <c>VERSION</c> is a valid <see cref="NuGetVersion"/>.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="identity">When successful, contains a reference to the created <see cref="DocumentIdentity"/> instance.</param>
        /// <returns>
        /// Returns <c>true</c> is <paramref name="value"/> was successfully parsed.
        /// Otherwise returns <c>false</c>.
        /// </returns>
        /// <seealso cref="ToString"/>
        public static bool TryParse(string value, [NotNullWhen(true)] out DocumentIdentity? identity)
        {
            identity = default;

            if (String.IsNullOrWhiteSpace(value))
                return false;

            var fragments = value.Split('@');

            if (fragments.Length != 2)
                return false;

            var idString = fragments[0];
            var versionString = fragments[1];

            if (!NuGetVersion.TryParse(versionString, out var version))
                return false;

            if (!DocumentName.TryCreate(idString, out var id))
                return false;

            identity = new DocumentIdentity(id, version);
            return true;
        }
    }
}
