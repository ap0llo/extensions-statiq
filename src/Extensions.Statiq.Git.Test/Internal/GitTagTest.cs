//Note: Adapted from https://github.com/ap0llo/changelog/blob/9c789d570199480801ea95d57f425b425b5f1964/src/ChangeLog.Test/Git/GitTagTest.cs

using System;
using System.Collections.Generic;
using FluentAssertions;
using Grynwald.Extensions.Statiq.Git.Internal;
using Grynwald.Extensions.Statiq.TestHelpers;
using NUnit.Framework;

namespace Grynwald.Extensions.Statiq.Git.Test.Internal
{
    /// <summary>
    /// Tests for <see cref="GitTag"/>
    /// </summary>
    public class GitTagTest : EqualityTest<GitTag, GitTagTest>, IEqualityTestDataProvider<GitTag>
    {
        public IEnumerable<(GitTag left, GitTag right)> GetEqualTestCases()
        {
            yield return (new GitTag("tag1", new GitId("abc123")), new GitTag("tag1", new GitId("abc123")));
            yield return (new GitTag("tag1", new GitId("abc123")), new GitTag("tag1", new GitId("ABC123")));
        }

        public IEnumerable<(GitTag left, GitTag right)> GetUnequalTestCases()
        {
            yield return (new GitTag("tag1", new GitId("abc123")), new GitTag("tag2", new GitId("abc123")));
            yield return (new GitTag("tag1", new GitId("abc123")), new GitTag("tag1", new GitId("def456")));
        }


        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        [TestCase("\t")]
        public void Name_must_not_be_null_or_whitespace(string name)
        {
            Action act = () => new GitTag(name, new GitId("abc123"));
            act.Should().Throw<ArgumentException>();
        }
    }
}
