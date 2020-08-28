using System;
using System.Collections.Generic;

namespace Grynwald.Extensions.Statiq.Git.Internal
{
    public interface IGitRepository : IDisposable
    {
        RepositoryKind Kind { get; }

        string CurrentBranch { get; }

        IEnumerable<string> Branches { get; }

        IEnumerable<GitTag> Tags { get; }

        GitId GetHeadCommitId(string branchName);

        GitDirectoryInfo GetRootDirectory(GitId commit);
    }
}
