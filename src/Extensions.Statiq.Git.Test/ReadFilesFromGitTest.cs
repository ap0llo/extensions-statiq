using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Statiq.Common;

namespace Grynwald.Extensions.Statiq.Git.Test
{
    /// <summary>
    /// Tests for <see cref="ReadFilesFromGit"/>
    /// </summary>
    [TestFixture]
    public class ReadFilesFromGitTest : GitTestBase
    {
        [Test]
        public async Task Execute_returns_file_with_expected_content_and_metadata()
        {
            // ARRANGE
            var repositoryUrl = m_WorkingDirectory.FullName;
            CreateFile("file1.txt", "Content");
            GitAdd();
            var commitId = GitCommit();

            var sut = new ReadFilesFromGit(repositoryUrl, "*");

            // ACT
            var output = await ExecuteAsync(sut).SingleAsync();

            // ASSERT
            var content = await output.GetContentStringAsync();
            content.Should().Be("Content\n");

            output
                .Should().Contain(x => x.Key == GitKeys.GitRepositoryUrl)
                .Which.Value.Should().Be(repositoryUrl);
            output
                .Should().Contain(x => x.Key == GitKeys.GitBranch)
                .Which.Value.Should().Be("master");
            output
                .Should().Contain(x => x.Key == GitKeys.GitCommit)
                .Which.Value
                    .Should().BeAssignableTo<string>()
                    .And.Be(commitId.Id);
            output
                .Should().Contain(x => x.Key == GitKeys.GitRelativePath)
                .Which.Value.Should().BeAssignableTo<NormalizedPath>().
                And.Be((NormalizedPath)"file1.txt");
        }

        private static IEnumerable<object?[]> GlobbingTestCases()
        {
            object?[] TestCase(string[] files, string[]? patterns, string[] expectedOutput)
            {
                return new object?[] { files, patterns, expectedOutput };
            }

            string[] Input(params string[] items)
            {
                return items;
            }

            string[]? Patterns(params string[]? items)
            {
                return items;
            }

            string[] Output(params string[] items)
            {
                return items;
            }

            yield return TestCase(
                Input(),
                Patterns("*"),
                Output()
            );

            yield return TestCase(
                Input("file1.txt", "dir/file2.txt"),
                Patterns("*.md"),
                Output());

            yield return TestCase(
                Input("file1.txt", "dir/file2.txt"),
                Patterns("**"),
                Output("file1.txt", "dir/file2.txt"));

            yield return TestCase(
                Input("file1.txt", "file2.md"),
                Patterns("*.md"),
                Output("file2.md"));

            yield return TestCase(
                Input("dir1/file1.txt", "dir2/file2.txt"),
                Patterns("dir1/*.txt"),
                Output("dir1/file1.txt"));

            yield return TestCase(
                Input("file1.txt", "file2.txt", "file3.md"),
                Patterns("*", "!*.txt"),
                Output("file3.md"));

            yield return TestCase(
                Input("file1.txt", "file2.txt", "file3.md"),
                Patterns("*.md", "*.txt"),
                Output("file1.txt", "file2.txt", "file3.md"));

            // specifying no patterns should return all files
            yield return TestCase(
              Input("file1.txt", "dir/file2.txt"),
              Patterns(null),
              Output("file1.txt", "dir/file2.txt"));
        }

        [TestCaseSource(nameof(GlobbingTestCases))]
        public async Task Execute_returns_only_files_matched_by_patterns(string[] files, string[]? patterns, string[] expectedOutput)
        {
            // ARRANGE
            var repositoryUrl = m_WorkingDirectory.FullName;
            foreach (var file in files)
            {
                CreateFile(file);
            }
            GitAdd();
            GitCommit(allowEmtpy: true);

            var sut = patterns is null
                ? new ReadFilesFromGit(repositoryUrl)
                : new ReadFilesFromGit(repositoryUrl, patterns);

            // ACT
            var output = await ExecuteAsync(sut);

            // ASSERT
            output.Should().HaveCount(expectedOutput.Length);

            var outputPaths = output.Select(x => x.Get<NormalizedPath>(GitKeys.GitRelativePath)).ToArray();
            foreach (var path in expectedOutput)
            {
                outputPaths.Should().Contain(path);
            }
        }

