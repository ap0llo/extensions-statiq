using System;
using System.Collections.Generic;

namespace Grynwald.Extensions.Statiq.Git.Internal
{
    public interface IGitRepository : IDisposable
    {
        RepositoryKind Kind { get; }

        IEnumerable<string> Branches { get; }

        IReadOnlyList<GitFile> GetFiles(string branch);
    }
}
