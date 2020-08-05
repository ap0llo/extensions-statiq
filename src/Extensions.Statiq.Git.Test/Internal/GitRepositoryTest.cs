using System;
using Grynwald.Extensions.Statiq.Git.Internal;
using NUnit.Framework;

namespace Grynwald.Extensions.Statiq.Git.Test.Internal
{
    /// <summary>
    /// Tests for <see cref="GitRepository"/>
    /// </summary>
    [TestFixture]
    public class GitRepositoryTest
    {
        [TestCase("https://github.com/example/example.git")]
        public void Create_returns_expected_GitRepository_for_remote_urls(string remoteUrl)
        {
            using var sut = GitRepository.Create(remoteUrl);
            Assert.AreEqual(RepositoryKind.Remote, sut.Kind);

            Assert.IsAssignableFrom<RemoteGitRepository>(sut);
        }

        [TestCase(@"C:\some\repo")]
        public void Create_returns_expected_GitRepository_for_local_paths(string remoteUrl)
        {
            using var sut = GitRepository.Create(remoteUrl);

            Assert.AreEqual(RepositoryKind.Local, sut.Kind);
            Assert.IsAssignableFrom<LocalGitRepository>(sut);
        }

        [TestCase("not-a-url")]
        [TestCase("git@github.com:example/example.git")]
        [TestCase("ssh://github.com/example/example.git")]
        public void Create_throws_ArgumentException_for_unsupported_remote_urls(string remoteUrl)
        {
            Assert.Throws<ArgumentException>(() => GitRepository.Create(remoteUrl));
        }
    }
}
