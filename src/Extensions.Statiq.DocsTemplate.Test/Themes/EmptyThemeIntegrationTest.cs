﻿using System.Threading.Tasks;
using ApprovalTests.Reporters;
using FluentAssertions;
using NUnit.Framework;
using Statiq.Common;
using Statiq.Testing;

namespace Grynwald.Extensions.Statiq.DocsTemplate.Test.Themes
{
    /// <summary>
    /// Rending tests for "empty" theme
    /// </summary>
    [UseReporter(typeof(DiffReporter))]
    public class EmptyThemeIntegrationTest : ThemeIntegrationTestBase
    {
        [Test]
        public async Task Rendering_produces_the_expected_output()
        {
            // ARRANGE
            var testCaseName = $"{nameof(EmptyThemeIntegrationTest)}.{nameof(Rendering_produces_the_expected_output)}";

            var bootstrapper = CreateBootstrapper(DocsTemplateThemeNames.Empty, "<p>Some Content</p>")
                .AddSetting(DocsTemplateKeys.SiteName, "Test Site Name");

            // ACT
            var result = await bootstrapper.RunTestAsync();

            // ASSERT
            result.ExitCode.Should().Be(0);
            Approve(testCaseName);
        }
    }
}
