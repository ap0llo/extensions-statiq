using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Statiq.Common;
using Statiq.Testing;

namespace Grynwald.Extensions.Statiq.TestHelpers
{
    public abstract class TestBase
    {
        protected TestExecutionContext m_TestExecutionContext = null!;

        [SetUp]
        public virtual void SetUp()
        {
            m_TestExecutionContext = new TestExecutionContext();
            m_TestExecutionContext.TestLoggerProvider.ThrowLogLevel = LogLevel.None;
        }


        protected Task<ImmutableArray<TestDocument>> ExecuteAsync(params IModule[] modules) => BaseFixture.ExecuteAsync(m_TestExecutionContext, modules);

        protected Task<ImmutableArray<TestDocument>> ExecuteAsync(TestDocument document, params IModule[] modules) => BaseFixture.ExecuteAsync(document, m_TestExecutionContext, modules);

        protected Task<ImmutableArray<TestDocument>> ExecuteAsync(IEnumerable<TestDocument> documents, params IModule[] modules) => BaseFixture.ExecuteAsync(documents, m_TestExecutionContext, modules);
    }
}
