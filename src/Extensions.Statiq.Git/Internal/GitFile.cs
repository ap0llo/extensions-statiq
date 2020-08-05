using System;
using System.IO;
using LibGit2Sharp;

namespace Grynwald.Extensions.Statiq.Git.Internal
{
    public sealed class GitFile
    {
        private readonly LocalGitRepository m_Repository;
        private readonly ObjectId m_Id;


        public string Path { get; }

        public string Commit { get; }


        internal GitFile(LocalGitRepository repository, string commit, string path, ObjectId id)
        {
            if (String.IsNullOrWhiteSpace(commit))
                throw new ArgumentException("Value must not be null or whitespace", nameof(commit));

            if (String.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Value must not be null or whitespace", nameof(path));
            m_Repository = repository ?? throw new ArgumentNullException(nameof(repository));
            Commit = commit;
            Path = path;
            m_Id = id ?? throw new ArgumentNullException(nameof(id));
        }


        public Stream GetContentStream() => m_Repository.GetFileContentStream(m_Id);
    }
}
