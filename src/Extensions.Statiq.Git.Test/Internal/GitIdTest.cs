//Note: Adapted from https://github.com/ap0llo/changelog/blob/9c789d570199480801ea95d57f425b425b5f1964/src/ChangeLog.Test/Git/GitIdTest.cs

using System;
using System.Collections.Generic;
using Grynwald.Extensions.Statiq.Git.Internal;
using Grynwald.Extensions.Statiq.TestHelpers;
using NUnit.Framework;

namespace Grynwald.Extensions.Statiq.Git.Test.Internal
{
    public class GitIdTest : EqualityTest<GitId, GitIdTest>, IEqualityTestDataProvider<GitId>
    {
        public IEnumerable<(GitId left, GitId right)> GetEqualTestCases()
        {
            yield return (new GitId("8BADF00D"), new GitId("8BADF00D"));
            yield return (new GitId("8badF00d"), new GitId("8BADF00D"));

        }

        public IEnumerable<(GitId left, GitId right)> GetUnequalTestCases()
        {
            yield return (new GitId("abc123"), new GitId("def456"));
        }


        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("\t")]
        [TestCase("not-a-commit-id")]
        [TestCase("8BADF00D ")]
        [TestCase("  8BADF00D")]
        public void Constructor_throws_ArgumentException_if_input_is_not_a_valid_git_object_id(string id)
        {
            var ex = Assert.Throws<ArgumentException>(() => new GitId(id));
            Assert.NotNull(ex);
            Assert.AreEqual("id", ex!.ParamName);
        }
    }
}
