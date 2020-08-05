using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Grynwald.Extensions.Statiq.Git.Internal;
using Grynwald.Utilities.IO;
using NUnit.Framework;

namespace Grynwald.Extensions.Statiq.Git.Test.Internal
{
    /// <summary>
    /// Tests for <see cref="GitRepository"/>
    /// </summary>
    [TestFixture]
    public class GitRepositoryTest
    {
        private TemporaryDirectory m_WorkingDirectory = null!;

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

        [TestCase("https://github.com/example/example.git", RepositoryKind.Remote)]
        [TestCase(@"C:\some\repo", RepositoryKind.Local)]
        public void Create_returns_expected_GitRepository(string remoteUrl, RepositoryKind expected)
        {
            using var sut = GitRepository.Create(remoteUrl);
            Assert.AreEqual(expected, sut.Kind);
        }

        [TestCase("not-a-url")]
        [TestCase("git@github.com:example/example.git")]
        [TestCase("ssh://github.com/example/example.git")]
        public void Create_throws_ArgumentException_for_unsupported_remote_urls(string remoteUrl)
        {
            Assert.Throws<ArgumentException>(() => GitRepository.Create(remoteUrl));
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
            using var sut = GitRepository.Create(m_WorkingDirectory);
            var branches = sut.Branches.ToList();

            // ASSERT
            Assert.AreEqual(1 + additionalBranches.Length, branches.Count);
            Assert.Contains("master", branches);
            foreach (var branchName in additionalBranches)
            {
                Assert.Contains(branchName, branches);
            }
        }


        private void Git(string command) =>
           Git(command, out _, out _);

        private void Git(string command, out string stdOut) =>
            Git(command, out stdOut, out _);

        private void Git(string command, out string stdOut, out string stdErr)
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
    }
}
