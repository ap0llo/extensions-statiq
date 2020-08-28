// Note: Adapted from https://github.com/ap0llo/changelog/blob/9c789d570199480801ea95d57f425b425b5f1964/src/ChangeLog/Git/GitTag.cs    

using System;
using System.Diagnostics.CodeAnalysis;

namespace Grynwald.Extensions.Statiq.Git.Internal
{
    public sealed class GitTag : IEquatable<GitTag>
    {
        public string Name { get; }

        public GitId Commit { get; }


        public GitTag(string name, GitId commit)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Value must not be null or whitespace", nameof(name));

            Name = name;
            Commit = commit;
        }


        public override int GetHashCode()
        {
            unchecked
            {
                var hash = StringComparer.Ordinal.GetHashCode(Name) * 397;
                hash ^= Commit.GetHashCode();
                return hash;
            }
        }

        public override bool Equals(object? obj) => Equals(obj as GitTag);

        public bool Equals([AllowNull] GitTag other) =>
            other != null &&
            StringComparer.Ordinal.Equals(Name, other.Name) &&
            Commit.Equals(other.Commit);
    }
}
