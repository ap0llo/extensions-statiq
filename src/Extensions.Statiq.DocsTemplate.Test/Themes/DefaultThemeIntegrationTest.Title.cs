using System.Threading.Tasks;
using AngleSharp.Dom.Html;
using FluentAssertions;
using Grynwald.Extensions.Statiq.TestHelpers;
using NUnit.Framework;
using Statiq.Common;
using Statiq.Testing;

namespace Grynwald.Extensions.Statiq.DocsTemplate.Test.Themes
{
    public partial class DefaultThemeIntegrationTest
    {
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
