using System;
using FluentAssertions;
using Grynwald.Extensions.Statiq.Git.Internal;
using NUnit.Framework;

namespace Grynwald.Extensions.Statiq.Git.Test.Internal
{
    /// <summary>
    /// Tests for <see cref="GitRepositoryFactory"/>
    /// </summary>
    [TestFixture]
    public class GitRepositoryFactoryTest
    {
        [TestCase("https://github.com/example/example.git")]
        public void GetRepository_returns_expected_GitRepository_for_remote_urls(string remoteUrl)
        {
            using var sut = GitRepositoryFactory.GetRepository(remoteUrl);

            sut.Kind.Should().Be(RepositoryKind.Remote);
            sut.Should().BeAssignableTo<RemoteGitRepository>();
        }

        [TestCase(@"C:\some\repo")]
        public void GetRepository_returns_expected_GitRepository_for_local_paths(string remoteUrl)
        {
            using var sut = GitRepositoryFactory.GetRepository(remoteUrl);

            sut.Kind.Should().Be(RepositoryKind.Local);
            sut.Should().BeAssignableTo<LocalGitRepository>();
        }

        [TestCase("not-a-url")]
        [TestCase("git@github.com:example/example.git")]
        [TestCase("ssh://github.com/example/example.git")]
        public void GetRepository_throws_ArgumentException_for_unsupported_remote_urls(string remoteUrl)
        {
            Action act = () => GitRepositoryFactory.GetRepository(remoteUrl);
            act.Should().Throw<ArgumentException>();
        }
    }
}
