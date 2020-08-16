using System;
using System.Diagnostics.CodeAnalysis;

namespace Grynwald.Extensions.Statiq.DocumentReferences.Model
{
    /// <summary>
    /// Represents a document's name.
    /// </summary>
    /// <remarks>
    /// A document name is a string with the following restrictions:
    /// <list type="bullet">    
    ///     <item><description>The name must not be empty or whitespace</description></item>
    ///     <item><description>The name must not contain <c>@</c> or <c>#</c></description></item>
    ///     <item><description>The name must not be <c>this</c> (reserved value)</description></item>
    ///     <item><description>The name must not contain leading or trailing whitespace characters</description></item>
    /// </list>
    /// All comparisons of document names are case-insensitive - two <see cref="DocumentName"/> instances are considered equal if their string value is equal regardless of casing.
    /// </remarks>
    public sealed class DocumentName : IEquatable<DocumentName>
    {
        /// <summary>
        /// Gets the <see cref="String"/> value of the document name.
        /// </summary>
        public string Value { get; }


        /// <summary>
        /// Initializes a new instance of <see cref="DocumentName"/>
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if the specified name is not a valid.</exception>
        public DocumentName(string value)
        {
            if (String.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Value must not be null or whitespace.", nameof(value));

            if (StringComparer.OrdinalIgnoreCase.Equals(value, "this"))
                throw new ArgumentException("A docs id must not be 'this'.", nameof(value));

            if (value.Contains("@"))
                throw new ArgumentException("A docs id  value must not contain '@'.", nameof(value));

            if (value.Contains("#"))
                throw new ArgumentException("A docs id  value must not contain '#'.", nameof(value));

            if (Char.IsWhiteSpace(value[0]) || Char.IsWhiteSpace(value[^1]))
                throw new ArgumentException("A docs id must not contains leading or trailing or whitespace.", nameof(value));

            Value = value;
        }


        /// <inheritdoc />
        public override string ToString() => Value;

        /// <summary>
        /// Converts the <see cref="DocumentName"/> to its string-representation.
        /// </summary>
        public static implicit operator string(DocumentName uid) => uid.Value;

        /// <inheritdoc />
        public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Value);

        /// <inheritdoc />
        public override bool Equals([AllowNull] object obj) => obj is DocumentName other && Equals(other);

        /// <inheritdoc />
        public bool Equals([AllowNull] DocumentName other) => ReferenceEquals(this, other) || StringComparer.OrdinalIgnoreCase.Equals(Value, other?.Value);


        /// <summary>
        /// Determines whether two <see cref="DocumentName"/> instances have the same value.
        /// The comparison is case-insensitive.
        /// </summary>
        public static bool operator ==(DocumentName? left, DocumentName? right)
        {
            if (ReferenceEquals(left, right))
                return true;

            if (left is null || right is null)
                return false;

            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two <see cref="DocumentName"/> instances have different values.
        /// The comparison is case-insensitive.
        /// </summary>
        public static bool operator !=(DocumentName? left, DocumentName? right)
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
        /// Attempts to create a <see cref="DocumentName"/> from the specified <see cref="String"/> value.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="id">When successful, contains a reference to the created <see cref="DocumentName"/> instance.</param>
        /// <returns>
        /// Returns <c>true</c> is <paramref name="value"/> was successfully converted to a <see cref="DocumentName"/>.
        /// Otherwise returns <c>false</c>.
        /// </returns>
        public static bool TryCreate(string value, [NotNullWhen(true)] out DocumentName? id)
        {
            id = default;

            if (String.IsNullOrWhiteSpace(value))
                return false;

            if (StringComparer.OrdinalIgnoreCase.Equals(value, "this"))
                return false;

            if (value.Contains("@"))
                return false;

            if (value.Contains("#"))
                return false;

            if (Char.IsWhiteSpace(value[0]) || Char.IsWhiteSpace(value[^1]))
                return false;

            id = new DocumentName(value);
            return true;
        }
    }
}
