using System;
using System.Collections.Generic;

namespace Grynwald.Extensions.Statiq.Git.Internal
{
    public interface IGitRepository : IDisposable
    {
        /// <summary>
        /// Gets the repository's url. Can be a local path or a remote url.
        /// </summary>
        string Url { get; }

        RepositoryKind Kind { get; }

        string CurrentBranch { get; }

        IEnumerable<string> Branches { get; }

        IEnumerable<GitTag> Tags { get; }

        GitId GetHeadCommitId(string branchName);

        GitDirectoryInfo GetRootDirectory(GitId commit);
    }
}
