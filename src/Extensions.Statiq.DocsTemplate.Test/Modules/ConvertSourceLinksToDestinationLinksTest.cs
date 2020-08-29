using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom.Html;
using FluentAssertions;
using Grynwald.Extensions.Statiq.DocsTemplate.Modules;
using Grynwald.Extensions.Statiq.TestHelpers;
using NUnit.Framework;
using Statiq.Testing;

namespace Grynwald.Extensions.Statiq.DocsTemplate.Test.Modules
{
    /// <summary>
    /// Tests for <see cref="ConvertSourceLinksToDestinationLinks"/>
    /// </summary>
    public class ConvertSourceLinksToDestinationLinksTest : TestBase
    {
        [Test]
        // Relative links
        [TestCase("/source1.html", "destination1.html", "/source2.html", "destination2.html", "source2.html", "destination2.html")]
        [TestCase("/dir1/source1.html", "destination1.html", "/dir2/source2.html", "destination2.html", "../dir2/source2.html", "destination2.html")]
        [TestCase("/source1.html", "dir1/destination1.html", "/source2.html", "destination2.html", "source2.html", "../destination2.html")]
        // Links to specific sections in the target page
        [TestCase("/source1.html", "destination1.html", "/source2.html", "destination2.html", "source2.html#my-heading", "destination2.html#my-heading")]
        // Links within the same page
        [TestCase("/source1.html", "destination1.html", "/source2.html", "destination2.html", "#my-heading", "#my-heading")]
        // Absolute links between documents
        [TestCase("/source1.html", "destination1.html", "/source2.html", "destination2.html", "/source2.html", "destination2.html")]
        // Absolute uris
        [TestCase("/source1.html", "destination1.html", "/source2.html", "destination2.html", "https://example.com", "https://example.com")]
        public async Task Execute_replaces_links_between_source_paths_with_links_between_target_paths(string source1, string destination1, string source2, string destination2, string link, string expectedResolvedPath)
        {
            // ARRANGE      
            var input1 = new TestDocument(
                source: source1,
                destination: destination1,
                content: $@"<html>
                                <head>
                                </head>
                                <body>
                                    <a href=""{link}"">Link</a>
                                </body>
                            </html>");

            var input2 = new TestDocument(
                source: source2,
                destination: destination2,
                content: "");

            var input = new[] { input1, input2 };

            var sut = new ConvertSourceLinksToDestinationLinks();

            // ACT 
            var output = await ExecuteAsync(input, sut);

            // ASSERT
            output.Should().HaveCount(2);
            var output1 = output.First();

            var html = await output1.ParseAsHtmlAsync();

            html.QuerySelectorAll("a")
                .Should().ContainSingle("there should be a single <a/> element")
                .Which.Should().BeAssignableTo<IHtmlAnchorElement>()
                .Which.GetAttribute("href")
                .Should().Be(expectedResolvedPath);
        }
    }
}
