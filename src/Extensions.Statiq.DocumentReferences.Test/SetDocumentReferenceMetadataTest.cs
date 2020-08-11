using System;
using System.Threading.Tasks;
using FluentAssertions;
using Grynwald.Extensions.Statiq.DocumentReferences.Model;
using NuGet.Versioning;
using NUnit.Framework;
using Statiq.Common;
using Statiq.Testing;

namespace Grynwald.Extensions.Statiq.DocumentReferences.Test
{
    /// <summary>
    /// Tests for <see cref="SetDocumentReferenceMetadata" />
    /// </summary>
    public class SetDocumentReferenceMetadataTest : TestBase
    {
        [Test]
        public async Task Execute_adds_expected_metadata()
        {
            // ARRANGE
            var input = new TestDocument();

            var sut = new SetDocumentReferenceMetadata(
                Config.FromValue(new DocumentName("name")),
                Config.FromValue(NuGetVersion.Parse("1.0"))
            );

            // ACT 
            var output = await ExecuteAsync(input, sut).SingleAsync();

            // ASSERT
            output.Should().NotBeNull();

            output.GetDocumentName().Should().NotBeNull().And.Be(new DocumentName("name"));
            output.GetDocumentVersion().Should().NotBeNull().And.Be(NuGetVersion.Parse("1.0"));
            output.GetDocumentIdentity().Should().NotBeNull().And.Be(DocumentIdentity.Parse("name@1.0"));
        }

        [Test]
        public async Task Execute_adds_expected_metadata_from_string_config()
        {
            // ARRANGE
            var input = new TestDocument();

            var sut = new SetDocumentReferenceMetadata("name", "1.0");

            // ACT 
            var output = await ExecuteAsync(input, sut).SingleAsync();

            // ASSERT
            output.Should().NotBeNull();

            output.GetDocumentName().Should().NotBeNull().And.Be(new DocumentName("name"));
            output.GetDocumentVersion().Should().NotBeNull().And.Be(NuGetVersion.Parse("1.0"));
            output.GetDocumentIdentity().Should().NotBeNull().And.Be(DocumentIdentity.Parse("name@1.0"));
        }

        [Test]
        public async Task Execute_throws_DuplicateDocumentIdentityException_when_multiple_documents_have_the_same_identity()
        {
            // ARRANGE
            var inputs = new[] { new TestDocument(), new TestDocument() };

            var sut = new SetDocumentReferenceMetadata("name", "1.0");

            // ACT 
            Func<Task> act = async () => await ExecuteAsync(inputs, sut);

            // ASSERT
            await act.Should().ThrowAsync<DuplicateDocumentIdentityException>();
        }
    }
}
