using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using Grynwald.Extensions.Statiq.Git.Internal;
using Grynwald.Utilities.IO;
using NUnit.Framework;

namespace Grynwald.Extensions.Statiq.Git.Test.Internal
{
    public abstract class GitRepositoryTestBase
    {
        protected TemporaryDirectory m_WorkingDirectory = null!;

        [SetUp]
        public void SetUp()
        {
            m_WorkingDirectory = new TemporaryDirectory();

            Git("init");
            Git("config --local user.name Example");
            Git("config --local user.email user@example.com");
        }

        [TearDown]
        public void TearDown()
        {
            m_WorkingDirectory.Dispose();
        }


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
        public void Files_returns_expected_files()
        {
            // ARRANGE
            CreateFile("file1.txt");
            CreateFile("file2.txt");
            CreateFile("directory1/file3.txt");
            CreateFile("directory2/DIRECTORY3/file4.txt");

            Git("add .");
            Git("commit -m Commit1");

            Git("rev-parse --short HEAD", out var commitId);
            commitId = commitId.Trim();

            // ACT
            using var sut = CreateInstance(m_WorkingDirectory);
            var files = sut.GetFiles("master").ToList();

            // ASSERT
            files
                .Should().HaveCount(4)
                .And.OnlyContain(x => x.Commit.Equals(commitId, StringComparison.OrdinalIgnoreCase))
                .And.Contain(x => x.Path == "file1.txt")
                .And.Contain(x => x.Path == "file2.txt")
                .And.Contain(x => x.Path == "directory1/file3.txt")
                .And.Contain(x => x.Path == "directory2/DIRECTORY3/file4.txt");
        }

        [Test]
        public void Files_returns_expected_files_from_specified_branch()
        {
            // ARRANGE
            var filePath1 = CreateFile("file1.txt");

            Git("add .");
            Git("commit -m Commit1");

            Git("checkout -b some-other-branch");
            File.Delete(filePath1);
            CreateFile("file2.txt");

            Git("add .");
            Git("commit -m Commit2");

            Git("rev-parse --short HEAD", out var commitId);
            commitId = commitId.Trim();

            // ACT
            using var sut = CreateInstance(m_WorkingDirectory);
            var files = sut.GetFiles("some-other-branch").ToList();

            // ASSERT
            files
                .Should().HaveCount(1)
                .And.OnlyContain(x => x.Commit.Equals(commitId, StringComparison.OrdinalIgnoreCase))
                .And.OnlyContain(x => x.Path == "file2.txt");
        }

        [Test]
        public void Files_returns_expected_files_with_expected_content()
        {
            // ARRANGE
            var path = CreateFile("file1.txt", "Line1", "Line2", "", "Line3");
            var expectedContent = File.ReadAllText(path).Replace("\r\n", "\n");

            Git("add .");
            Git("commit -m Commit1");

            // ACT
            using var sut = CreateInstance(m_WorkingDirectory);
            var files = sut.GetFiles("master").ToList();

            // ASSERT
            files
                .Should().ContainSingle()
                .Which.Content.Should().Be(expectedContent);
        }

        //TODO: ensure files of right commit are returned

        protected abstract IGitRepository CreateInstance(string repositoryUrl);

        protected void Git(string command) => Git(command, out _, out _);

        protected void Git(string command, out string stdOut) => Git(command, out stdOut, out _);

        protected void Git(string command, out string stdOut, out string stdErr)
        {
            var startInfo = new ProcessStartInfo()
            {
                FileName = "git",
                Arguments = command,
                WorkingDirectory = m_WorkingDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            var stdOutBuilder = new StringBuilder();
            var stdErrBuilder = new StringBuilder();

            var process = Process.Start(startInfo);

            process.ErrorDataReceived += (s, e) =>
            {
                if (e.Data is string)
                    stdErrBuilder.AppendLine(e.Data);
            };

            process.OutputDataReceived += (s, e) =>
            {
                if (e.Data is string)
                    stdOutBuilder.AppendLine(e.Data);
            };

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit();

            process.CancelErrorRead();
            process.CancelOutputRead();

            stdOut = stdOutBuilder.ToString();
            stdErr = stdErrBuilder.ToString();


            TestContext.Out.WriteLine("--------------------------------");
            TestContext.Out.WriteLine($"Begin Command 'git {command}'");
            TestContext.Out.WriteLine("--------------------------------");
            TestContext.Out.WriteLine("StdOut:");
            TestContext.Out.WriteLine(stdOut);
            TestContext.Out.WriteLine("StdErr:");
            TestContext.Out.WriteLine(stdErr);
            TestContext.Out.WriteLine("--------------------------------");
            TestContext.Out.WriteLine($"End Command 'git {command}'");
            TestContext.Out.WriteLine("--------------------------------");

            if (process.ExitCode != 0)
            {
                throw new Exception($"Command 'git {command}' completed with exit code {process.ExitCode}");
            }
        }

        protected string CreateFile(string relativePath, params string[] content)
        {
            var absolutePath = Path.Combine(m_WorkingDirectory, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(absolutePath));
            File.WriteAllLines(absolutePath, content);

            return absolutePath;
        }
    }
}