        [Test]
        public async Task Execute_returns_file_from_multiple_branches()
        {
            // ARRANGE
            var repositoryUrl = m_WorkingDirectory.FullName;
            var initialCommit = GitCommit(allowEmtpy: true);
            CreateFile("file1.txt", "Content 1");
            GitAdd();
            var commit1 = GitCommit();

            Git($"checkout {initialCommit} -b branch2");
            CreateFile("file2.txt", "Content 2");
            GitAdd();
            var commit2 = GitCommit();

            var sut = new ReadFilesFromGit(repositoryUrl, "*")
                .WithBranchNames("master", "branch*");

            // ACT
            var outputs = await ExecuteAsync(sut);

            // ASSERT
            outputs.Should().HaveCount(2);

            var output1 = outputs.Single(x => x.GetGitBranch() == "master");
            var output2 = outputs.Single(x => x.GetGitBranch() == "branch2");

            (await output1.GetContentStringAsync()).Should().Be("Content 1\n");
            (await output2.GetContentStringAsync()).Should().Be("Content 2\n");

            output1
                .Should().Contain(x => x.Key == GitKeys.GitRepositoryUrl)
                .Which.Value.Should().Be(repositoryUrl);
            output1
                .Should().Contain(x => x.Key == GitKeys.GitBranch)
                .Which.Value.Should().Be("master");
            output1
                .Should().NotContain(x => x.Key == GitKeys.GitTag);
            output1
                .Should().Contain(x => x.Key == GitKeys.GitCommit)
                .Which.Value
                    .Should().BeAssignableTo<string>()
                    .And.Be(commit1.Id);
            output1
                .Should().Contain(x => x.Key == GitKeys.GitRelativePath)
                .Which.Value
                    .Should().BeAssignableTo<NormalizedPath>()
                    .And.Be((NormalizedPath)"file1.txt");

            output2
                 .Should().Contain(x => x.Key == GitKeys.GitRepositoryUrl)
                 .Which.Value.Should().Be(repositoryUrl);
            output2
                .Should().Contain(x => x.Key == GitKeys.GitBranch)
                .Which.Value.Should().Be("branch2");
            output2
                .Should().NotContain(x => x.Key == GitKeys.GitTag);
            output2
                .Should().Contain(x => x.Key == GitKeys.GitCommit)
                .Which.Value
                    .Should().BeAssignableTo<string>()
                    .And.Be(commit2.Id);
            output2
                .Should().Contain(x => x.Key == GitKeys.GitRelativePath)
                .Which.Value
                    .Should().BeAssignableTo<NormalizedPath>()
                    .And.Be((NormalizedPath)"file2.txt");
        }

        [Test]
        public async Task Execute_emits_warning_is_no_branches_match_the_specified_names()
        {
            // ARRANGE
            var repositoryUrl = m_WorkingDirectory.FullName;
            CreateFile("file1.txt");
            GitAdd();
            GitCommit();

            var patterns = new[] { "branch1*", "branch2*" };

            var sut = new ReadFilesFromGit(repositoryUrl, "*")
                .WithBranchNames(patterns);

            // ACT
            var outputs = await ExecuteAsync(sut);

            // ASSERT
            outputs.Should().BeEmpty();

            m_TestExecutionContext.LogMessages.Should().OnlyContain(x => x.LogLevel == LogLevel.Warning);
            foreach (var pattern in patterns)
            {
                m_TestExecutionContext.LogMessages
                    .Should().Contain(message => message.FormattedMessage.Contains($"'{pattern}'"));
            }
        }

        [Test]
        public async Task Execute_reads_files_from_default_branch_if_no_branch_names_are_specified()
        {
            // ARRANGE
            var repositoryUrl = m_WorkingDirectory.FullName;
            var initalCommit = GitCommit(allowEmtpy: true);
            CreateFile("file1.txt");
            GitAdd();
            GitCommit();

            Git($"checkout {initalCommit} -b my-new-default-branch");
            CreateFile("file2.txt");
            GitAdd();
            GitCommit();

            Git("branch -D master");

            var sut = new ReadFilesFromGit(repositoryUrl, "*");

            // ACT
            var outputs = await ExecuteAsync(sut);

            // ASSERT
            outputs.Should().ContainSingle()
                .Which.Should().Contain(x => x.Key == GitKeys.GitRelativePath)
                .Which.Value
                    .Should().BeAssignableTo<NormalizedPath>()
                    .And.Be((NormalizedPath)"file2.txt");
        }

        [Test]
        public async Task Execute_reads_no_documents_if_branches_are_ignored()
        {
            // ARRANGE
            var repositoryUrl = m_WorkingDirectory.FullName;
            _ = GitCommit(allowEmtpy: true);
            CreateFile("file1.txt");
            GitAdd();
            GitCommit();

            var sut = new ReadFilesFromGit(repositoryUrl, "*")
                .IgnoreBranches();

            // ACT
            var outputs = await ExecuteAsync(sut);

            // ASSERT
            outputs.Should().BeEmpty();
        }

