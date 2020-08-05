using System.IO;
using Grynwald.Extensions.Statiq.Git.Internal;
using NUnit.Framework;

namespace Grynwald.Extensions.Statiq.Git.Test.Internal
{
    [TestFixture]
    public class RemoteGitRepositoryTest : GitRepositoryTestBase
    {
        protected override IGitRepository CreateInstance(string repositoryUrl) => new RemoteGitRepository(repositoryUrl);

        [Test]
        public void Disposing_the_GitRepository_deletes_the_temporary_repository_directory()
        {
            var sut = new RemoteGitRepository(m_WorkingDirectory);

            // Access branches to trigger cloning of the repository
            _ = sut.Branches;

            Assert.NotNull(sut.RepositoryDirectory);
            Assert.True(Directory.Exists(sut.RepositoryDirectory));

            sut.Dispose();

            Assert.NotNull(sut.RepositoryDirectory);
            Assert.False(Directory.Exists(sut.RepositoryDirectory));
        }
    }
}
