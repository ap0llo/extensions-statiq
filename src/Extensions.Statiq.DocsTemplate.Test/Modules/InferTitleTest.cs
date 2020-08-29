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
    /// Tests for <see cref="InferTitle"/>
    /// </summary>
    public class InferTitleTest : TestBase
    {
        [Test]
        public async Task Title_is_null_if_document_has_no_heading()
        {
            // ARRANGE
            var input = new TestDocument(
                @"<html>
                    <head>
                    </head>
                    <body>                           
                    </body>
                  </html>");

            var sut = new InferTitle();

            // ACT 
            var output = await ExecuteAsync(input, sut).SingleAsync();

            // ASSERT
            output.Keys.Should().NotContain(DocsTemplateKeys.Title);
        }

        [Test]
        public async Task Title_is_not_overwritten_if_document_already_has_a_title()
        {
            // ARRANGE
            var input = new TestDocument(
                @"<html>
                    <head>
                    </head>
                    <body>
                        <h1>Some Heading</h1>
                    </body>
                </html>");

            input.TestMetadata[DocsTemplateKeys.Title] = "Document Title";

            var sut = new InferTitle();

            // ACT 
            var output = await ExecuteAsync(input, sut).SingleAsync();

            // ASSERT
            output.Keys.Should().Contain(DocsTemplateKeys.Title);
            output.GetString("Title").Should().Be("Document Title");
        }

        [Test]
        public async Task Title_is_content_of_first_h1_heading()
        {
            // ARRANGE
            var input = new TestDocument(
                @"<html>
                    <head>
                    </head>
                    <body>
                        <h1>Some Heading</h1>
                        <h2>Some Other Heading</h2>
                    </body>
                </html>");

            var sut = new InferTitle();

            // ACT 
            var output = await ExecuteAsync(input, sut).SingleAsync();

            // ASSERT
            output.Keys.Should().Contain(DocsTemplateKeys.Title);
            output.GetString("Title").Should().Be("Some Heading");
        }

        [Test]
        public async Task Title_is_content_of_the_first_heading_of_the_highest_level([Range(1, 6)] int level)
        {
            // ARRANGE
            var input = new TestDocument(
                $@"<html>
                    <head>
                    </head>
                    <body>
                        <h{level}>Some Heading</h{level}>
                        <h{level}>Some Other Heading</h{level}>
                    </body>
                </html>");

            var sut = new InferTitle();

            // ACT 
            var output = await ExecuteAsync(input, sut).SingleAsync();

            // ASSERT
            output.Keys.Should().Contain(DocsTemplateKeys.Title);
            output.GetString("Title").Should().Be("Some Heading");
        }
    }
}
