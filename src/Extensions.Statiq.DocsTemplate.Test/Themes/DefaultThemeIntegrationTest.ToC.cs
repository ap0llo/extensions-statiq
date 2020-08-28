using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Statiq.Common;
using Statiq.Testing;

namespace Grynwald.Extensions.Statiq.DocsTemplate.Test.Themes
{
    public partial class DefaultThemeIntegrationTest
    {
        [Test]
        public async Task ToC_is_not_included_if_input_document_has_no_ToC_metadata()
        {
            // ARRANGE
            var metadata = new TestMetadata();
            var bootstrapper = CreateBootstrapper(DocsTemplateThemeNames.Default, "<p>Some Content</p>", metadata);

            // ACT
            var result = await bootstrapper.RunTestAsync();

            // ASSERT
            result.ExitCode.Should().Be(0);

            GetHtmlOutput()
                .QuerySelectorAll("#toc")
                .Should().BeEmpty();
        }

        [Test]
        [TestCase(true)]
        [TestCase(null)]
        public async Task ToC_is_included_if_input_document_has_ToC_metadata(bool? showToc)
        {
            // ARRANGE
            var metadata = GetMetadataWithToC(
                GetTocEntry("Heading 1", "some-id"),
                GetTocEntry("Heading 2", "some-other-id")
            );

            if (showToc.HasValue)
                metadata[DocsTemplateKeys.ShowToc] = showToc.Value;

            var bootstrapper = CreateBootstrapper(DocsTemplateThemeNames.Default, "<p>Some Content</p>", metadata);

            // ACT
            var result = await bootstrapper.RunTestAsync();

            // ASSERT
            result.ExitCode.Should().Be(0);

            var items = GetHtmlOutput().QuerySelectorAll("#toc .toc-content li a");

            items.Should().HaveCount(2);

            items.Select(x => x.GetAttribute("href")).Should().Equal(new[] { "#some-id", "#some-other-id" });
            items.Select(x => x.TextContent).Should().Equal(new[] { "Heading 1", "Heading 2" });
        }

        [Test]
        public async Task ToC_is_not_included_if_ShowToc_is_false()
        {
            // ARRANGE
            var metadata = GetMetadataWithToC(
                GetTocEntry("Heading 1", "some-id"),
                GetTocEntry("Heading 2", "some-other-id")
            );

            metadata[DocsTemplateKeys.ShowToc] = false;

            var bootstrapper = CreateBootstrapper(DocsTemplateThemeNames.Default, "<p>Some Content</p>", metadata);

            // ACT
            var result = await bootstrapper.RunTestAsync();

            // ASSERT
            result.ExitCode.Should().Be(0);

            GetHtmlOutput()
                .QuerySelectorAll("#toc")
                .Should().BeEmpty();
        }

        [Test]
        public async Task ToC_child_items_are_included_in_the_output()
        {
            // ARRANGE
            var metadata = GetMetadataWithToC(
                GetTocEntry("Heading 1", "some-id",
                    GetTocEntry("Heading 2", "child-item-1"),
                    GetTocEntry("Heading 3", "child-item-2"))
            );

            var bootstrapper = CreateBootstrapper(DocsTemplateThemeNames.Default, "<p>Some Content</p>", metadata);

            // ACT
            var result = await bootstrapper.RunTestAsync();

            // ASSERT
            result.ExitCode.Should().Be(0);

            var html = GetHtmlOutput();

            var items = html.QuerySelectorAll("#toc > .toc-content > ul > li > a");
            items.Should().ContainSingle();

            var childItems = html.QuerySelectorAll("#toc > .toc-content > ul > li > ul > li > a");
            childItems.Select(x => x.GetAttribute("href")).Should().Equal(new[] { "#child-item-1", "#child-item-2" });
            childItems.Select(x => x.TextContent).Should().Equal(new[] { "Heading 2", "Heading 3" });
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public async Task A_ToC_entries_is_not_rendered_as_link_if_the_heading_id_is_null_or_empty(string id)
        {
            // ARRANGE
            var metadata = GetMetadataWithToC(
                GetTocEntry("Heading 1", id)
            );

            var bootstrapper = CreateBootstrapper(DocsTemplateThemeNames.Default, "<p>Some Content</p>", metadata);

            // ACT
            var result = await bootstrapper.RunTestAsync();

            // ASSERT
            result.ExitCode.Should().Be(0);

            var html = GetHtmlOutput();

            html.QuerySelectorAll("#toc .toc-content ul li")
                .Should().ContainSingle()
                .Which.TextContent.Trim().Should().Be("Heading 1");

            html.QuerySelectorAll("#toc .toc-content ul li a")
                .Should().BeEmpty();
        }


        private TestMetadata GetMetadataWithToC(params IDocument[] tocEntries)
        {
            return new TestMetadata()
            {
                {DocsTemplateKeys.ToC, tocEntries }
            };
        }

        private TestDocument GetTocEntry(string title, string? headingId, params IDocument[] items)
        {
            return new TestDocument(new TestMetadata()
            {
                { DocsTemplateKeys.TocTitle, title },
                { DocsTemplateKeys.TocHeadingId, headingId },
                { DocsTemplateKeys.TocItems, items },
            });
        }
    }
}