        [Test]
        public async Task Execute_returns_file_from_multiple_tags()
        {
            // ARRANGE
            var repositoryUrl = m_WorkingDirectory.FullName;

            var initialCommit = GitCommit(allowEmtpy: true);
            CreateFile("file1.txt", "Content 1");
            GitAdd();
            var commit1 = GitCommit();
            var tag1 = GitTag("tag1", commit1);

            Git($"checkout {initialCommit} -b branch2");
            CreateFile("file2.txt", "Content 2");
            GitAdd();
            var commit2 = GitCommit();
            var tag2 = GitTag("another-tag", commit2);

            var sut = new ReadFilesFromGit(repositoryUrl, "*")
                .IgnoreBranches()
                .WithTagNames("tag1", "another-*");

            // ACT
            var outputs = await ExecuteAsync(sut);

            // ASSERT
            outputs.Should().HaveCount(2);

            var output1 = outputs.Single(x => x.GetGitTag() == "tag1");
            var output2 = outputs.Single(x => x.GetGitTag() == "another-tag");

            (await output1.GetContentStringAsync()).Should().Be("Content 1\n");
            (await output2.GetContentStringAsync()).Should().Be("Content 2\n");

            output1
                .Should().Contain(x => x.Key == GitKeys.GitRepositoryUrl)
                .Which.Value.Should().Be(repositoryUrl);
            output1
                .Should().NotContain(x => x.Key == GitKeys.GitBranch);
            output1
                .Should().Contain(x => x.Key == GitKeys.GitTag)
                .Which.Value.Should().Be("tag1");
            output1
                .Should().Contain(x => x.Key == GitKeys.GitCommit)
                .Which.Value
                    .Should().BeAssignableTo<string>()
                    .And.Be(commit1.Id);
            output1
                .Should().Contain(x => x.Key == GitKeys.GitRelativePath)
                .Which.Value
                    .Should().BeAssignableTo<NormalizedPath>()
                    .And.Be((NormalizedPath)"file1.txt");

            output2
                 .Should().Contain(x => x.Key == GitKeys.GitRepositoryUrl)
                 .Which.Value.Should().Be(repositoryUrl);
            output2
                .Should().NotContain(x => x.Key == GitKeys.GitBranch);
            output2
                .Should().Contain(x => x.Key == GitKeys.GitTag)
                .Which.Value.Should().Be("another-tag");
            output2
                .Should().Contain(x => x.Key == GitKeys.GitCommit)
                .Which.Value
                    .Should().BeAssignableTo<string>()
                    .And.Be(commit2.Id);
            output2
                .Should().Contain(x => x.Key == GitKeys.GitRelativePath)
                .Which.Value
                    .Should().BeAssignableTo<NormalizedPath>()
                    .And.Be((NormalizedPath)"file2.txt");
        }

        [Test]
        public async Task Execute_emits_warning_if_no_tags_match_the_specified_names()
        {
            // ARRANGE
            var repositoryUrl = m_WorkingDirectory.FullName;
            CreateFile("file1.txt");
            GitAdd();
            GitCommit();

            var patterns = new[] { "tag1*", "tag2*" };

            var sut = new ReadFilesFromGit(repositoryUrl, "*")
                .IgnoreBranches()
                .WithTagNames(patterns);

            // ACT
            var outputs = await ExecuteAsync(sut);

            // ASSERT
            outputs.Should().BeEmpty();

            m_TestExecutionContext.LogMessages.Should().OnlyContain(x => x.LogLevel == LogLevel.Warning);
            foreach (var pattern in patterns)
            {
                m_TestExecutionContext.LogMessages
                    .Should().Contain(message => message.FormattedMessage.Contains($"'{pattern}'"));
            }
        }

        [TestCase("master")]
        [TestCase("some/other/branch")]
        public async Task Execute_adds_a_placeholder_source_path_to_the_documents_read_from_branches(string branchName)
        {
            // ARRANGE
            Git($"checkout -b {branchName}");
            var repositoryUrl = m_WorkingDirectory.FullName;
            CreateFile("dir1/file1.txt");
            GitAdd();
            GitCommit();

            var sut = new ReadFilesFromGit(repositoryUrl);

            // ACT
            var outputs = await ExecuteAsync(sut);

            // ASSERT
            outputs
                .Should().ContainSingle()
                .Which.Source
                    .Should().Match<NormalizedPath>(x => !x.IsNull)
                    .And.Subject.ToString()
                        .Should().EndWith($"{branchName}/dir1/file1.txt")
                        .And.Contain("/git/");
        }

        [TestCase("tag1")]
        [TestCase("another-tag")]
        public async Task Execute_adds_a_placeholder_source_path_to_the_documents_read_from_tags(string tagName)
        {
            // ARRANGE
            var repositoryUrl = m_WorkingDirectory.FullName;
            CreateFile("dir1/file1.txt");
            GitAdd();
            GitTag(tagName, GitCommit());

            var sut = new ReadFilesFromGit(repositoryUrl)
                .IgnoreBranches()
                .WithTagNames(tagName);

            // ACT
            var outputs = await ExecuteAsync(sut);

            // ASSERT
            outputs
                .Should().ContainSingle()
                .Which.Source
                    .Should().Match<NormalizedPath>(x => !x.IsNull)
                    .And.Subject.ToString()
                        .Should().EndWith($"{tagName}/dir1/file1.txt")
                        .And.Contain("/git/");
        }
    }
}
