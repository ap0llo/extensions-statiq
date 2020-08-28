using System;
using System.Collections.Generic;
using System.Linq;
using Grynwald.Utilities.IO;
using LibGit2Sharp;

namespace Grynwald.Extensions.Statiq.Git.Internal
{
    public class RemoteGitRepository : IGitRepository
    {
        private TemporaryDirectory? m_RepositoryDirectory;
        private LocalGitRepository? m_LocalRepository;


        private LocalGitRepository LocalRepository
        {
            get
            {
                if (m_LocalRepository is null)
                {
                    // clone repository into a new temporary directory
                    m_RepositoryDirectory?.Dispose();
                    m_RepositoryDirectory = new TemporaryDirectory();
                    Repository.Clone(Url, m_RepositoryDirectory, new CloneOptions() { IsBare = true });

                    // create all remote branches as local branches
                    using var repository = new Repository(m_RepositoryDirectory);

                    var origin = repository.Network.Remotes["origin"];
                    var remoteBranches = repository
                        .Network.ListReferences(origin)
                        .Where(x => x.CanonicalName.StartsWith("refs/heads/"));

                    foreach (var remoteBranch in remoteBranches)
                    {
                        var name = remoteBranch.CanonicalName.Remove(0, "refs/heads/".Length);
                        var localBranch = repository.Branches[name];

                        if (localBranch is null)
                        {
                            repository.CreateBranch(name, remoteBranch.ResolveToDirectReference().Target.Sha);
                        }
                    }

                    m_LocalRepository = new LocalGitRepository(m_RepositoryDirectory);
                }

                return m_LocalRepository;
            }
        }


        public string Url { get; }

        public string? RepositoryDirectory => m_RepositoryDirectory?.FullName;

        public RepositoryKind Kind => RepositoryKind.Remote;

        public string CurrentBranch => LocalRepository.CurrentBranch;

        public IEnumerable<string> Branches => LocalRepository.Branches;

        public IEnumerable<GitTag> Tags => LocalRepository.Tags;


        public RemoteGitRepository(string remoteUrl)
        {
            if (String.IsNullOrWhiteSpace(remoteUrl))
                throw new ArgumentException("Value must not be null or whitespace", nameof(remoteUrl));

            Url = remoteUrl;
        }


        public GitId GetHeadCommitId(string branchName) => LocalRepository.GetHeadCommitId(branchName);

        public GitDirectoryInfo GetRootDirectory(GitId commitId) => LocalRepository.GetRootDirectory(commitId);

        public void Dispose()
        {
            m_LocalRepository?.Dispose();
            m_RepositoryDirectory?.Dispose();
        }

    }
}
