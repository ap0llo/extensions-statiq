using System;
using System.Text.RegularExpressions;

namespace Grynwald.Extensions.Statiq.Git.Internal
{
    // Copied from https://github.com/ap0llo/changelog/blob/9c789d570199480801ea95d57f425b425b5f1964/src/ChangeLog/Git/GitId.cs
    // TODO: Add tests
    public struct GitId : IEquatable<GitId>
    {
        private static readonly Regex s_ObjectIdRegex = new Regex(@"^[\dA-z]+$", RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public string Id { get; }


        public GitId(string id)
        {
            if (String.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Value must not be null or whitespace", nameof(id));

            if (!s_ObjectIdRegex.IsMatch(id))
                throw new ArgumentException($"'{id}' is not a valid git object id", nameof(id));

            Id = id;
        }


        public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Id);

        public override bool Equals(object? obj) => obj is GitId otherId && Equals(otherId);

        public bool Equals(GitId other) => StringComparer.OrdinalIgnoreCase.Equals(Id, other.Id);

        public override string ToString() => Id;


        public static bool operator ==(GitId left, GitId right) => left.Equals(right);

        public static bool operator !=(GitId left, GitId right) => !left.Equals(right);
    }
}
