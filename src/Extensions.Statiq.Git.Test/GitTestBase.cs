﻿using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Grynwald.Utilities.IO;
using NUnit.Framework;

namespace Grynwald.Extensions.Statiq.Git.Test
{
    public abstract class GitTestBase
    {
        protected TemporaryDirectory m_WorkingDirectory = null!;


        [SetUp]
        public virtual void SetUp()
        {
            m_WorkingDirectory = new TemporaryDirectory();

            Git("init");
            Git("config --local user.name Example");
            Git("config --local user.email user@example.com");
        }

        [TearDown]
        public virtual void TearDown()
        {
            m_WorkingDirectory.Dispose();
        }


        protected void GitAdd(string path = ".") => Git($@"add ""{path}""");

        protected string GitCommit(string message = "Commit", bool allowEmtpy = false)
        {
            Git($@"commit -m ""{message}"" {(allowEmtpy ? "--allow-empty" : "")} ");

            Git("rev-parse --short HEAD", out var commitId);
            commitId = commitId.Trim();

            return commitId;
        }

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
