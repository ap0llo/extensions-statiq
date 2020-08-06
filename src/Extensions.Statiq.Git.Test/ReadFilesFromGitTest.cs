using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Statiq.Common;
using Statiq.Testing;

namespace Grynwald.Extensions.Statiq.Git.Test
{
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
            var output = await BaseFixture.ExecuteAsync(sut).SingleAsync();

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
                .Which.Value.Should().Be("file1.txt");
        }

        private static IEnumerable<object[]> GlobbingTestCases()
        {
            object[] TestCase(string[] files, string[] patterns, string[] expectedOutput)
            {
                return new object[] { files, patterns, expectedOutput };
            }

            string[] Input(params string[] items)
            {
                return items;
            }

            string[] Patterns(params string[] items)
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
        }

        [TestCaseSource(nameof(GlobbingTestCases))]
        public async Task Execute_returns_only_files_matched_by_patterns(string[] files, string[] patterns, string[] expectedOutput)
        {
            // ARRANGE
            var repositoryUrl = m_WorkingDirectory.FullName;
            foreach (var file in files)
            {
                CreateFile(file);
            }
            GitAdd();
            GitCommit(allowEmtpy: true);

            var sut = new ReadFilesFromGit(repositoryUrl, patterns);

            // ACT
            var output = await BaseFixture.ExecuteAsync(sut);

            // ASSERT
            output.Should().HaveCount(expectedOutput.Length);

            var outputPaths = output.Select(x => x.GetString(GitKeys.GitRelativePath)).ToArray();
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
                .WithBranchPatterns("master", "branch*");

            // ACT
            var outputs = await BaseFixture.ExecuteAsync(sut);

            // ASSERT
            outputs.Should().HaveCount(2);

            var output1 = outputs.Single(x => x.GetString(GitKeys.GitBranch) == "master");
            var output2 = outputs.Single(x => x.GetString(GitKeys.GitBranch) == "branch2");

            (await output1.GetContentStringAsync()).Should().Be("Content 1\n");
            (await output2.GetContentStringAsync()).Should().Be("Content 2\n");

            output1
                .Should().Contain(x => x.Key == GitKeys.GitRepositoryUrl)
                .Which.Value.Should().Be(repositoryUrl);
            output1
                .Should().Contain(x => x.Key == GitKeys.GitBranch)
                .Which.Value.Should().Be("master");
            output1
                .Should().Contain(x => x.Key == GitKeys.GitCommit)
                .Which.Value
                    .Should().BeAssignableTo<string>()
                    .And.Be(commit1.Id);
            output1
                .Should().Contain(x => x.Key == GitKeys.GitRelativePath)
                .Which.Value.Should().Be("file1.txt");

            output2
                 .Should().Contain(x => x.Key == GitKeys.GitRepositoryUrl)
                 .Which.Value.Should().Be(repositoryUrl);
            output2
                .Should().Contain(x => x.Key == GitKeys.GitBranch)
                .Which.Value.Should().Be("branch2");
            output2
                .Should().Contain(x => x.Key == GitKeys.GitCommit)
                .Which.Value
                    .Should().BeAssignableTo<string>()
                    .And.Be(commit2.Id);
            output2
                .Should().Contain(x => x.Key == GitKeys.GitRelativePath)
                .Which.Value.Should().Be("file2.txt");
        }
    }
}
