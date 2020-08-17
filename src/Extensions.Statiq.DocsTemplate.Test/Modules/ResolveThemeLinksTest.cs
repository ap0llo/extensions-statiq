using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom.Html;
using FluentAssertions;
using Grynwald.Extensions.Statiq.DocsTemplate.Modules;
using NUnit.Framework;
using Statiq.Common;
using Statiq.Testing;

namespace Grynwald.Extensions.Statiq.DocsTemplate.Test.Modules
{
    /// <summary>
    /// Tests for <see cref="ResolveThemeLinks"/>
    /// </summary>
    public class ResolveThemeLinksTest : BaseFixture
    {
        [TestCase("theme:unknown-page")]
        public async Task Execute_leaves_unresolvable_theme_links_unchanged(string themeLink)
        {
            // ARRANGE
            var source = (NormalizedPath)"/source.html";
            var destination = (NormalizedPath)"directory/destination.html";

            var input = new TestDocument(source, destination, $@"<html>
                        <head>
                        </head>
                        <body>
                            <a href=""{themeLink}"">Link</a>
                            <script src=""{themeLink}""></script>
                        </body>
                    </html>");

            // ACT
            var sut = new ResolveThemeLinks();

            // ASSERT
            var output = await ExecuteAsync(input, sut).SingleAsync();
            var html = await output.ParseAsHtmlAsync();

            html.QuerySelectorAll("a")
                .Should().ContainSingle()
                .Which.Should().BeAssignableTo<IHtmlAnchorElement>()
                .Which.GetAttribute("href").Should().Be(themeLink);

            html.QuerySelectorAll("script")
                 .Should().ContainSingle()
                 .Which.Should().BeAssignableTo<IHtmlScriptElement>()
                 .Which.GetAttribute("src").Should().Be(themeLink);
        }

        [Test]
        public async Task Execute_resolves_asset_links()
        {
            // ARRANGE
            var source = (NormalizedPath)"/source.html";
            var destination = (NormalizedPath)"directory/destination.html";

            var input = new TestDocument(source, destination, $@"<html>
                        <head>
                            <link rel=""stylesheet"" href=""theme:assets/some-asset.css"" />
                        </head>
                        <body>
                            <script src=""theme:assets/some-script.js""></script>
                        </body>
                    </html>");

            // ACT
            var sut = new ResolveThemeLinks();

            // ASSERT
            var output = await ExecuteAsync(input, sut).SingleAsync();
            var html = await output.ParseAsHtmlAsync();

            html.QuerySelectorAll("link")
                .Should().ContainSingle()
                .Which.Should().BeAssignableTo<IHtmlLinkElement>()
                .Which.GetAttribute("href").Should().Be("./../assets/some-asset.css");

            html.QuerySelectorAll("script")
                .Should().ContainSingle()
                .Which.Should().BeAssignableTo<IHtmlScriptElement>()
                .Which.GetAttribute("src").Should().Be("./../assets/some-script.js");
        }

    }
}
