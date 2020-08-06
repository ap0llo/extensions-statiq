using System.IO;
using FluentAssertions;
using Grynwald.Extensions.Statiq.Git.Internal;
using NUnit.Framework;

namespace Grynwald.Extensions.Statiq.Git.Test.Internal
{
    [TestFixture]
    public class RemoteGitRepositoryTest : GitRepositoryTest
    {
        protected override IGitRepository CreateInstance(string repositoryUrl) => new RemoteGitRepository(repositoryUrl);

        [Test]
        public void Disposing_the_GitRepository_deletes_the_temporary_repository_directory()
        {
            // create repository instance, but do NOT use a "using" statement, Dispose() is called explicitly below.
            var sut = new RemoteGitRepository(m_WorkingDirectory);

            // Access branches to trigger cloning of the repository
            _ = sut.Branches;

            sut.RepositoryDirectory.Should().NotBeNullOrEmpty();
            Directory.Exists(sut.RepositoryDirectory).Should().BeTrue();

            sut.Dispose();

            sut.RepositoryDirectory.Should().NotBeNullOrEmpty();
            Directory.Exists(sut.RepositoryDirectory).Should().BeFalse();
        }
    }
}
