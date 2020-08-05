using System;
using FluentAssertions;
using Grynwald.Extensions.Statiq.Git.Internal;
using LibGit2Sharp;
using NUnit.Framework;

namespace Grynwald.Extensions.Statiq.Git.Test.Internal
{
    /// <summary>
    /// Tests for <see cref="GitFileInfo"/>
    /// </summary>
    public class GitFileInfoTest : GitTestBase
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            GitCommit(allowEmtpy: true);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("\t")]
        [TestCase("name/name")]
        [TestCase("name\\name")]
        public void Constructor_throws_ArgumentException_for_invalid_names(string name)
        {
            using var repository = new Repository(m_WorkingDirectory);

            Action act = () => new GitFileInfo(name, new GitDirectoryInfo(repository.Head.Tip.Tree));
            act.Should().Throw<ArgumentException>();
        }

        [Test]
        public void Constructor_throws_ArgumentException_if_parent_is_null()
        {
            Action act = () => new GitFileInfo("dir", null!);
            act.Should().Throw<ArgumentNullException>();
        }



    }
}
