using System;
using FluentAssertions;
using Grynwald.Extensions.Statiq.Git.Internal;
using NUnit.Framework;

namespace Grynwald.Extensions.Statiq.Git.Test.Internal
{
    [TestFixture]
    public class LocalGitRepositoryTest : GitRepositoryTest
    {
        protected override IGitRepository CreateInstance(string repositoryUrl) => new LocalGitRepository(repositoryUrl);

        [Test]
        public void CurrentBranch_throws_InvalidOperationException_if_repository_is_in_detached_HEAD_state()
        {
            var commit1 = GitCommit(allowEmtpy: true);
            _ = GitCommit(allowEmtpy: true);
            Git($"checkout {commit1}");

            using var sut = new LocalGitRepository(m_WorkingDirectory);

            Action act = () => _ = sut.CurrentBranch;
            act.Should().Throw<InvalidOperationException>().WithMessage("*'detached HEAD'*");
        }
    }
}
