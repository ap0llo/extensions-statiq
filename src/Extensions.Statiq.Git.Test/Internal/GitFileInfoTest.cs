using System;
using System.IO;
using System.Linq;
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
            CreateFile("file1.txt");
            GitAdd();
            GitCommit();

            using var repository = new Repository(m_WorkingDirectory);
            var blob = (Blob)repository.Head.Tip.Tree.First(x => x.Name == "file1.txt").Target;

            Action act = () => new GitFileInfo(name, new GitDirectoryInfo(repository.Head.Tip), blob);
            act.Should().Throw<ArgumentException>();
        }

        [Test]
        public void Constructor_throws_ArgumentException_if_parent_is_null()
        {
            CreateFile("file1.txt");
            GitAdd();
            GitCommit();

            using var repository = new Repository(m_WorkingDirectory);
            var blob = (Blob)repository.Head.Tip.Tree.First(x => x.Name == "file1.txt").Target;

            Action act = () => new GitFileInfo("file", null!, blob);
            act.Should().Throw<ArgumentNullException>();
        }


        [Test]
        public void Constructor_throws_ArgumentException_if_blob_is_null()
        {
            using var repository = new Repository(m_WorkingDirectory);

            Action act = () => new GitFileInfo("file", new GitDirectoryInfo(repository.Head.Tip), null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void GetContentStream_returns_expected_content()
        {
            // ARRANGE
            var path = CreateFile("file1.txt", "Line1", "Line2", "", "Line3");
            var expectedContent = File.ReadAllText(path).Replace("\r\n", "\n");
            GitAdd();
            GitCommit();

            using var repository = new Repository(m_WorkingDirectory);
            var blob = (Blob)repository.Head.Tip.Tree.First(x => x.Name == "file1.txt").Target;
            var sut = new GitFileInfo("file1.txt", new GitDirectoryInfo(repository.Head.Tip), blob);

            // ACT
            using var contentStream = sut.GetContentStream();
            var content = contentStream.ReadAsString();

            // ASSERT
            content.Should().Be(expectedContent);
        }
    }
}
