using System.Linq;
using Grynwald.Extensions.Statiq.Git.Internal;
using NUnit.Framework;

namespace Grynwald.Extensions.Statiq.Git.Test.Internal
{
    [TestFixture]
    public class LocalGitRepositoryTest : GitRepositoryTestBase
    {
        protected override IGitRepository CreateInstance(string repositoryUrl) => new LocalGitRepository(repositoryUrl);
    }
}
