﻿using System.IO;
using System.Linq;
using FluentAssertions;
using Grynwald.Extensions.Statiq.Git.Internal;
using Grynwald.Extensions.Statiq.TestHelpers;
using NUnit.Framework;

namespace Grynwald.Extensions.Statiq.Git.Test.Internal
{
    /// <summary>
    /// Base test class for implementations of <see cref="IGitRepository"/>
    /// </summary>
    public abstract class GitRepositoryTest : GitTestBase
    {
        protected abstract IGitRepository CreateInstance(string repositoryUrl);


        [TestCase(new object[] { new string[0] })]
        [TestCase(new object[] { new string[] { "develop", "features/some-feature" } })]
        public void Branches_returns_expected_branches_from_local_repository(string[] additionalBranches)
        {
            // ARRANGE
            GitCommit(allowEmtpy: true);
            foreach (var branchName in additionalBranches)
            {
                Git($"checkout -b {branchName}");
            }

            // ACT
            using var sut = CreateInstance(m_WorkingDirectory);
            var branches = sut.Branches.ToList();

            // ASSERT
            branches.Count.Should().Be(1 + additionalBranches.Length);

            branches.Should().Contain("master");
            foreach (var branchName in additionalBranches)
            {
                branches.Should().Contain(branchName);
            }
        }

        [Test]
        public void GetHeadCommitId_returns_expected_Commit_for_default_branch()
        {
            // ARRANGE            
            var commitId = GitCommit(allowEmtpy: true);
            using var sut = CreateInstance(m_WorkingDirectory);

            // ACT
            var headCommit = sut.GetHeadCommitId("master");

            // ASSERT
            headCommit.Should().Be(commitId);
        }


        [Test]
        public void GetHeadCommitId_returns_expected_Commit()
        {
            // ARRANGE
            Git("checkout -b some-other-branch");
            var commitId = GitCommit(allowEmtpy: true);
            using var sut = CreateInstance(m_WorkingDirectory);

            // ACT
            var headCommit = sut.GetHeadCommitId("some-other-branch");

            // ASSERT
            headCommit.Should().Be(commitId);
        }

        [Test]
        public void Files_returns_expected_files()
        {
            // ARRANGE
            CreateFile("file1.txt");
            CreateFile("file2.txt");
            CreateFile("directory1/file3.txt");
            CreateFile("directory2/DIRECTORY3/file4.txt");

            GitAdd();
            var commit = GitCommit();

            // ACT
            using var sut = CreateInstance(m_WorkingDirectory);
            var files = sut.GetRootDirectory(commit).EnumerateFilesRescursively().ToList();

            // ASSERT
            files
                .Should().HaveCount(4)
                .And.Contain(x => x.FullName == "file1.txt")
                .And.Contain(x => x.FullName == "file2.txt")
                .And.Contain(x => x.FullName == "directory1/file3.txt")
                .And.Contain(x => x.FullName == "directory2/DIRECTORY3/file4.txt");
        }

        [Test]
        public void Files_returns_expected_files_from_specified_branch()
        {
            // ARRANGE
            var filePath1 = CreateFile("file1.txt");

            GitAdd();
            GitCommit();

            Git("checkout -b some-other-branch");
            File.Delete(filePath1);
            CreateFile("file2.txt");

            GitAdd();
            var commitId = GitCommit();

            using var sut = CreateInstance(m_WorkingDirectory);

            // ACT
            var files = sut.GetRootDirectory(commitId).EnumerateFilesRescursively().ToList();

            // ASSERT
            files
                .Should().HaveCount(1)
                .And.OnlyContain(x => x.FullName == "file2.txt");
        }

        [Test]
        public void Files_returns_expected_files_with_expected_content()
        {
            // ARRANGE
            var path = CreateFile("file1.txt", "Line1", "Line2", "", "Line3");
            var expectedContent = File.ReadAllText(path).Replace("\r\n", "\n");

            GitAdd();
            var commitId = GitCommit();

            // ACT
            using var sut = CreateInstance(m_WorkingDirectory);
            var files = sut.GetRootDirectory(commitId).EnumerateFilesRescursively().ToList();

            // ASSERT
            files
                .Should().ContainSingle()
                .Which.GetContentStream().ReadAsString().Should().Be(expectedContent);
        }

        [Test]
        public void CurrentBranch_returns_expected_value_01()
        {
            using var sut = CreateInstance(m_WorkingDirectory);
            sut.CurrentBranch.Should().Be("master");
        }

        [Test]
        public void CurrentBranch_returns_expected_value_02()
        {
            // ARRANGE
            GitCommit(allowEmtpy: true);
            Git("checkout -b my-default-branch");
            Git("branch -D master");

            using var sut = CreateInstance(m_WorkingDirectory);

            // ACT
            var currentBranch = sut.CurrentBranch;

            // ASSERT
            currentBranch.Should().Be("my-default-branch");
        }

        [Test]
        public void Tags_returns_empty_enumerable_if_there_are_no_tags()
        {
            // ARRANGE
            GitCommit(allowEmtpy: true);

            using var sut = CreateInstance(m_WorkingDirectory);

            // ACT
            var tags = sut.Tags;

            // ASSERT
            tags.Should().NotBeNull().And.BeEmpty();
        }


        [Test]
        public void Tags_returns_expected_tags()
        {
            // ARRANGE
            var commit1 = GitCommit(allowEmtpy: true);
            _ = GitCommit(allowEmtpy: true);
            var commit3 = GitCommit(allowEmtpy: true);

            var tag1 = GitTag("my-tag-1", commit1);
            var tag2 = GitTag("my-tag-2", commit3);

            using var sut = CreateInstance(m_WorkingDirectory);

            // ACT
            var tags = sut.Tags;

            // ASSERT
            tags
                .Should().NotBeNull()
                .And.HaveCount(2)
                .And.Contain(tag1)
                .And.Contain(tag2);
        }

    }
}
