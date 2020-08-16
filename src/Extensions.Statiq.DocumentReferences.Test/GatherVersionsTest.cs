using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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
    /// Tests for <see cref="GatherVersions"/>
    /// </summary>
    public class GatherVersionsTest : TestBase
    {
        private readonly GatherVersions m_Instance;

        public GatherVersionsTest()
        {
            m_Instance = new GatherVersions();
        }

        [Test]
        public async Task Execute_throws_MissingDocumentIdentityException_if_input_documents_have_no_identity_metadata()
        {
            // ARRANGE
            var input = new TestDocument();

            // ACT
            Func<Task> act = async () => await ExecuteAsync(input).SingleAsync();

            // ASSERT
            await act.Should().ThrowAsync<MissingDocumentIdentityException>().WithMessage("*Failed to determine identity for document*");
        }

        [Test]
        public async Task Execute_adds_LatestDocumentVersion_and_LatestVersion_metadata()
        {
            // ARRANGE
            var input = GetInputsFromIdentities($"name@1.0", "name@2.0", "other-name@3.0");

            // ACT
            var output = (await ExecuteAsync(input)).First();

            // ASSERT
            output
                .Should().ContainSingle(x => x.Key == DocumentReferenceKeys.LatestDocumentVersion)
                .Which.Value
                    .Should().BeAssignableTo<NuGetVersion>()
                .Which
                    .Should().Be(NuGetVersion.Parse("2.0"));

            output
                .Should().ContainSingle(x => x.Key == DocumentReferenceKeys.LatestVersion)
                .Which.Value
                    .Should().BeAssignableTo<NuGetVersion>()
                .Which
                    .Should().Be(NuGetVersion.Parse("3.0"));
        }


        [Test]
        public async Task Execute_uses_Config_to_retrieve_document_identity()
        {
            // ARRANGE
            var input = GetInputsFromIdentities("id1@1.0");

            var sut = new GatherVersions()
                .WithDocumentIdentity(Config.FromValue(DocumentIdentity.Parse("id2@2.0")));

            // ACT
            var output = await ExecuteAsync(input, sut).SingleAsync();

            // ASSERT
            output
                .Should().ContainSingle(x => x.Key == DocumentReferenceKeys.LatestDocumentVersion)
                .Which.Value
                    .Should().BeAssignableTo<NuGetVersion>()
                .Which
                    .Should().Be(NuGetVersion.Parse("2.0"));
        }

        [Test]
        public async Task Execute_adds_AllDocumentVersions_metadata()
        {
            // ARRANGE
            var inputs = GetInputsFromIdentities("name1@1.0", "name2@2.0", "name1@3.0", "name1@2.0");

            // ACT 
            var output = (await ExecuteAsync(inputs)).First();

            // ASSERT
            output
                .Should().ContainSingle(x => x.Key == DocumentReferenceKeys.AllDocumentVersions)
                .Which.Value
                    .Should().BeAssignableTo<IReadOnlyList<IMetadata>>()
                .Which
                    .Should().HaveCount(3);

            var otherVersions = output.GetChildren(DocumentReferenceKeys.AllDocumentVersions);


            foreach (var item in otherVersions)
            {
                item
                    .Should().Contain(x => x.Key == DocumentReferenceKeys.DocumentName)
                    .And.Contain(x => x.Key == DocumentReferenceKeys.DocumentVersion)
                    .And.Contain(x => x.Key == DocumentReferenceKeys.DocumentIdentity);

                item.GetDocumentName()
                    .Should().NotBeNull()
                    .And.Be(new DocumentName("name1"));
            }

            otherVersions[0].GetDocumentVersion().Should().Be(NuGetVersion.Parse("1.0"));
            otherVersions[0].GetDocumentIdentity().Should().Be(DocumentIdentity.Parse("name1@1.0"));
            otherVersions[0].GetDocumentReference().Should().Be(DocumentReference.Parse("ref:name1@1.0"));
            otherVersions[1].GetDocumentVersion().Should().Be(NuGetVersion.Parse("2.0"));
            otherVersions[1].GetDocumentIdentity().Should().Be(DocumentIdentity.Parse("name1@2.0"));
            otherVersions[1].GetDocumentReference().Should().Be(DocumentReference.Parse("ref:name1@2.0"));
            otherVersions[2].GetDocumentVersion().Should().Be(NuGetVersion.Parse("3.0"));
            otherVersions[2].GetDocumentIdentity().Should().Be(DocumentIdentity.Parse("name1@3.0"));
            otherVersions[2].GetDocumentReference().Should().Be(DocumentReference.Parse("ref:name1@3.0"));
        }

        private Task<ImmutableArray<TestDocument>> ExecuteAsync(params TestDocument[] inputs) => ExecuteAsync(inputs, m_Instance);

        private TestDocument[] GetInputsFromIdentities(params string[] identities)
        {
            return identities.Select(identity =>
            {
                var input = new TestDocument();
                input.TestMetadata[DocumentReferenceKeys.DocumentIdentity] = DocumentIdentity.Parse(identity);
                return input;
            })
            .ToArray();
        }
    }
}
