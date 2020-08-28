using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Grynwald.Extensions.Statiq.DocsTemplate.Modules;
using NUnit.Framework;
using Statiq.Common;
using Statiq.Testing;
using Statiq.Yaml;

namespace Grynwald.Extensions.Statiq.DocsTemplate.Test.Modules
{
    /// <summary>
    /// Tests for <see cref="ReadDirectoryMetadataFromInputFiles"/>
    /// </summary>
    public class ReadDirectoryMetadataFromInputFilesTest : BaseFixture
    {
        [Test]
        public async Task Execute_returns_input_document_if_no_metadata_files_are_found()
        {
            // ARRANGE
            var input = new TestDocument(path: "/dir/input.md");

            var sut = new ReadDirectoryMetadataFromInputFiles("docs.yml", new ParseYaml());

            // ACT
            var output = await ExecuteAsync(input, sut).SingleAsync();

            // ASSERT
            output.Should().BeSameAs(input);
        }

        [Test]
        public async Task Execute_adds_metadata_from_metadata_documents()
        {
            // ARRANGE
            var inputs = new[]
            {
                new TestDocument(path: "/dir/input.md"),
                new TestDocument(path: "/dir/docs.yml", content: "some-key: some-value"),
            };

            var sut = new ReadDirectoryMetadataFromInputFiles("docs.yml", new ParseYaml());

            // ACT
            var outputs = await ExecuteAsync(inputs, sut);

            // ASSERT
            outputs.Should().HaveCount(2);

            outputs.First().Source.Should().Be("/dir/input.md");
            outputs.First().Should().Contain(x => x.Key == "some-key" && x.Value as string == "some-value");
        }

        [Test]
        public async Task Execute_adds_metadata_from_multiple_metadata_documents()
        {
            // ARRANGE
            var inputs = new[]
            {
                new TestDocument(path: "/dir/input.md"),
                new TestDocument(path: "/dir/docs.yml", content: "some-key: some-value"),
                new TestDocument(path: "/docs.yml", content: "some-other-key: some-other-value"),
            };

            var sut = new ReadDirectoryMetadataFromInputFiles("docs.yml", new ParseYaml());

            // ACT
            var outputs = await ExecuteAsync(inputs, sut);

            // ASSERT
            outputs.Should().HaveCount(3);

            outputs.First().Source.Should().Be("/dir/input.md");
            outputs.First()
                .Should().Contain(x => x.Key == "some-key" && x.Value as string == "some-value")
                .And.Contain(x => x.Key == "some-other-key" && x.Value as string == "some-other-value");
        }

        [Test]
        public async Task More_specific_metadata_documents_override_metadata_from_less_specific_documents()
        {
            // ARRANGE
            var inputs = new[]
            {
                new TestDocument(path: "/dir/input.md"),
                new TestDocument(path: "/dir/docs.yml", content: "some-key: expected-value"),
                new TestDocument(path: "/docs.yml", content: "some-key: overridden-value"),
                new TestDocument(path: "/input.md"),
            };

            var sut = new ReadDirectoryMetadataFromInputFiles("docs.yml", new ParseYaml());

            // ACT
            var outputs = await ExecuteAsync(inputs, sut);

            // ASSERT
            outputs.Should().HaveCount(4);

            outputs.First().Source.Should().Be("/dir/input.md");
            outputs.First().Should().Contain(x => x.Key == "some-key" && x.Value as string == "expected-value");

            outputs.Last().Source.Should().Be("/input.md");
            outputs.Last().Should().Contain(x => x.Key == "some-key" && x.Value as string == "overridden-value");
        }
    }
}
