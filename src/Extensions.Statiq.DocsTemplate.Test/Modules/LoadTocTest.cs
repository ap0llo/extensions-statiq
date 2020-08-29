using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Grynwald.Extensions.Statiq.DocsTemplate.Modules;
using Grynwald.Extensions.Statiq.TestHelpers;
using NUnit.Framework;
using Statiq.Common;
using Statiq.Testing;

namespace Grynwald.Extensions.Statiq.DocsTemplate.Test.Modules
{
    /// <summary>
    /// Tests for <see cref="LoadToc"/>
    /// </summary>
    public class LoadTocTest : TestBase
    {
        [Test]
        [TestCase("<h2>Heading Text</h2>", "Heading Text", null)]
        [TestCase("<H2>Heading Text</H2>", "Heading Text", null)]
        [TestCase("<h2>Heading <em>Text</em></h2>", "Heading Text", null)]
        [TestCase("<H2>Heading <em>Text</em></H2>", "Heading Text", null)]
        [TestCase("<h2 id=\"my-id\">Heading Text</h2>", "Heading Text", "my-id")]
        public async Task Execute_adds_expected_ToC_metadata(string heading, string expectedTitle, string expectedHeadingId)
        {
            // ARRANGE
            var input = new TestDocument(
                content: $@"<html>
                                <head>
                                    {heading}
                                </head>
                                <body>                         
                                </body>
                            </html>"
            );

            var sut = new LoadToc();

            // ACT 
            var output = await ExecuteAsync(input, sut).SingleAsync();

            // ASSERT
            output.Keys.Should().Contain(DocsTemplateKeys.ToC);
            var toc = output.Get<IEnumerable<IMetadata>>(DocsTemplateKeys.ToC);
            toc.Should().ContainSingle();

            toc.Single().Keys.Should().Contain(DocsTemplateKeys.TocTitle);
            toc.Single().GetString(DocsTemplateKeys.TocTitle).Should().Be(expectedTitle);

            toc.Single().GetString(DocsTemplateKeys.TocHeadingId).Should().Be(expectedHeadingId);
        }

        [Test]
        public async Task Execute_adds_expected_entries_for_multi_level_ToCs()
        {
            // ARRANGE
            var input = new TestDocument(
                content: $@"<html>
                                <head>
                                    <h2>Heading 2.1</h2>
                                        <h3>Heading 3.1</h3>
                                        <h3>Heading 3.2</h3>
                                            <h4>Heading 4.1</h4>
                                    <h2>Heading 2.2</h2>                        
                                </head>
                                <body>                         
                                </body>
                            </html>"
            );

            var sut = new LoadToc();

            // ACT 
            var output = await ExecuteAsync(input, sut).SingleAsync();

            // ASSERT
            var toc = output.Get<IEnumerable<IMetadata>>(DocsTemplateKeys.ToC);

            toc
                .Should().NotBeNull()
                .And.HaveCount(2);

            toc.First().GetString(DocsTemplateKeys.TocTitle)
                .Should().Be("Heading 2.1");

            var level2Entries = toc.First().GetDocuments(DocsTemplateKeys.TocItems);

            level2Entries
                .Should().NotBeNull()
                .And.HaveCount(2);

            level2Entries.First().GetString(DocsTemplateKeys.TocTitle)
                .Should().Be("Heading 3.1");

            level2Entries.Last().GetString(DocsTemplateKeys.TocTitle)
                .Should().Be("Heading 3.2");

            toc.Last().GetString(DocsTemplateKeys.TocTitle)
                .Should().Be("Heading 2.2");

            toc.Last().GetDocuments(DocsTemplateKeys.TocItems)
                .Should().NotBeNull()
                .And.BeEmpty();
        }

        [Test]
        public async Task Execute_does_not_add_metadata_if_input_cannot_be_parsed()
        {
            // ARRANGE
            var input = new TestDocument(content: "not-html");

            var sut = new LoadToc();

            // ACT 
            var output = await ExecuteAsync(input, sut).SingleAsync();

            // ASSERT
            output.Should().BeSameAs(input);
        }

        [Test]
        // no h2 heading
        [TestCase("<h3>Heading 3.1</h3>")]
        // h3 heading before the first h2 heading
        [TestCase("<h3>Heading 3.1</h3> <h2>Heading 3.1</h2>")]
        public async Task Execute_does_not_add_metadata_if_input_contains_unexpected_order_of_headings(string headings)
        {
            // ARRANGE
            var input = new TestDocument(
                content: $@"<html>
                                <head>
                                        
                                </head>
                                <body>                         
                                </body>
                            </html>"
            );

            var sut = new LoadToc();

            // ACT 
            var output = await ExecuteAsync(input, sut).SingleAsync();

            // ASSERT
            output.Should().BeSameAs(input);
        }
    }
}
