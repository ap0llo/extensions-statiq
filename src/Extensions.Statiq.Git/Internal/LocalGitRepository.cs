using System;
using System.Collections.Generic;
using System.IO;
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



        public IReadOnlyList<GitFile> GetFiles(string branch)
        {
            using var repository = OpenRepository();

            var commit = repository.Branches[branch].Tip;
            var commitId = repository.ObjectDatabase.ShortenObjectId(commit);
            return EnumerateFiles(commitId, commit.Tree).ToArray();

        }

        public void Dispose()
        { }

        public Stream GetFileContentStream(ObjectId id)
        {
            using var repo = OpenRepository();
            return repo.Lookup<Blob>(id).GetContentStream();
        }


        private Repository OpenRepository() => new Repository(m_RepositoryPath);

        private IEnumerable<GitFile> EnumerateFiles(string commitId, Tree tree)
        {
            foreach (var item in tree)
            {
                if (item.Target is Tree subtree)
                {
                    foreach (var file in EnumerateFiles(commitId, subtree))
                    {
                        yield return file;
                    }
                }
                else if (item.Target is Blob)
                {
                    yield return new GitFile(this, commitId, item.Path, item.Target.Id);
                }
            }
        }
    }
}
