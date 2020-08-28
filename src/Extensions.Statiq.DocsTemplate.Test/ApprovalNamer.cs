using System;
using System.IO;
using ApprovalTests.Namers;

namespace Grynwald.Extensions.Statiq.DocsTemplate.Test
{
    public class ApprovalNamer : UnitTestFrameworkNamer
    {
        private readonly string m_TestCaseName;

        public override string Subdirectory => Path.Combine(base.Subdirectory, "_referenceOutput");

        public override string Name => m_TestCaseName;


        public ApprovalNamer(string testCaseName)
        {
            if (String.IsNullOrWhiteSpace(testCaseName))
                throw new ArgumentException("Value must not be null or whitespace", nameof(testCaseName));
            m_TestCaseName = testCaseName;
        }
    }
}
