using System.Linq;
using Grynwald.Extensions.Statiq.Git.Internal;
using NUnit.Framework;

namespace Grynwald.Extensions.Statiq.Git.Test.Internal
{
    [TestFixture]
    public class LocalGitRepositoryTest : GitTestBase
    {
        [TestCase(new object[] { new string[0] })]
        [TestCase(new object[] { new string[] { "develop", "features/some-feature" } })]
        public void Branches_returns_expected_branches_from_local_repository(string[] additionalBranches)
        {
            // ARRANGE
            Git("commit --allow-empty -m Commit1");
            foreach (var branchName in additionalBranches)
            {
                Git($"checkout -b {branchName}");
            }

            // ACT
            using var sut = new LocalGitRepository(m_WorkingDirectory);
            var branches = sut.Branches.ToList();

            // ASSERT
            Assert.AreEqual(1 + additionalBranches.Length, branches.Count);
            Assert.Contains("master", branches);
            foreach (var branchName in additionalBranches)
            {
                Assert.Contains(branchName, branches);
            }
        }
    }
}
