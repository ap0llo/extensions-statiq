using System.Threading.Tasks;
using AngleSharp.Dom.Html;
using ApprovalTests.Reporters;
using FluentAssertions;
using NUnit.Framework;
using Statiq.Common;
using Statiq.Testing;

namespace Grynwald.Extensions.Statiq.DocsTemplate.Test.Themes
{
    /// <summary>
    /// Rending tests for "Default" theme
    /// </summary>
    [UseReporter(typeof(DiffReporter))]
    public class DefaultThemeIntegrationTest : ThemeIntegrationTestBase
    {
        [Test]
        public async Task Rendering_produces_the_expected_output()
        {
            // ARRANGE
            var testCaseName = $"{nameof(DefaultThemeIntegrationTest)}.{nameof(Rendering_produces_the_expected_output)}";

            var bootstrapper = CreateBootstrapper(DocsTemplateThemeNames.Default, "<p>Some Content</p>")
                .AddSetting(DocsTemplateKeys.SiteName, "Test Site Name");

            // ACT
            var result = await bootstrapper.RunTestAsync();

            // ASSERT
            result.ExitCode.Should().Be(0);
            Approve(testCaseName);
        }

        [TestCase(null, null, "")]
        [TestCase("", "", "")]
        [TestCase(null, "Page Title", "Page Title")]
        [TestCase("", "Page Title", "Page Title")]
        [TestCase("Site Name", null, "Site Name")]
        [TestCase("Site Name", "", "Site Name")]
        [TestCase("Site Name", "Page Title", "Page Title - Site Name")]
        public async Task Output_includes_the_expected_title(string siteName, string pageTitle, string expectedTitle)
        {
            // ARRANGE            
            var bootstrapper = CreateBootstrapper(DocsTemplateThemeNames.Default, "<p>Some Content</p>")
                .AddSetting(DocsTemplateKeys.Title, pageTitle)
                .AddSetting(DocsTemplateKeys.SiteName, siteName);

            // ACT
            var result = await bootstrapper.RunTestAsync();

            // ASSERT
            result.ExitCode.Should().Be(0);

            var output = LoadOutput().ParseAsHtml();

            output.QuerySelectorAll("title")
                .Should().ContainSingle()
                .Which.Should().BeAssignableTo<IHtmlTitleElement>()
                .Which.Text.Should().Be(expectedTitle);
        }
    }
}
