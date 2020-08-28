using System;
using FluentAssertions;
using Grynwald.Extensions.Statiq.Git.Internal;
using LibGit2Sharp;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using NUnit.Framework;

namespace Grynwald.Extensions.Statiq.Git.Test.Internal
{
    /// <summary>
    /// Tests for <see cref="GitDirectoryInfo"/>
    /// </summary>
    public class GitDirectoryInfoTest : GitTestBase
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            GitCommit(allowEmtpy: true);
        }

        [Test]
        public void Root_directory_has_empty_path()
        {
            using var repository = new Repository(m_WorkingDirectory);

            var sut = new GitDirectoryInfo(repository.Head.Tip);

            sut.Name.Should().Be("");
            sut.FullName.Should().Be("");
            sut.ParentDirectory.Should().BeNull();
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
            var root = new GitDirectoryInfo(repository.Head.Tip);

            Action act = () => new GitDirectoryInfo(root.Commit, name, root, repository.Head.Tip.Tree);
            act.Should().Throw<ArgumentException>();
        }

        [Test]
        public void Constructor_throws_ArgumentException_if_parent_is_null()
        {
            using var repository = new Repository(m_WorkingDirectory);
            var commitId = repository.Head.Tip.GetGitId();

            Action act = () => new GitDirectoryInfo(commitId, "dir", null!, repository.Head.Tip.Tree);
            act.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Constructor_throws_ArgumentException_if_Commit_is_null()
        {
            Action act = () => new GitDirectoryInfo(null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Constructor_throws_ArgumentException_if_tree_is_null()
        {
            using var repository = new Repository(m_WorkingDirectory);
            var root = new GitDirectoryInfo(repository.Head.Tip);

            Action act = () => new GitDirectoryInfo(root.Commit, "dir", root, null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void EnumerateFileSystemInfos_returns_expected_items_for_empty_repository()
        {
            using var repository = new Repository(m_WorkingDirectory);

            var sut = new GitDirectoryInfo(repository.Head.Tip);

            sut.EnumerateFileSystemInfos()
                .Should().NotBeNull()
                .And.BeEmpty();
        }

        [Test]
        public void EnumerateFileSystemInfos_returns_expected_items()
        {
            // ARRANGE
            var files = new[]
            {
                "file1.txt",
                "file2.txt",
                "dir1/file3.txt",
                "dir2/dir3/file4.txt"
            };

            foreach (var file in files)
            {
                CreateFile(file);
            }
            GitAdd();
            GitCommit();

            using var repository = new Repository(m_WorkingDirectory);

            // ACT
            var sut = new GitDirectoryInfo(repository.Head.Tip);
            var fileSystemInfos = sut.EnumerateFileSystemInfos();

            // ASSERT
            fileSystemInfos
                .Should().NotBeNull()
                .And.HaveCount(files.Length);

            fileSystemInfos
                .Should().Contain(f => f.FullName == "file1.txt")
                .And.Contain(x => x.FullName == "file2.txt")
                .And.Contain(x => x.FullName == "dir1")
                .And.Contain(x => x.FullName == "dir2");
        }

        [TestCase("dir1", "dir1", "dir1")]
        [TestCase("dir2", "dir2", "dir2")]
        [TestCase("dir2/dir3", "dir3", "dir2/dir3")]
        [TestCase("dir2\\dir3", "dir3", "dir2/dir3")]
        public void GetDirectory_returns_expected_directory(string path, string expectedName, string expectedFullName)
        {
            // ARRANGE
            var files = new[]
            {
                "file1.txt",
                "file2.txt",
                "dir1/file3.txt",
                "dir2/dir3/file4.txt"
            };

            foreach (var file in files)
            {
                CreateFile(file);
            }
            GitAdd();
            GitCommit();

            using var repository = new Repository(m_WorkingDirectory);

            // ACT
            var sut = new GitDirectoryInfo(repository.Head.Tip);

            // ASSERT
            sut.GetDirectory(path)
                .Should().NotBeNull()
                .And.Match<DirectoryInfoBase>(dir => dir.FullName == expectedFullName)
                .And.Match<DirectoryInfoBase>(dir => dir.Name == expectedName);
        }

        [TestCase("dir1")]
        [TestCase("dir2/dir3")]
        [TestCase("dir2\\dir3")]
        public void GetDirectory_returns_if_directory_does_not_exist(string path)
        {
            // ARRANGE            
            using var repository = new Repository(m_WorkingDirectory);
            var sut = new GitDirectoryInfo(repository.Head.Tip);

            // ACT
            var dir = sut.GetDirectory(path);

            // ASSERT
            dir.Should().BeNull();
        }

        [TestCase("file1.txt", "file1.txt", "file1.txt")]
        [TestCase("file2.txt", "file2.txt", "file2.txt")]
        [TestCase("dir1/file3.txt", "file3.txt", "dir1/file3.txt")]
        [TestCase("dir1\\file3.txt", "file3.txt", "dir1/file3.txt")]
        [TestCase("dir2/dir3/file4.txt", "file4.txt", "dir2/dir3/file4.txt")]
        [TestCase("dir2\\dir3/file4.txt", "file4.txt", "dir2/dir3/file4.txt")]
        [TestCase("dir2/dir3\\file4.txt", "file4.txt", "dir2/dir3/file4.txt")]
        [TestCase("dir2\\dir3\\file4.txt", "file4.txt", "dir2/dir3/file4.txt")]
        public void GetFile_returns_expected_file(string path, string expectedName, string expectedFullName)
        {
            // ARRANGE
            var files = new[]
            {
                "file1.txt",
                "file2.txt",
                "dir1/file3.txt",
                "dir2/dir3/file4.txt"
            };

            foreach (var file in files)
            {
                CreateFile(file);
            }
            GitAdd();
            GitCommit();

            using var repository = new Repository(m_WorkingDirectory);

            // ACT
            var sut = new GitDirectoryInfo(repository.Head.Tip);

            // ASSERT
            sut.GetFile(path)
                .Should().NotBeNull()
                .And.Match<FileInfoBase>(dir => dir.FullName == expectedFullName)
                .And.Match<FileInfoBase>(dir => dir.Name == expectedName);
        }

        [TestCase("file1.txt")]
        [TestCase("dir1/file3.txt")]
        [TestCase("dir1\\file3.txt")]
        [TestCase("dir2/dir3/file4.txt")]
        [TestCase("dir2\\dir3/file4.txt")]
        [TestCase("dir2/dir3\\file4.txt")]
        [TestCase("dir2\\dir3\\file4.txt")]
        public void GetFile_returns_null_if_file_does_not_exist(string path)
        {
            // ARRANGE            
            using var repository = new Repository(m_WorkingDirectory);
            var sut = new GitDirectoryInfo(repository.Head.Tip);

            // ACT
            var file = sut.GetFile(path);

            // ASSERT
            file.Should().BeNull();
        }
    }
}
