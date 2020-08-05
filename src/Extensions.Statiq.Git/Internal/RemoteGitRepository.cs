using System;
using System.Collections.Generic;
using Grynwald.Utilities.IO;

namespace Grynwald.Extensions.Statiq.Git.Internal
{
    public class RemoteGitRepository : GitRepository
    {
        private readonly string m_RemoteUrl;
        private readonly TemporaryDirectory m_TempDirectory;


        public override RepositoryKind Kind => RepositoryKind.Remote;

        public override IEnumerable<string> Branches => throw new NotImplementedException();


        public RemoteGitRepository(string remoteUrl)
        {
            if (String.IsNullOrWhiteSpace(remoteUrl))
                throw new ArgumentException("Value must not be null or whitespace", nameof(remoteUrl));

            m_RemoteUrl = remoteUrl;
            m_TempDirectory = new TemporaryDirectory();
        }


        public override void Dispose() => m_TempDirectory.Dispose();
    }
}
