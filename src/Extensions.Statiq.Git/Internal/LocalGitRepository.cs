using System;
using System.Collections.Generic;
using System.Linq;
using LibGit2Sharp;

namespace Grynwald.Extensions.Statiq.Git.Internal
{
    public class LocalGitRepository : IGitRepository
    {
        private readonly string m_RepositoryPath;


        public RepositoryKind Kind => RepositoryKind.Local;

        public IEnumerable<string> Branches
        {
            get
            {
                using var repository = OpenRepository();

                var branches = repository.Branches
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
        }


        public void Dispose()
        { }


        private Repository OpenRepository() => new Repository(m_RepositoryPath);
    }
}
