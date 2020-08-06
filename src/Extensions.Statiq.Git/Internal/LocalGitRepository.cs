using System;
using System.Collections.Generic;
using System.Linq;
using LibGit2Sharp;

namespace Grynwald.Extensions.Statiq.Git.Internal
{
    public class LocalGitRepository : IGitRepository
    {
        private readonly string m_RepositoryPath;
        private readonly Lazy<Repository> m_Repository;


        private Repository Repository => m_Repository.Value;


        public RepositoryKind Kind => RepositoryKind.Local;

        public string CurrentBranch => Repository.Info.IsHeadDetached
            ? throw new InvalidOperationException("Cannot get current branch name because repository is in 'detached HEAD' state")
            : Repository.Head.FriendlyName;

        public IEnumerable<string> Branches
        {
            get
            {
                var branches = Repository.Branches
                    .Select(x => x.CanonicalName)
                    .Where(x => x.StartsWith("refs/heads/"))
                    .Select(x => x.Remove(0, "refs/heads/".Length))
                    .ToArray();

                return branches;
            }
        }


        public LocalGitRepository(string repositoryPath)
        {
            if (String.IsNullOrWhiteSpace(repositoryPath))
                throw new ArgumentException("Value must not be null or whitespace", nameof(repositoryPath));

            m_RepositoryPath = repositoryPath;
            m_Repository = new Lazy<Repository>(() => new Repository(m_RepositoryPath));
        }


        public GitId GetHeadCommitId(string branchName)
        {
            var commit = Repository.Branches[branchName].Tip;
            var id = Repository.ObjectDatabase.ShortenObjectId(commit);
            return new GitId(id);
        }

        public GitDirectoryInfo GetRootDirectory(GitId commitId)
        {
            var commit = Repository.Lookup<Commit>(commitId.Id);
            return new GitDirectoryInfo(commit.Tree);
        }

        public void Dispose()
        {
            if (m_Repository.IsValueCreated)
                m_Repository.Value.Dispose();
        }


    }
}
