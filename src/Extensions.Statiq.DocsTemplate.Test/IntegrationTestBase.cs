using System.IO;
using Grynwald.Utilities.IO;
using NUnit.Framework;

namespace Grynwald.Extensions.Statiq.DocsTemplate.Test
{
    [TestFixture]
    public abstract class IntegrationTestBase
    {
        protected TemporaryDirectory m_OutputDirectory = null!;  // value is set in SetUp(), called by NUnit

        [SetUp]
        public virtual void SetUp()
        {
            m_OutputDirectory = new TemporaryDirectory();
        }

        [TearDown]
        public virtual void TearDown()
        {
            m_OutputDirectory.Dispose();
        }


        protected string GetFullOutputPath(string relativePath) => Path.Combine(m_OutputDirectory, relativePath);
    }
}
